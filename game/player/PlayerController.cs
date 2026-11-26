using System;
using System.Collections.Generic;
using FitaSemEtiqueta.Core;
using FitaSemEtiqueta.Interaction;
using Godot;

namespace FitaSemEtiqueta.Player;

/// <summary>
/// First-person movement: WASD to walk, mouse to look.
/// Loose objects in the way get kicked.
/// Can stop to watch something (the test TV): bends to it and looks at it until the player moves on.
/// </summary>
public partial class PlayerController : CharacterBody3D, IViewer
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
    private Interactor _interactor = null!;
    private readonly HashSet<RigidBody3D> _kicked = new();
    private float _eyeHeight;
    private float _yaw;
    private float _pitch;

    // Watching: where the feet go and how deep the body bends for the eyes to be where asked, which way
    // the view turns, how far the mouse has moved it from there, and who to tell when it stops.
    private const float WatchTime = 0.6f;
    private static readonly Vector2 WatchLookRange = new(0.3f, 0.2f);
    private bool _watching;
    private float _watchBlend;
    private float _watchDepth;
    private Vector3 _watchFeet;
    private float _watchYaw;
    private float _watchPitch;
    private Vector2 _watchLook;
    private Action? _stopped;
    private Action? _watchInteract;
    private string _watchPromptKey = "INTERACT_STOP";

    /// <inheritdoc/>
    public bool Watching => _watching;

    /// <inheritdoc/>
    public string WatchPromptKey => _watchPromptKey;

    public override void _Ready()
    {
        _head = GetNode<Node3D>("Head");
        _body = GetNode<PlayerBody>("Body");
        _interactor = GetNode<Interactor>("Head/Camera3D/Interactor");
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

        // The right hand holds whatever the interactor carries, and only what the body can reach.
        _body.SetHandTarget(_interactor.Grip);
        _body.SetEyeHeight(_eyeHeight);
        _interactor.Body = _body;
        _interactor.Viewer = this;

        Input.MouseMode = Input.MouseModeEnum.Captured;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventMouseMotion motion && Input.MouseMode == Input.MouseModeEnum.Captured)
        {
            float sensitivity = MouseSensitivity * Settings.Instance.MouseSensitivity;
            if (_watching)
            {
                // Watching, the mouse only looks a little around what is watched.
                _watchLook = (_watchLook - motion.ScreenRelative * sensitivity).Clamp(-WatchLookRange, WatchLookRange);
                return;
            }
            _yaw -= motion.ScreenRelative.X * sensitivity;
            _pitch = Mathf.Clamp(_pitch - motion.ScreenRelative.Y * sensitivity, -MaxPitch, MaxPitch);
        }
    }

    public override void _Process(double delta)
    {
        Vector3 feet = GetGlobalTransformInterpolated().Origin;
        _watchBlend = Mathf.MoveToward(_watchBlend, _watching ? 1 : 0, (float)delta / WatchTime);
        float watch = Mathf.SmoothStep(0, 1, _watchBlend);
        if (_watching)
        {
            float turn = 1 - Mathf.Exp(-8 * (float)delta);
            _yaw = Mathf.LerpAngle(_yaw, _watchYaw + _watchLook.X, turn);
            _pitch = Mathf.Lerp(_pitch, _watchPitch + _watchLook.Y, turn);
        }

        float bend = Mathf.Max(_interactor.Bend, watch);
        float stoop = Mathf.Max(_interactor.Depth * _interactor.Bend, _watchDepth * watch);
        Vector3 eyes = feet + Vector3.Up * _eyeHeight;

        // Bending to pick something up, the camera goes where the stooping body's eyes are (the body
        // stands under the eyes then), as much as the pose is the stoop: never behind the eyes, where it
        // would see the neck. Where the head has no room, the body is pulled back instead.
        Vector3 stooped = feet + new Basis(Vector3.Up, _yaw) * _body.EyesAt(stoop);
        Vector3 shift = _body.HeadRoom(eyes, (stooped - eyes) * PlayerBody.StoopShare(bend), out Vector3 pulledBack);

        _head.GlobalPosition = eyes + shift;
        _head.Rotation = new Vector3(_pitch, _yaw, 0);
        _body.Place(feet, _yaw, _pitch, bend, pulledBack, delta);
        _body.Stoop(stoop, bend);
        _body.PoseRightHand(_interactor.ArmReach, _interactor.HandClosed);
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

        // Standing still while picking something up. Watching, the feet go to their spot, and moving
        // or the "interact" action stops watching.
        Vector2 input = _interactor.Busy || _watching
            ? Vector2.Zero
            : Input.GetVector("move_left", "move_right", "move_forward", "move_back");
        Vector3 direction = Transform.Basis * new Vector3(input.X, 0, input.Y);
        velocity.X = direction.X * WalkSpeed;
        velocity.Z = direction.Z * WalkSpeed;
        if (_watching)
        {
            Vector3 toSpot = (_watchFeet - GlobalPosition) with { Y = 0 };
            Vector3 glide = (toSpot / (float)delta).LimitLength(WalkSpeed);
            velocity.X = glide.X;
            velocity.Z = glide.Z;
            if (MovePressed())
            {
                StopWatching();
            }
            else if (Input.IsActionJustPressed("interact"))
            {
                if (_watchInteract != null)
                {
                    _watchInteract();
                }
                else
                {
                    StopWatching();
                }
            }
        }

        Velocity = velocity;
        MoveAndSlide();

        // The feet kick from where the body was last drawn (placed in _Process). The body is not placed
        // here too: left at the physics position until the frame, the reaching hand lands a tick behind
        // and shakes while walking. Below 60 frames per second a kick can come one frame late.
        KickLooseObjects();

        // What the player really moved: walking into a wall, the legs stand still.
        _body.Walk(GlobalBasis.Inverse() * GetRealVelocity(), delta);
    }

    /// <inheritdoc/>
    public void Watch(Vector3 eyes, Vector3 target, Action stopped, Action? onInteract = null, string promptKey = "INTERACT_STOP")
    {
        // The bend that puts the eyes at the height asked for (the stoop lowers them all the way down).
        float height = eyes.Y - GlobalPosition.Y;
        _watchDepth = 0;
        for (int i = 0; i <= 20; i++)
        {
            float depth = i / 20f;
            if (Mathf.Abs(_body.EyesAt(depth).Y - height) < Mathf.Abs(_body.EyesAt(_watchDepth).Y - height))
            {
                _watchDepth = depth;
            }
        }

        Vector3 look = target - eyes;
        _watchYaw = Mathf.Atan2(-look.X, -look.Z);
        _watchPitch = Mathf.Atan2(look.Y, new Vector2(look.X, look.Z).Length());
        _watchFeet = eyes - new Basis(Vector3.Up, _watchYaw) * _body.EyesAt(_watchDepth);
        _watchLook = Vector2.Zero;
        _stopped = stopped;
        _watchInteract = onInteract;
        _watchPromptKey = promptKey;
        _watching = true;
    }

    /// <inheritdoc/>
    public void StopWatching()
    {
        if (!_watching)
        {
            return;
        }

        _watching = false;
        Action? stopped = _stopped;
        _stopped = null;
        _watchInteract = null;
        _watchPromptKey = "INTERACT_STOP";
        stopped?.Invoke();
    }

    private static bool MovePressed() =>
        Input.IsActionJustPressed("move_left") || Input.IsActionJustPressed("move_right") ||
        Input.IsActionJustPressed("move_forward") || Input.IsActionJustPressed("move_back");

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

        // Tomba para frente e acrescenta giro lateral determinístico proporcional ao desvio do chute.
        Vector3 spin = Vector3.Up.Cross(away) * kickSpeed * KickSpin;
        Vector3 flatVel = new Vector3(Velocity.X, 0, Velocity.Z);
        if (!flatVel.IsZeroApprox())
        {
            Vector3 lateral = Vector3.Up.Cross(flatVel.Normalized());
            float offset = Mathf.Clamp((body.GlobalPosition - ankle).Dot(lateral), -0.08f, 0.08f);
            spin += Vector3.Up * (offset * kickSpeed * KickSpin * 6.0f);
        }
        body.AngularVelocity = spin;
    }
}
