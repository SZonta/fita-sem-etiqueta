using FitaSemEtiqueta.Dialogue;
using FitaSemEtiqueta.Interaction;
using Godot;

namespace FitaSemEtiqueta.Npc;

/// <summary>
/// Paçoca, o vira-lata caramelo da calçada do bar: reage a carinho com animação feliz e som curto.
/// Nunca sofre dano.
/// </summary>
public partial class DogPacoca : StaticBody3D, IInteractable
{
    private const float HappyDuration = 2.8f;
    private const float BlendTime = 0.2f;

    [Export] public DialoguePlayer Dialogue { get; set; } = null!;
    [Export] public AudioStream[] NondasVoice { get; set; } = [];
    [Export] public AudioStream? HappySound { get; set; }

    private AnimationPlayer _anim = null!;
    private AudioStreamPlayer3D _voice = null!;
    private float _happyTimer;
    private string _currentAnim = "";

    public string PromptKey => _happyTimer <= 0.0f && !Dialogue.IsPlaying ? "INTERACT_PET_PACOCA" : "";

    public override void _Ready()
    {
        _anim = GetNode<AnimationPlayer>("Model/AnimationPlayer");
        _voice = GetNode<AudioStreamPlayer3D>("Voice");
        foreach (string name in new[] { "lying_idle", "pet_happy" })
        {
            if (_anim.HasAnimation(name))
            {
                _anim.GetAnimation(name).LoopMode = Animation.LoopModeEnum.Linear;
            }
        }
        PlayAnim("lying_idle");
    }

    public void Interact(Interactor interactor)
    {
        if (_happyTimer > 0.0f || Dialogue.IsPlaying)
        {
            return;
        }

        _happyTimer = HappyDuration;
        PlayAnim("pet_happy");
        if (HappySound != null)
        {
            _voice.Stream = HappySound;
            _voice.Play();
        }
        Dialogue.Play(new DialogueLine("SPEAKER_NONDAS", "N1_NONDAS_PET_PACOCA", NondasVoice));
    }

    public override void _Process(double delta)
    {
        if (_happyTimer <= 0.0f)
        {
            return;
        }

        _happyTimer -= (float)delta;
        if (_happyTimer <= 0.0f)
        {
            _happyTimer = 0.0f;
            PlayAnim("lying_idle");
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
}
