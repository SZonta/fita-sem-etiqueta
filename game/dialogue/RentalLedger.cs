using FitaSemEtiqueta.Interaction;
using Godot;

namespace FitaSemEtiqueta.Dialogue;

/// <summary>
/// Caderno aberto sobre o balcão onde o atendente registra a locação da fita.
/// Pode ser acionado mesmo enquanto o jogador segura uma fita na mão.
/// </summary>
public partial class RentalLedger : StaticBody3D, IInteractable
{
    [Signal] public delegate void LoggedEventHandler();

    /// <summary>Som de caneta escrevendo ao anotar a locação.</summary>
    [Export] public AudioStream? PenSound { get; set; }

    private static readonly FontVariation BoldFont = new() { VariationEmbolden = 0.6f };

    private AudioStreamPlayer3D _audio = null!;
    private Label3D _header = null!;
    private Label3D _entry = null!;
    private bool _active;
    private bool _logged;

    public bool WorksWhileHolding => true;

    public bool IsLogged => _logged;

    public string PromptKey => _active && !_logged ? "INTERACT_LOG_RENTAL" : "";

    public override void _Ready()
    {
        _audio = new AudioStreamPlayer3D
        {
            Name = "PenAudio",
            Stream = PenSound,
            VolumeDb = -2.0f,
            MaxDistance = 8.0f,
        };
        AddChild(_audio);

        // Texto deitado na metade inferior em branco da página direita do caderno, voltado para o atendente (+X).
        var flatForAttendant = new Basis(new Vector3(0, 0, -1), new Vector3(-1, 0, 0), new Vector3(0, 1, 0));

        _header = BuildPageLabel("LEDGER_PAGE_HEADER", new Color(0.18f, 0.18f, 0.22f));
        _header.Transform = new Transform3D(flatForAttendant, new Vector3(0.005f, 0.011f, -0.025f));
        AddChild(_header);

        _entry = BuildPageLabel("LEDGER_ENTRY_NEIDE", new Color(0.08f, 0.14f, 0.45f));
        _entry.Transform = new Transform3D(flatForAttendant, new Vector3(0.038f, 0.011f, -0.025f));
        _entry.Visible = false;
        AddChild(_entry);
    }

    /// <summary>Habilita o registro após a cliente pedir a fita.</summary>
    public void Activate() => _active = true;

    public void Interact(Interactor interactor)
    {
        if (!_active || _logged)
        {
            return;
        }
        _logged = true;
        _entry.Visible = true;
        if (PenSound != null)
        {
            _audio.Play();
        }
        EmitSignal(SignalName.Logged);
    }

    private static Label3D BuildPageLabel(string key, Color color) => new()
    {
        Text = key,
        Font = BoldFont,
        PixelSize = 0.00055f,
        FontSize = 24,
        OutlineSize = 0,
        AlphaCut = Label3D.AlphaCutMode.Discard,
        AutowrapMode = TextServer.AutowrapMode.WordSmart,
        Width = 350.0f,
        Shaded = true,
        Modulate = color,
        HorizontalAlignment = HorizontalAlignment.Left,
        VerticalAlignment = VerticalAlignment.Top,
    };
}
