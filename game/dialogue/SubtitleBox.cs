using FitaSemEtiqueta.Core;
using Godot;

namespace FitaSemEtiqueta.Dialogue;

/// <summary>
/// Caixa de legenda na parte inferior da tela: respeita idioma e tamanho de fonte configurados
/// em Settings, atualizando imediatamente mesmo quando alterados no menu de pausa.
/// </summary>
public partial class SubtitleBox : CanvasLayer
{
    private PanelContainer _panel = null!;
    private Label _label = null!;
    private string _speakerKey = "";
    private string _textKey = "";
    private float _progress = 1.0f;
    private int _prefixLength;
    private int _bodyLength;
    private int _lastVisibleChars = -1;

    public override void _Ready()
    {
        _panel = GetNode<PanelContainer>("%Panel");
        _label = GetNode<Label>("%Text");
        ApplySettings();
        _panel.Visible = false;
    }

    public override void _EnterTree()
    {
        if (Settings.Instance != null)
        {
            Settings.Instance.Changed += ApplySettings;
        }
    }

    public override void _ExitTree()
    {
        if (Settings.Instance != null)
        {
            Settings.Instance.Changed -= ApplySettings;
        }
    }

    /// <summary>Exibe uma fala identificada por chaves de tradução e fração revelada (0..1).</summary>
    public void ShowLine(string speakerKey, string textKey, float progress = 1.0f)
    {
        _speakerKey = speakerKey;
        _textKey = textKey;
        _progress = Mathf.Clamp(progress, 0.0f, 1.0f);
        RefreshText();
        _panel.Visible = true;
    }

    /// <summary>Atualiza apenas a fração revelada da fala atual sem realocar texto.</summary>
    public void SetProgress(float progress)
    {
        float clamped = Mathf.Clamp(progress, 0.0f, 1.0f);
        if (Mathf.IsEqualApprox(_progress, clamped))
        {
            return;
        }
        _progress = clamped;
        if (_panel.Visible && IsInstanceValid(_label))
        {
            ApplyVisibleCharacters();
        }
    }

    /// <summary>Oculta a caixa de legenda.</summary>
    public void Clear()
    {
        _speakerKey = "";
        _textKey = "";
        _lastVisibleChars = -1;
        if (IsInstanceValid(_label))
        {
            _label.Text = "";
            _label.VisibleCharacters = -1;
        }
        if (IsInstanceValid(_panel))
        {
            _panel.Visible = false;
        }
    }

    private void ApplySettings()
    {
        if (!IsInstanceValid(_label) || Settings.Instance == null)
        {
            return;
        }
        _label.AddThemeFontSizeOverride("font_size", Settings.Instance.SubtitleFontSize);
        if (_panel.Visible)
        {
            RefreshText();
        }
    }

    private void RefreshText()
    {
        if (_textKey == "")
        {
            _label.Text = "";
            _label.VisibleCharacters = -1;
            _lastVisibleChars = -1;
            return;
        }
        string speaker = Tr(_speakerKey);
        string body = Tr(_textKey);
        _label.Text = $"{speaker}: {body}";
        _prefixLength = speaker.Length + 2;
        _bodyLength = body.Length;
        _lastVisibleChars = int.MinValue;
        ApplyVisibleCharacters();
    }

    private void ApplyVisibleCharacters()
    {
        // Revela somente o corpo após o nome do falante usando VisibleCharacters para manter quebra de linha estável.
        int visible = _progress >= 0.999f ? -1 : _prefixLength + Mathf.RoundToInt(_bodyLength * _progress);
        if (visible == _lastVisibleChars)
        {
            return;
        }
        _lastVisibleChars = visible;
        _label.VisibleCharacters = visible;
    }
}
