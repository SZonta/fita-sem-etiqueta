using System.Collections.Generic;
using Godot;

namespace FitaSemEtiqueta.Dialogue;

/// <summary>
/// Reproduz sequências de diálogo sem voz: revela a legenda aos poucos, dispara sílabas de ruído
/// de fala (3D no NPC ou 2D para o Nondas) e controla callbacks de gesto enquanto a fala dura.
/// </summary>
public partial class DialoguePlayer : Node
{
    [Signal] public delegate void FinishedEventHandler();

    /// <summary>Emitido quando um falante inicia ou encerra a fala (para acionar gestos).</summary>
    [Signal] public delegate void GestureChangedEventHandler(string speakerName, bool active);

    /// <summary>Caixa de legenda controlada por este reprodutor.</summary>
    [Export] public SubtitleBox Subtitles { get; set; } = null!;

    private const float CharsPerSecond = 34.0f;
    private const float HoldAfterReveal = 2.0f;
    private const int CharsPerBlip = 3;

    private readonly Queue<DialogueLine> _queue = new();
    private readonly RandomNumberGenerator _rng = new();

    private AudioStreamPlayer _playerVoice = null!;
    private AudioStreamPlayer3D _npcVoice = null!;

    private DialogueLine? _current;
    private int _totalChars;
    private int _lastBlipStep;
    private float _clock;
    private float _revealDuration;
    private bool _gesturing;

    public bool IsPlaying => _current != null || _queue.Count > 0;

    public override void _Ready()
    {
        _rng.Seed = 1996;
        _playerVoice = new AudioStreamPlayer { Name = "PlayerVoice", VolumeDb = -4.0f };
        _npcVoice = new AudioStreamPlayer3D
        {
            Name = "NpcVoice",
            VolumeDb = -2.0f,
            MaxDistance = 14.0f,
            UnitSize = 2.5f,
        };
        AddChild(_playerVoice);
        AddChild(_npcVoice);
    }

    /// <summary>Inicia imediatamente uma sequência de falas, substituindo qualquer anterior.</summary>
    public void Play(params DialogueLine[] lines)
    {
        Stop();
        foreach (DialogueLine line in lines)
        {
            _queue.Enqueue(line);
        }
        Advance();
    }

    /// <summary>Interrompe a fala atual e limpa a legenda.</summary>
    public void Stop()
    {
        SetGesturing(false);
        _queue.Clear();
        _current = null;
        Subtitles?.Clear();
    }

    public override void _Process(double delta)
    {
        if (_current == null)
        {
            return;
        }

        _clock += (float)delta;
        float progress = _revealDuration > 0.0f ? Mathf.Clamp(_clock / _revealDuration, 0.0f, 1.0f) : 1.0f;
        Subtitles?.SetProgress(progress);

        int revealed = Mathf.FloorToInt(progress * _totalChars);
        int blipStep = revealed / CharsPerBlip;
        if (progress < 1.0f && blipStep > _lastBlipStep)
        {
            _lastBlipStep = blipStep;
            PlayBlip(_current);
        }

        if (progress >= 1.0f && _gesturing)
        {
            SetGesturing(false);
        }

        if (_clock >= _revealDuration + HoldAfterReveal)
        {
            Advance();
        }
    }

    private void Advance()
    {
        SetGesturing(false);
        if (_queue.Count == 0)
        {
            _current = null;
            Subtitles?.Clear();
            EmitSignal(SignalName.Finished);
            return;
        }

        _current = _queue.Dequeue();
        _totalChars = Mathf.Max(1, Tr(_current.TextKey).Length);
        _revealDuration = _totalChars / CharsPerSecond;
        _clock = 0.0f;
        _lastBlipStep = 0;

        Subtitles?.ShowLine(_current.SpeakerKey, _current.TextKey, 0.0f);
        SetGesturing(true);
        PlayBlip(_current);
    }

    private void SetGesturing(bool active)
    {
        if (_gesturing == active)
        {
            return;
        }
        _gesturing = active;
        if (_current?.SpeakerName is { Length: > 0 } speakerName)
        {
            EmitSignal(SignalName.GestureChanged, speakerName, active);
        }
    }

    private void PlayBlip(DialogueLine line)
    {
        if (line.VoiceClips.Length == 0)
        {
            return;
        }
        AudioStream clip = line.VoiceClips[_rng.RandiRange(0, line.VoiceClips.Length - 1)];
        float pitch = _rng.RandfRange(0.95f, 1.06f);
        if (line.SpeakerNode != null)
        {
            _npcVoice.GlobalPosition = line.SpeakerNode.GlobalPosition;
            _npcVoice.Stream = clip;
            _npcVoice.PitchScale = pitch;
            _npcVoice.Play();
        }
        else
        {
            _playerVoice.Stream = clip;
            _playerVoice.PitchScale = pitch;
            _playerVoice.Play();
        }
    }
}
