using Godot;

namespace FitaSemEtiqueta.Interaction;

/// <summary>
/// A body with a hand that the <see cref="Interactor"/> drives when picking things up.
/// </summary>
public interface IReacher
{
    /// <summary>
    /// How deep the body must stoop (0 to 1) for the wrist to reach a point, or at least to come within
    /// slack metres of it; -1 if it cannot.
    /// </summary>
    float StoopFor(Vector3 point, float slack);

    /// <summary>Where the wrist really ended up last frame, which is short of its target when out of reach.</summary>
    Vector3 Wrist { get; }
}
