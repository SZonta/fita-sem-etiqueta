using FitaSemEtiqueta.Interaction;
using Godot;

namespace FitaSemEtiqueta.Tapes;

/// <summary>
/// An empty run on a board of the tape rack, for tapes of one genre. Tapes are put away standing,
/// spine out, side by side, in the free place nearest to where the player aims.
/// The node sits on the board's front edge where the run starts; the run goes along local -Z,
/// into the rack along local +X. Needs a child CollisionShape3D over the run, on physics layer 5.
/// </summary>
public partial class ShelfGap : Area3D, IReceiver
{
    /// <summary>Translation key of the genre kept here (SIGN_GENRE_*).</summary>
    [Export] public string GenreKey { get; set; } = "";

    /// <summary>Length of the empty run, metres.</summary>
    [Export] public float Length { get; set; } = 0.2f;

    // Tapes stand this far apart, as the rack's own tapes do (tools/blender/store.py), metres.
    private const float Pitch = 0.0265f;

    // Gap left under a tape put away, so it is not inside the board, and how far back from
    // the board's front edge its spine stands, as on the rack's own tapes, metres.
    private const float Lift = 0.001f;
    private const float Inset = 0.005f;

    // How much smaller than the tape the room test is, so the neighbours' sides do not count, metres.
    private const float Shrink = 0.004f;

    // Physics layers 1 and 3: furniture, and loose things (another tape already there).
    private const uint SolidMask = 1 | 4;

    // Standing, spine out: the tape's length (X) up, its spine (+Y) towards the player (-X), its thin side along the board.
    private static readonly Basis Standing = new(new Vector3(0, 1, 0), new Vector3(-1, 0, 0), new Vector3(0, 0, 1));

    private readonly PhysicsShapeQueryParameters3D _room = new() { CollisionMask = SolidMask };
    private readonly BoxShape3D _box = new();

    public bool Accepts(Pickable item, out string hint)
    {
        TapeInfo? tape = TapeInfo.Of(item);
        if (tape == null)
        {
            hint = "";
            return false;
        }

        bool here = tape.GenreKey == GenreKey;
        hint = string.Format(Tr(here ? "INTERACT_SHELVE" : "SHELF_WRONG_GENRE"), Tr(GenreKey));
        return here;
    }

    public Transform3D? SpotFor(Pickable item, Vector3 aimedAt)
    {
        _box.Size = item.HalfSize * 2 - Vector3.One * Shrink;
        _room.Shape = _box;
        PhysicsDirectSpaceState3D space = GetWorld3D().DirectSpaceState;

        Transform3D? best = null;
        float nearest = float.MaxValue;
        int count = (int)(Length / Pitch);
        for (int i = 0; i < count; i++)
        {
            // Standing, the tape's X (length) is up and its Y (height) goes into the rack.
            var centre = new Vector3(item.HalfSize.Y + Inset, item.HalfSize.X + Lift, -(i + 0.5f) * Pitch);
            var spot = new Transform3D(GlobalBasis * Standing, GlobalTransform * centre);
            _room.Transform = spot;
            float distance = spot.Origin.DistanceTo(aimedAt);
            if (distance < nearest && space.IntersectShape(_room, 1).Count == 0)
            {
                best = spot;
                nearest = distance;
            }
        }
        return best;
    }

    public void Receive(Pickable item, Interactor interactor)
    {
        // It stays where it was put, still, until picked up again.
    }
}
