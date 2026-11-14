using System.Collections.Generic;
using FitaSemEtiqueta.Core;
using Godot;

namespace FitaSemEtiqueta.Player;

/// <summary>
/// First-person movement: WASD to walk, mouse to look.
/// Loose objects in the way get kicked.
/// </summary>
public partial class PlayerController : CharacterBody3D
{
    [Export] public float WalkSpeed { get; set; } = 2.0f;

    /// <summary>
    /// Radians of rotation per pixel of mouse movement, before the player's
    /// own sensitivity setting is applied.
    /// </summary>
    [Export] public float MouseSensitivity { get; set; } = 0.002f;

    /// <summary>
    /// How hard the player knocks loose objects away: the object leaves at this
    /// multiple of the speed the player walked into it.
    /// </summary>
    [Export] public float KickStrength { get; set; } = 1.8f;

    // Just short of straight up/down, so the view never flips over.
    private static readonly float MaxPitch = Mathf.DegToRad(89);

    // Upward part of a kick, as a fraction of its forward part: a small hop off the floor.
    private const float KickLift = 0.2f;

    // Tumble given by a kick: radians per second of spin for each metre per second of kick.
    private const float KickSpin = 4f;

    // Physics layer 1: walls, floor and furniture.
    private const uint WorldMask = 1;

    // Height above the player's feet the reach check starts from: above the floor, below a low step.
    private const float KickReachHeight = 0.1f;

    private Node3D _head = null!;
    private PlayerBody _body = null!;
    private readonly HashSet<RigidBody3D> _kicked = new();
    private float _eyeHeight;
    private float _yaw;
    private float _pitch;

    public override void _Ready()
    {
        _head = GetNode<Node3D>("Head");
        _body = GetNode<PlayerBody>("Body");
        _eyeHeight = _head.Position.Y;
        _yaw = GlobalRotation.Y;

        // The body moves on physics ticks and the engine smooths it between them.
        // The view has to follow the mouse on every frame instead, so the head is
        // detached from the body and placed by hand in _Process
        // (first-person pattern from the Godot physics interpolation docs).
        // The visible body turns with the view too: turning on physics ticks,
        // the legs would trail behind the camera when looking down and turning fast.
        foreach (Node3D part in new[] { _head, _body })
        {
            part.TopLevel = true;
            part.PhysicsInterpolationMode = PhysicsInterpolationModeEnum.Off;
        }

        Input.MouseMode = Input.MouseModeEnum.Captured;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventMouseMotion motion && Input.MouseMode == Input.MouseModeEnum.Captured)
        {
            float sensitivity = MouseSensitivity * Settings.Instance.MouseSensitivity;
            _yaw -= motion.ScreenRelative.X * sensitivity;
            _pitch = Mathf.Clamp(_pitch - motion.ScreenRelative.Y * sensitivity, -MaxPitch, MaxPitch);
        }
    }

    public override void _Process(double delta)
    {
        Vector3 feet = GetGlobalTransformInterpolated().Origin;
        _head.GlobalPosition = feet + Vector3.Up * _eyeHeight;
        _head.Rotation = new Vector3(_pitch, _yaw, 0);
        PlaceBody(feet);
    }

    private void PlaceBody(Vector3 feet)
    {
        _body.GlobalTransform = new Transform3D(new Basis(Vector3.Up, _yaw), feet);
    }

    public override void _PhysicsProcess(double delta)
    {
        // The body turns left/right only; looking up/down is the head alone.
        Rotation = new Vector3(0, _yaw, 0);

        Vector3 velocity = Velocity;

        if (!IsOnFloor())
        {
            velocity += GetGravity() * (float)delta;
        }

        Vector2 input = Input.GetVector("move_left", "move_right", "move_forward", "move_back");
        Vector3 direction = Transform.Basis * new Vector3(input.X, 0, input.Y);
        velocity.X = direction.X * WalkSpeed;
        velocity.Z = direction.Z * WalkSpeed;

        Velocity = velocity;
        MoveAndSlide();

        // Also placed here, not only in _Process: when frames run slower than physics ticks,
        // the feet would otherwise kick and splash from where the player was a tick ago.
        PlaceBody(GlobalPosition);
        KickLooseObjects();

        // What the player really moved: walking into a wall, the legs stand still.
        _body.Walk(GlobalBasis.Inverse() * GetRealVelocity(), delta);
    }

    /// <summary>
    /// The player's body does not collide with loose objects: it would get stuck on
    /// them, or climb them. They are knocked away instead, as soon as a foot touches
    /// them: an object let go at chest height falls through the body untouched,
    /// and one between the feet is stepped over.
    /// </summary>
    private void KickLooseObjects()
    {
        _kicked.Clear();
        foreach (Area3D foot in _body.Feet)
        {
            foreach (Node3D node in foot.GetOverlappingBodies())
            {
                if (node is RigidBody3D body && _kicked.Add(body))
                {
                    Kick(body, foot.GlobalPosition);
                }
            }
        }
    }

    /// <summary>Knocks a loose object away from the foot (at the ankle) that touched it.</summary>
    private void Kick(RigidBody3D body, Vector3 ankle)
    {
        // The model stands behind the camera, so walking backwards the heels can reach
        // through a thin wall. Nothing is kicked that the player could not reach.
        var reach = PhysicsRayQueryParameters3D.Create(GlobalPosition + Vector3.Up * KickReachHeight, body.GlobalPosition, WorldMask);
        if (GetWorld3D().DirectSpaceState.IntersectRay(reach).Count > 0)
        {
            return;
        }

        Vector3 away = body.GlobalPosition - ankle;
        away.Y = 0; // Standing over an object must not press it into the floor.
        if (away.IsZeroApprox())
        {
            return;
        }
        away = away.Normalized();

        // Velocity is what the player really moved: zero against a wall,
        // so an object caught between player and wall is not hammered into it.
        float closingSpeed = (Velocity - body.LinearVelocity).Dot(away);
        if (closingSpeed <= 0)
        {
            return; // The player is not walking into it, or it is already faster.
        }

        float kickSpeed = closingSpeed * KickStrength;
        body.ApplyCentralImpulse((away + Vector3.Up * KickLift) * kickSpeed * body.Mass);

        // Topples forward, as if hit above its middle. Without the spin a can slides away standing up.
        body.AngularVelocity = Vector3.Up.Cross(away) * kickSpeed * KickSpin;
    }
}
