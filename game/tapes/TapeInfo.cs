using FitaSemEtiqueta.Interaction;
using Godot;

namespace FitaSemEtiqueta.Tapes;

/// <summary>
/// What a rental tape is: its film, its genre, its end credits, and where it is wound to.
/// A child of the tape's <see cref="Pickable"/>, named "TapeInfo".
/// </summary>
public partial class TapeInfo : Node, IDescribed
{
    /// <summary>Translation key of the film's title.</summary>
    [Export] public string TitleKey { get; set; } = "";

    /// <summary>Translation key of the genre, the same as on the rack's labels (SIGN_GENRE_*).</summary>
    [Export] public string GenreKey { get; set; } = "";

    /// <summary>End credits, as translation keys: a role, then the name, and so on.</summary>
    [Export] public string[] Credits { get; set; } = [];

    /// <summary>Chaves de tradução das linhas do carimbo gravado no vídeo caseiro (ex.: hora e data).</summary>
    [Export] public string[] Stamp { get; set; } = [];

    /// <summary>True for a tape someone recorded over after the credits.</summary>
    [Export] public bool Tampered { get; set; }

    /// <summary>True once rewound to the start. Returned tapes come back at the end.</summary>
    [Export] public bool Rewound { get; set; }

    /// <summary>
    /// True once played to the end in the store's VCR. Only a tested tape may be rewound: rewound first,
    /// what was recorded after the credits would never be seen.
    /// </summary>
    [Export] public bool Tested { get; set; }

    public string Caption => string.Format(Tr("TAPE_CAPTION"), Tr(TitleKey), Tr(GenreKey));

    /// <summary>The tape this describes.</summary>
    public static TapeInfo? Of(Pickable item) => item.GetNodeOrNull<TapeInfo>("TapeInfo");
}
