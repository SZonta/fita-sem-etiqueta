using FitaSemEtiqueta.Dialogue;
using FitaSemEtiqueta.Interaction;
using FitaSemEtiqueta.Player;
using Godot;

namespace FitaSemEtiqueta.Npc;

/// <summary>
/// Tião, dono do bar: conversa com Nondas atrás do balcão, comenta sobre a quermesse
/// e o cliente desconhecido, e convida para sentar na calçada com uma cerveja.
/// </summary>
public partial class BarOwnerTiao : AnimatableBody3D, IInteractable
{
    private const float TurnSpeed = 6.0f;
    private const float BlendTime = 0.2f;

    [Export] public DialoguePlayer Dialogue { get; set; } = null!;
    [Export] public PlayerController Player { get; set; } = null!;
    [Export] public AudioStream[] TiaoVoice { get; set; } = [];
    [Export] public AudioStream[] NondasVoice { get; set; } = [];

    private AnimationPlayer _anim = null!;
    private Node3D _mouth = null!;
    private float _restYaw;
    private string _currentAnim = "";
    private bool _gesturing;
    private bool _greeted;

    public bool WorksWhileHolding => true;

    public string PromptKey => !Dialogue.IsPlaying ? "INTERACT_TALK_TIAO" : "";

    public override void _Ready()
    {
        SyncToPhysics = false;
        _restYaw = Rotation.Y;
        _anim = GetNode<AnimationPlayer>("Model/AnimationPlayer");
        _mouth = GetNode<Node3D>("Mouth");

        foreach (string name in new[] { "idle", "talk" })
        {
            if (_anim.HasAnimation(name))
            {
                _anim.GetAnimation(name).LoopMode = Animation.LoopModeEnum.Linear;
            }
        }

        Dialogue.GestureChanged += OnGesture;
        PlayAnim("idle");
    }

    public override void _ExitTree()
    {
        if (Dialogue != null)
        {
            Dialogue.GestureChanged -= OnGesture;
        }
    }

    public void Interact(Interactor interactor)
    {
        if (Dialogue.IsPlaying)
        {
            return;
        }

        if (!_greeted)
        {
            _greeted = true;
            Dialogue.Play(
                TiaoLine("N1_TIAO_HELLO"),
                NondasLine("N1_NONDAS_BAR_1"),
                TiaoLine("N1_TIAO_STRANGER"),
                TiaoLine("N1_TIAO_INVITE"),
                NondasLine("N1_NONDAS_BAR_2"));
            return;
        }

        Dialogue.Play(TiaoLine("N1_TIAO_AGAIN"));
    }

    public override void _PhysicsProcess(double delta)
    {
        float dt = (float)delta;
        float desiredYaw = _restYaw;

        // Vira suavemente para o jogador quando estiver perto do balcão ou conversando.
        if (Player != null && GlobalPosition.DistanceTo(Player.GlobalPosition) < 4.0f)
        {
            Vector3 look = (Player.GlobalPosition - GlobalPosition) with { Y = 0.0f };
            if (look.LengthSquared() > 0.001f)
            {
                desiredYaw = Mathf.Atan2(-look.X, -look.Z);
            }
        }

        Rotation = new Vector3(0.0f, Mathf.LerpAngle(Rotation.Y, desiredYaw, 1.0f - Mathf.Exp(-TurnSpeed * dt)), 0.0f);
        PlayAnim(_gesturing ? "talk" : "idle");
    }

    private void OnGesture(string speakerName, bool active)
    {
        if (speakerName == Name.ToString())
        {
            _gesturing = active;
        }
    }

    private void PlayAnim(string name)
    {
        if (!_anim.HasAnimation(name) || _currentAnim == name)
        {
            return;
        }
        _currentAnim = name;
        _anim.Play(name, BlendTime);
    }

    private DialogueLine TiaoLine(string textKey) => new("SPEAKER_TIAO", textKey, TiaoVoice, _mouth, Name);

    private DialogueLine NondasLine(string textKey) => new("SPEAKER_NONDAS", textKey, NondasVoice);
}
