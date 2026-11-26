using Godot;

namespace FitaSemEtiqueta.Core;

/// <summary>
/// Player preferences, kept between runs in user://settings.cfg.
/// Autoload: reach it from anywhere through <see cref="Instance"/>.
/// </summary>
public partial class Settings : Node
{
    private const string FilePath = "user://settings.cfg";
    private const string Section = "settings";

    private static readonly string[] ValidLanguages = ["pt", "en", "es"];

    /// <summary>Emitted after any setting changes.</summary>
    [Signal] public delegate void ChangedEventHandler();

    public static Settings Instance { get; private set; } = null!;

    private float _vhsIntensity = 0.7f;
    private float _mouseSensitivity = 1.0f;
    private float _volume = 1.0f;
    private string _language = "pt";
    private int _subtitleSize = 1;

    /// <summary>Strength of the VHS filter: 0 (off) to 1 (heavy).</summary>
    public float VhsIntensity
    {
        get => _vhsIntensity;
        set { _vhsIntensity = value; OnChanged(); }
    }

    /// <summary>Multiplier over the player's base look speed. 1 is the default feel.</summary>
    public float MouseSensitivity
    {
        get => _mouseSensitivity;
        set { _mouseSensitivity = value; OnChanged(); }
    }

    /// <summary>Master volume: 0 (silent) to 1 (full).</summary>
    public float Volume
    {
        get => _volume;
        set { _volume = value; OnChanged(); }
    }

    /// <summary>Idioma ativo da interface e legendas ("pt", "en" ou "es").</summary>
    public string Language
    {
        get => _language;
        set
        {
            string normalized = NormalizeLanguage(value);
            if (_language == normalized)
            {
                return;
            }
            _language = normalized;
            OnChanged();
        }
    }

    /// <summary>Tamanho das legendas: 0 (pequena), 1 (média) ou 2 (grande).</summary>
    public int SubtitleSize
    {
        get => _subtitleSize;
        set
        {
            int clamped = Mathf.Clamp(value, 0, 2);
            if (_subtitleSize == clamped)
            {
                return;
            }
            _subtitleSize = clamped;
            OnChanged();
        }
    }

    /// <summary>Tamanho de fonte em pixels correspondente à opção de legenda escolhida.</summary>
    public int SubtitleFontSize => _subtitleSize switch
    {
        0 => 20,
        2 => 34,
        _ => 26,
    };

    public override void _EnterTree()
    {
        Instance = this;
        Load();
        ApplyVolume();
        ApplyLanguage();
    }

    private void OnChanged()
    {
        ApplyVolume();
        ApplyLanguage();
        Save();
        EmitSignal(SignalName.Changed);
    }

    private void ApplyVolume()
    {
        // Bus 0 is the master bus: every sound in the game goes through it.
        AudioServer.SetBusVolumeLinear(0, _volume);
    }

    private void ApplyLanguage()
    {
        TranslationServer.SetLocale(_language);
    }

    private static string NormalizeLanguage(string? value)
    {
        foreach (string lang in ValidLanguages)
        {
            if (lang == value)
            {
                return lang;
            }
        }
        return "pt";
    }

    private void Load()
    {
        var file = new ConfigFile();
        if (file.Load(FilePath) != Error.Ok)
        {
            return; // First run: no file yet, keep the defaults.
        }

        _vhsIntensity = (float)file.GetValue(Section, "vhs_intensity", _vhsIntensity);
        _mouseSensitivity = (float)file.GetValue(Section, "mouse_sensitivity", _mouseSensitivity);
        _volume = (float)file.GetValue(Section, "volume", _volume);
        _language = NormalizeLanguage((string)file.GetValue(Section, "language", _language));
        _subtitleSize = Mathf.Clamp((int)file.GetValue(Section, "subtitle_size", _subtitleSize), 0, 2);
    }

    private void Save()
    {
        var file = new ConfigFile();
        file.SetValue(Section, "vhs_intensity", _vhsIntensity);
        file.SetValue(Section, "mouse_sensitivity", _mouseSensitivity);
        file.SetValue(Section, "volume", _volume);
        file.SetValue(Section, "language", _language);
        file.SetValue(Section, "subtitle_size", _subtitleSize);
        file.Save(FilePath);
    }
}
