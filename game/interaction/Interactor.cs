using Godot;

namespace FitaSemEtiqueta.Interaction;

/// <summary>
/// The player's reach: a ray out of the camera. Whatever it hits that implements
/// <see cref="IInteractable"/> can be used with the "interact" action.
/// Also carries one <see cref="Pickable"/> at a time.
/// </summary>
public partial class Interactor : RayCast3D
{
    /// <summary>Emitted with the translation key of the hint to show, or "" for none.</summary>
    [Signal] public delegate void PromptChangedEventHandler(string key);

    // Physics layer 1: walls, floor and furniture.
    private const uint WorldMask = 1;

    // Gap kept between a carried item and the wall it is pulled back from, in metres.
    private const float WallGap = 0.02f;

    private Node3D _holdPoint = null!;
    private Pickable? _held;
    private PhysicsShapeQueryParameters3D _sweep = null!;
    private string _prompt = "";

    public override void _Ready()
    {
        _holdPoint = GetNode<Node3D>("../HoldPoint");
    }

    public override void _PhysicsProcess(double delta)
    {
        IInteractable? target = _held == null ? GetCollider() as IInteractable : null;

        SetPrompt(_held != null ? "INTERACT_DROP" : target?.PromptKey ?? "");

        if (!Input.IsActionJustPressed("interact"))
        {
            return;
        }

        if (_held != null)
        {
            _held.Release();
            _held = null;
        }
        else
        {
            target?.Interact(this);
        }
    }

    /// <summary>
    /// Keeps the carried item out of the walls: it is swept from the eyes towards
    /// the hand and stops where it would touch something, like an arm folding back.
    /// Runs every frame because the camera moves every frame.
    /// </summary>
    public override void _Process(double delta)
    {
        if (_held == null)
        {
            return;
        }

        Vector3 eyes = GlobalPosition;
        _sweep.Transform = new Transform3D(_holdPoint.GlobalBasis, eyes);
        _sweep.Motion = _holdPoint.GlobalPosition - eyes;

        // First number: how far along the sweep the item can go without touching, from 0 to 1.
        float clearFraction = GetWorld3D().DirectSpaceState.CastMotion(_sweep)[0];
        _held.GlobalPosition = eyes + _sweep.Motion * clearFraction;
    }

    /// <summary>Takes the item into the player's hand.</summary>
    public void Hold(Pickable item)
    {
        _sweep = new PhysicsShapeQueryParameters3D
        {
            Shape = item.Shape,
            CollisionMask = WorldMask,
            Margin = WallGap,
        };

        item.AttachTo(_holdPoint);
        _held = item;
    }

    private void SetPrompt(string key)
    {
        if (key == _prompt)
        {
            return;
        }

        _prompt = key;
        EmitSignal(SignalName.PromptChanged, key);
    }
}
