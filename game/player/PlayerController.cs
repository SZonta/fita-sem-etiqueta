using FitaSemEtiqueta.Core;
using Godot;

namespace FitaSemEtiqueta.Player;

/// <summary>
/// First-person movement: WASD to walk, mouse to look.
/// </summary>
public partial class PlayerController : CharacterBody3D
{
    [Export] public float WalkSpeed { get; set; } = 3.0f;

    /// <summary>
    /// Radians of rotation per pixel of mouse movement, before the player's
    /// own sensitivity setting is applied.
    /// </summary>
    [Export] public float MouseSensitivity { get; set; } = 0.002f;

    // Just short of straight up/down, so the view never flips over.
    private static readonly float MaxPitch = Mathf.DegToRad(89);

    private Node3D _head = null!;
    private float _eyeHeight;
    private float _yaw;
    private float _pitch;

    public override void _Ready()
    {
        _head = GetNode<Node3D>("Head");
        _eyeHeight = _head.Position.Y;
        _yaw = GlobalRotation.Y;

        // The body moves on physics ticks and the engine smooths it between them.
        // The view has to follow the mouse on every frame instead, so the head is
        // detached from the body and placed by hand in _Process
        // (first-person pattern from the Godot physics interpolation docs).
        _head.TopLevel = true;
        _head.PhysicsInterpolationMode = PhysicsInterpolationModeEnum.Off;

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
        _head.GlobalPosition = GetGlobalTransformInterpolated().Origin + Vector3.Up * _eyeHeight;
        _head.Rotation = new Vector3(_pitch, _yaw, 0);
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
    }
}
