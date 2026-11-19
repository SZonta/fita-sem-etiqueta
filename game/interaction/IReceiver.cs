using Godot;

namespace FitaSemEtiqueta.Interaction;

/// <summary>
/// A place the item in hand can be put: a gap on a shelf, a machine's slot.
/// Aimed at through physics layer 5, "slots", by the <see cref="Interactor"/>.
/// </summary>
public interface IReceiver
{
    /// <summary>
    /// True if the item goes here; <paramref name="hint"/> is then the text to show for putting it.
    /// False if it does not; <paramref name="hint"/> then says why, or is "" to let the item simply be dropped.
    /// </summary>
    bool Accepts(Pickable item, out string hint);

    /// <summary>Where the item would be put, near the point aimed at, or null if there is no room for it.</summary>
    Transform3D? SpotFor(Pickable item, Vector3 aimedAt);

    /// <summary>The hand has let go of the item, standing still at its spot.</summary>
    void Receive(Pickable item, Interactor interactor);
}
