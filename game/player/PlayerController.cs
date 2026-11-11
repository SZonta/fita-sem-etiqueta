using Godot;

namespace FitaSemEtiqueta.Player;

/// <summary>
/// First-person movement: WASD to walk, mouse to look.
/// Esc releases the mouse, a click captures it again.
/// </summary>
public partial class PlayerController : CharacterBody3D
{
    [Export] public float WalkSpeed { get; set; } = 3.0f;

    /// <summary>Radians of rotation per pixel of mouse movement.</summary>
    [Export] public float MouseSensitivity { get; set; } = 0.002f;

    // Just short of straight up/down, so the view never flips over.
    private static readonly float MaxPitch = Mathf.DegToRad(89);

    private Node3D _head = null!;

    public override void _Ready()
    {
        _head = GetNode<Node3D>("Head");
        Input.MouseMode = Input.MouseModeEnum.Captured;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        bool mouseCaptured = Input.MouseMode == Input.MouseModeEnum.Captured;

        if (@event is InputEventMouseMotion motion && mouseCaptured)
        {
            // The body turns left/right; only the head tilts up/down.
            RotateY(-motion.ScreenRelative.X * MouseSensitivity);
            float pitch = _head.Rotation.X - motion.ScreenRelative.Y * MouseSensitivity;
            _head.Rotation = new Vector3(Mathf.Clamp(pitch, -MaxPitch, MaxPitch), 0, 0);
        }
        else if (@event.IsActionPressed("ui_cancel"))
        {
            Input.MouseMode = Input.MouseModeEnum.Visible;
        }
        else if (@event is InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Left } && !mouseCaptured)
        {
            Input.MouseMode = Input.MouseModeEnum.Captured;
        }
    }

    public override void _PhysicsProcess(double delta)
    {
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
