using Godot;

namespace FitaSemEtiqueta.Dialogue;

/// <summary>
/// Uma fala curta exibida na legenda inferior com ruído de fala e gesto opcional do falante.
/// </summary>
public sealed record DialogueLine(
    string SpeakerKey,
    string TextKey,
    AudioStream[] VoiceClips,
    Node3D? SpeakerNode = null,
    string? SpeakerName = null);
