using FitaSemEtiqueta.Interaction;
using FitaSemEtiqueta.Tapes;
using Godot;

namespace FitaSemEtiqueta.Dialogue;

/// <summary>
/// Área sobre o balcão onde o jogador pousa a fita pedida pela cliente.
/// Só aceita a fita correta; se o jogador apontar com outra fita, exibe o aviso sem soltá-la.
/// </summary>
public partial class CounterSpot : Area3D, IReceiver
{
    [Signal] public delegate void TapeHandedOverEventHandler(Pickable tape);

    /// <summary>Chave de tradução do título esperado pela cliente.</summary>
    [Export] public string ExpectedTitleKey { get; set; } = "";

    // Fita deitada no tampo do balcão, lombada voltada para o lado do salão (-X).
    private static readonly Basis Lying = new(new Vector3(0, 0, -1), new Vector3(-1, 0, 0), new Vector3(0, 1, 0));

    private Pickable? _placed;

    /// <summary>Verdadeiro enquanto aguarda o jogador entregar a fita pedida.</summary>
    public bool Active { get; set; }

    public bool Accepts(Pickable item, out string hint)
    {
        if (!Active || _placed != null)
        {
            hint = "";
            return false;
        }

        TapeInfo? tape = TapeInfo.Of(item);
        if (tape == null)
        {
            hint = "";
            return false;
        }

        if (tape.TitleKey != ExpectedTitleKey)
        {
            hint = Tr("COUNTER_WRONG_TAPE");
            return false;
        }

        hint = Tr("INTERACT_HAND_OVER");
        return true;
    }

    public Transform3D? SpotFor(Pickable item, Vector3 aimedAt)
    {
        if (!Active || _placed != null)
        {
            return null;
        }
        Vector3 resting = GlobalPosition + Vector3.Up * (item.HalfSize.Z + 0.001f);
        return new Transform3D(GlobalBasis * Lying, resting);
    }

    public void Receive(Pickable item, Interactor interactor)
    {
        _placed = item;
        item.SetReachable(false);
        EmitSignal(SignalName.TapeHandedOver, item);
    }

    /// <summary>Remove a fita entregue quando a cliente termina de agradecer e a leva embora.</summary>
    public void TakePlaced()
    {
        if (_placed != null && IsInstanceValid(_placed))
        {
            _placed.QueueFree();
        }
        _placed = null;
    }
}
