using FitaSemEtiqueta.Core;
using Godot;

namespace FitaSemEtiqueta.Vfx;

/// <summary>
/// The VHS look: the 3D world is rendered at about 480 lines and stretched to the
/// window, then the VHS shader is drawn over it. Intensity comes from Settings.
/// </summary>
public partial class VhsFilter : CanvasLayer
{
    private const float InternalLines = 480f;

    // Lowest 3D scale the engine accepts. Reached only on windows taller than 4800 pixels.
    private const float MinScale = 0.1f;

    private ShaderMaterial _material = null!;

    public override void _EnterTree()
    {
        _material = (ShaderMaterial)GetNode<ColorRect>("Screen").Material;

        GetWindow().SizeChanged += UpdateInternalResolution;
        Settings.Instance.Changed += UpdateIntensity;

        UpdateInternalResolution();
        UpdateIntensity();
    }

    public override void _ExitTree()
    {
        GetWindow().SizeChanged -= UpdateInternalResolution;
        Settings.Instance.Changed -= UpdateIntensity;
    }

    private void UpdateInternalResolution()
    {
        // Only the 3D world is scaled down; menus and text stay at window resolution.
        Window window = GetWindow();
        window.Scaling3DScale = Mathf.Clamp(InternalLines / window.Size.Y, MinScale, 1f);
    }

    private void UpdateIntensity()
    {
        _material.SetShaderParameter("intensity", Settings.Instance.VhsIntensity);
    }
}
