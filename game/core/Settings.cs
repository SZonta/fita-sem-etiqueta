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

    /// <summary>Emitted after any setting changes.</summary>
    [Signal] public delegate void ChangedEventHandler();

    public static Settings Instance { get; private set; } = null!;

    private float _vhsIntensity = 0.7f;
    private float _mouseSensitivity = 1.0f;
    private float _volume = 1.0f;

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

    public override void _EnterTree()
    {
        Instance = this;
        Load();
        ApplyVolume();
    }

    private void OnChanged()
    {
        ApplyVolume();
        Save();
        EmitSignal(SignalName.Changed);
    }

    private void ApplyVolume()
    {
        // Bus 0 is the master bus: every sound in the game goes through it.
        AudioServer.SetBusVolumeLinear(0, _volume);
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
    }

    private void Save()
    {
        var file = new ConfigFile();
        file.SetValue(Section, "vhs_intensity", _vhsIntensity);
        file.SetValue(Section, "mouse_sensitivity", _mouseSensitivity);
        file.SetValue(Section, "volume", _volume);
        file.Save(FilePath);
    }
}
