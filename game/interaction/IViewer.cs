using System;
using Godot;

namespace FitaSemEtiqueta.Interaction;

/// <summary>
/// Someone who can stop and look at something for a while (the test TV), given to the
/// <see cref="Interactor"/> by whoever owns it.
/// </summary>
public interface IViewer
{
    /// <summary>True while looking. The viewer then takes the "interact" action to stop.</summary>
    bool Watching { get; }

    /// <summary>Chave de tradução da dica exibida enquanto o jogador observa.</summary>
    string WatchPromptKey => "INTERACT_STOP";

    /// <summary>
    /// Goes and puts the eyes at <paramref name="eyes"/>, looking at <paramref name="target"/>, and stays there
    /// until the player moves or presses the "interact" action, or <see cref="StopWatching"/> is called.
    /// <paramref name="stopped"/> is called once, when that happens.
    /// </summary>
    void Watch(Vector3 eyes, Vector3 target, Action stopped, Action? onInteract = null, string promptKey = "INTERACT_STOP");

    void StopWatching();
}
