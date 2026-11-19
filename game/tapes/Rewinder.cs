using FitaSemEtiqueta.Interaction;
using Godot;

namespace FitaSemEtiqueta.Tapes;

/// <summary>
/// The counter's tape rewinder. A tape put in lies flat in it, spine towards the attendant; the lid closes,
/// the light comes on for a few seconds (the tape cannot be taken meanwhile), then the lid opens and the
/// rewound tape waits to be picked up. Idle, the lid stays open.
/// Needs a child "Model" (assets/models/rewinder.glb, with "Lid", "Led" and "TapeSpot") and a child
/// CollisionShape3D over the rewinder, on physics layer 5.
/// </summary>
public partial class Rewinder : Area3D, IReceiver
{
    /// <summary>Seconds a tape takes to rewind (a real one took about two minutes).</summary>
    [Export] public float RewindTime { get; set; } = 4f;

    /// <summary>How far the lid opens, degrees. More than the model's 75, so the tape comes out past the lid's edge.</summary>
    [Export] public float LidOpenDegrees { get; set; } = 100f;

    // Seconds for the lid to close or open.
    private const float LidTime = 0.35f;

    // Lying flat: the tape's length (X) across the rewinder, spine (+Y) towards the attendant (+X), thin side (Z) up.
    private static readonly Basis Lying = new(new Vector3(0, 0, 1), new Vector3(1, 0, 0), new Vector3(0, 1, 0));

    private Node3D _lid = null!;
    private Node3D _spot = null!;
    private StandardMaterial3D _led = null!;
    private Pickable? _tape;
    private float _clock;
    private readonly PhysicsShapeQueryParameters3D _room = new() { CollisionMask = 4 }; // Layer 3: a tape already there.

    public override void _Ready()
    {
        _lid = GetNode<Node3D>("Model/Lid");
        _spot = GetNode<Node3D>("Model/TapeSpot");

        // The light's own copy of its material, lit by code.
        var led = GetNode<MeshInstance3D>("Model/Led");
        _led = (StandardMaterial3D)led.Mesh.SurfaceGetMaterial(0).Duplicate();
        _led.EmissionEnabled = true;
        _led.Emission = new Color(1f, 0.11f, 0.07f);
        led.MaterialOverride = _led;
        Light(false);
        OpenLid(1);
    }

    public bool Accepts(Pickable item, out string hint)
    {
        TapeInfo? tape = TapeInfo.Of(item);
        if (tape == null)
        {
            hint = "";
            return false;
        }

        if (tape.Rewound || !tape.Tested)
        {
            hint = Tr(tape.Rewound ? "TAPE_ALREADY_REWOUND" : "TAPE_TEST_FIRST");
            return false;
        }

        hint = Tr("INTERACT_REWIND");
        return true;
    }

    public Transform3D? SpotFor(Pickable item, Vector3 aimedAt)
    {
        if (_tape != null)
        {
            return null; // Busy rewinding: no room, and nothing is dropped on it.
        }

        var spot = new Transform3D(GlobalBasis * Lying, _spot.GlobalPosition);
        _room.Shape = item.Shape;
        _room.Transform = spot;
        return GetWorld3D().DirectSpaceState.IntersectShape(_room, 1).Count == 0 ? spot : null;
    }

    public void Receive(Pickable item, Interactor interactor)
    {
        _tape = item;
        _clock = 0;
        item.SetReachable(false);
    }

    public override void _Process(double delta)
    {
        if (_tape == null)
        {
            return;
        }

        // Close, rewind with the light on, open.
        _clock += (float)delta;
        float rewound = LidTime + RewindTime;
        OpenLid(_clock < rewound ? 1 - _clock / LidTime : (_clock - rewound) / LidTime);
        Light(_clock >= LidTime && _clock < rewound);
        if (_clock < rewound + LidTime)
        {
            return;
        }

        if (IsInstanceValid(_tape))
        {
            TapeInfo.Of(_tape)!.Rewound = true;
            _tape.SetReachable(true);
        }
        _tape = null;
    }

    /// <summary>Opens the lid this much: 0 closed, 1 open.</summary>
    private void OpenLid(float open)
    {
        float at = Mathf.SmoothStep(0, 1, Mathf.Clamp(open, 0, 1));
        _lid.Rotation = new Vector3(-Mathf.DegToRad(LidOpenDegrees) * at, 0, 0);
    }

    private void Light(bool on) => _led.EmissionEnergyMultiplier = on ? 3f : 0f;
}
