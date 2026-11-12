namespace FitaSemEtiqueta.Interaction;

/// <summary>
/// Something the player can use by looking at it and pressing the "interact" action.
/// </summary>
public interface IInteractable
{
    /// <summary>Translation key of the hint shown while the player looks at this.</summary>
    string PromptKey { get; }

    void Interact(Interactor interactor);
}
