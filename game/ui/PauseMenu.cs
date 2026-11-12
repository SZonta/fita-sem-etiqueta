using FitaSemEtiqueta.Core;
using Godot;

namespace FitaSemEtiqueta.UI;

/// <summary>
/// Pause menu: Esc opens and closes it.
/// While it is open the game is paused and the mouse is free.
/// </summary>
public partial class PauseMenu : CanvasLayer
{
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

        GetNode<Button>("%ResumeButton").Pressed += () => SetOpen(false);
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
