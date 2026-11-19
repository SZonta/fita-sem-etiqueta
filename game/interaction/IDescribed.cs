namespace FitaSemEtiqueta.Interaction;

/// <summary>A child node that tells what a <see cref="Pickable"/> is, shown while it is in hand.</summary>
public interface IDescribed
{
    /// <summary>Text ready to show (already translated).</summary>
    string Caption { get; }
}
