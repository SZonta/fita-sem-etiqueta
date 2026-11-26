using FitaSemEtiqueta.Core;
using Godot;

namespace FitaSemEtiqueta.UI;

/// <summary>
/// Pause menu: Esc opens and closes it.
/// While it is open the game is paused and the mouse is free.
/// </summary>
public partial class PauseMenu : CanvasLayer
{
    private static readonly (string Code, string Key)[] Languages =
    [
        ("pt", "LANGUAGE_PT"),
        ("en", "LANGUAGE_EN"),
        ("es", "LANGUAGE_ES"),
    ];

    private static readonly string[] SubtitleSizeKeys =
    [
        "SUBTITLE_SIZE_SMALL",
        "SUBTITLE_SIZE_MEDIUM",
        "SUBTITLE_SIZE_LARGE",
    ];

    private OptionButton _languageButton = null!;
    private OptionButton _subtitleSizeButton = null!;

    public override void _Ready()
    {
        Settings settings = Settings.Instance;

        HSlider vhsIntensity = GetNode<HSlider>("%VhsIntensitySlider");
        vhsIntensity.Value = settings.VhsIntensity;
        vhsIntensity.ValueChanged += value => settings.VhsIntensity = (float)value;

        HSlider mouseSensitivity = GetNode<HSlider>("%MouseSensitivitySlider");
        mouseSensitivity.Value = settings.MouseSensitivity;
        mouseSensitivity.ValueChanged += value => settings.MouseSensitivity = (float)value;

        HSlider volume = GetNode<HSlider>("%VolumeSlider");
        volume.Value = settings.Volume;
        volume.ValueChanged += value => settings.Volume = (float)value;

        _languageButton = GetNode<OptionButton>("%LanguageButton");
        for (int i = 0; i < Languages.Length; i++)
        {
            _languageButton.AddItem(Languages[i].Key, i);
            if (Languages[i].Code == settings.Language)
            {
                _languageButton.Selected = i;
            }
        }
        _languageButton.ItemSelected += index => settings.Language = Languages[(int)index].Code;

        _subtitleSizeButton = GetNode<OptionButton>("%SubtitleSizeButton");
        for (int i = 0; i < SubtitleSizeKeys.Length; i++)
        {
            _subtitleSizeButton.AddItem(Tr(SubtitleSizeKeys[i]), i);
        }
        _subtitleSizeButton.Selected = settings.SubtitleSize;
        _subtitleSizeButton.ItemSelected += index => settings.SubtitleSize = (int)index;

        GetNode<Button>("%ResumeButton").Pressed += () => SetOpen(false);
    }

    public override void _EnterTree()
    {
        if (Settings.Instance != null)
        {
            Settings.Instance.Changed += RefreshTranslatedOptions;
        }
    }

    public override void _ExitTree()
    {
        if (Settings.Instance != null)
        {
            Settings.Instance.Changed -= RefreshTranslatedOptions;
        }
    }

    private void RefreshTranslatedOptions()
    {
        if (_subtitleSizeButton == null)
        {
            return;
        }
        // Atualiza o texto dos itens já traduzidos manualmente no OptionButton sem disparar ItemSelected.
        for (int i = 0; i < SubtitleSizeKeys.Length; i++)
        {
            _subtitleSizeButton.SetItemText(i, Tr(SubtitleSizeKeys[i]));
        }
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsActionPressed("ui_cancel"))
        {
            SetOpen(!Visible);
            GetViewport().SetInputAsHandled();
        }
    }

    private void SetOpen(bool open)
    {
        Visible = open;
        GetTree().Paused = open;
        Input.MouseMode = open ? Input.MouseModeEnum.Visible : Input.MouseModeEnum.Captured;
    }
}
