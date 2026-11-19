using System.Collections.Generic;
using FitaSemEtiqueta.Interaction;
using Godot;

namespace FitaSemEtiqueta.Tapes;

/// <summary>
/// The store's VCR and test TV. A tape put in the VCR slides in, the player stops to watch, and the tape
/// plays from where it is wound to: a returned tape shows the end credits (and, if someone recorded over
/// what came after, that recording); a rewound one, the film's title. Then blank tape and the blue screen,
/// and the VCR pushes the tape back out, to be picked up. Moving or the "interact" action stops it early.
/// The node sits on the VCR's slot, local -X out towards the attendant.
/// Needs a child CollisionShape3D over the VCR, on physics layer 5.
/// </summary>
public partial class TapeDeck : Area3D, IReceiver
{
    /// <summary>The TV's screen. While a tape plays its material is replaced by the picture.</summary>
    [Export] public MeshInstance3D Screen { get; set; } = null!;

    /// <summary>Where the eyes go to watch.</summary>
    [Export] public Node3D WatchEyes { get; set; } = null!;

    /// <summary>The tape's picture: a 320x240 viewport with the credits (2D) and the recorded shot (3D, own world).</summary>
    [Export] public SubViewport Feed { get; set; } = null!;

    /// <summary>The camera of the recorded shot, held by hand.</summary>
    [Export] public Camera3D ShotCamera { get; set; } = null!;

    /// <summary>Black background and text of the film's own picture, inside <see cref="Feed"/>.</summary>
    [Export] public Control Card { get; set; } = null!;

    [Export] public ShaderMaterial ScreenMaterial { get; set; } = null!;

    private enum Part { Black, Credits, Title, Shot, Snow, Blue }

    private enum State { Empty, Inserting, Playing, Ejecting, Out }

    // Seconds for the tape to slide in or out.
    private const float SlideTime = 0.35f;

    // The tape, lying flat in the slot: length (X) across, spine (+Y) out towards the attendant, thin side up.
    // Out: pushed out by the VCR, most of it outside. In: all inside.
    private static readonly Basis Lying = new(new Vector3(0, 0, -1), new Vector3(-1, 0, 0), new Vector3(0, 1, 0));
    private static readonly Vector3 OutAt = new(-0.0225f, 0, 0);
    private static readonly Vector3 InAt = new(0.06f, 0, 0);

    // Credits layout in the 320x240 picture, pixels: a line's height for a role and for a name,
    // the space after each pair, and how fast the roll goes up. The end title stops in the middle.
    private const int RoleHeight = 16;
    private const int NameHeight = 22;
    private const int PairGap = 18;
    private const float RollSpeed = 30f;
    private const float CreditsTime = 10f;

    private State _state = State.Empty;
    private Pickable? _tape;
    private IViewer? _viewer;
    private float _clock;
    private readonly List<(Part part, float length)> _program = new();
    private Transform3D _shotFrom;
    private float _shotFov;
    private Control _roll = null!;
    private Label _end = null!;
    private Label _title = null!;

    public override void _Ready()
    {
        _shotFrom = ShotCamera.Transform;
        _shotFov = ShotCamera.Fov;
        _roll = new Control();
        _end = Line("CREDITS_THE_END", 28);
        _title = Line("", 22);
        _title.Position = new Vector2(0, 120 - _title.Size.Y / 2);
        Card.AddChild(_roll);
        Card.AddChild(_title);
        ScreenMaterial.SetShaderParameter("picture", Feed.GetTexture());
        Feed.RenderTargetUpdateMode = SubViewport.UpdateMode.Disabled;
    }

    public bool Accepts(Pickable item, out string hint)
    {
        bool takes = TapeInfo.Of(item) != null;
        hint = takes ? Tr("INTERACT_TEST") : "";
        return takes;
    }

    // With a tape already in or sticking out, there is no room: nothing happens, and nothing is dropped on it.
    public Transform3D? SpotFor(Pickable item, Vector3 aimedAt) =>
        _state == State.Empty ? new Transform3D(GlobalBasis * Lying, GlobalTransform * OutAt) : null;

    public void Receive(Pickable item, Interactor interactor)
    {
        _tape = item;
        _viewer = interactor.Viewer;
        item.SetReachable(false);
        _state = State.Inserting;
        _clock = 0;
    }

    public override void _Process(double delta)
    {
        _clock += (float)delta;
        switch (_state)
        {
            case State.Inserting:
                if (!IsInstanceValid(_tape))
                {
                    // Gone (a level unloading): the slot is free again.
                    _tape = null;
                    _state = State.Empty;
                    break;
                }
                Slide(_clock / SlideTime);
                if (_clock >= SlideTime)
                {
                    Play();
                }
                break;
            case State.Playing:
                ShowProgram();
                break;
            case State.Ejecting:
                Slide(1 - _clock / SlideTime);
                if (_clock >= SlideTime)
                {
                    _state = State.Out;
                    if (IsInstanceValid(_tape))
                    {
                        _tape!.SetReachable(true);
                    }
                }
                break;
            case State.Out:
                // Picked up again: the slot is free.
                if (!IsInstanceValid(_tape) || _tape!.GlobalPosition.DistanceTo(GlobalTransform * OutAt) > 0.01f)
                {
                    _tape = null;
                    _state = State.Empty;
                }
                break;
        }
    }

    /// <summary>Puts the tape between out (0) and in (1).</summary>
    private void Slide(float t)
    {
        if (!IsInstanceValid(_tape))
        {
            return;
        }
        float at = Mathf.SmoothStep(0, 1, Mathf.Clamp(t, 0, 1));
        _tape!.GlobalTransform = new Transform3D(GlobalBasis * Lying, GlobalTransform * OutAt.Lerp(InAt, at));
    }

    /// <summary>Lays out what this tape shows, from where it is wound to, and starts.</summary>
    private void Play()
    {
        TapeInfo tape = TapeInfo.Of(_tape!)!;
        _program.Clear();
        if (tape.Rewound)
        {
            _program.Add((Part.Black, 1f));
            _program.Add((Part.Title, 4f));
            _program.Add((Part.Black, 1f));
        }
        else
        {
            _program.Add((Part.Credits, CreditsTime));
            _program.Add((Part.Black, 1.2f));
            if (tape.Tampered)
            {
                _program.Add((Part.Shot, 10f));
                _program.Add((Part.Snow, 1.2f));
            }
            else
            {
                _program.Add((Part.Snow, 1.5f));
            }
        }
        _program.Add((Part.Blue, 1.5f));

        BuildCredits(tape);
        _title.Text = tape.TitleKey;
        Screen.MaterialOverride = ScreenMaterial;
        _state = State.Playing;
        _clock = 0;

        Vector3 screen = Screen.GlobalTransform * Screen.GetAabb().GetCenter();
        _viewer?.Watch(WatchEyes.GlobalPosition, screen, Stopped);
    }

    /// <summary>Shows the part of the program the clock is in; at its end, stops watching.</summary>
    private void ShowProgram()
    {
        float start = 0;
        foreach ((Part part, float length) in _program)
        {
            if (_clock < start + length)
            {
                ShowPart(part, _clock - start, start);
                return;
            }
            start += length;
        }

        // Played to the end: tested, and only now may it be rewound (see TapeInfo.Tested).
        if (IsInstanceValid(_tape))
        {
            TapeInfo.Of(_tape!)!.Tested = true;
        }

        if (_viewer != null)
        {
            _viewer.StopWatching();
        }
        else
        {
            Stopped();
        }
    }

    private void ShowPart(Part part, float t, float start)
    {
        bool picture = part is Part.Black or Part.Credits or Part.Title or Part.Shot;
        Feed.RenderTargetUpdateMode = picture ? SubViewport.UpdateMode.Always : SubViewport.UpdateMode.Disabled;
        Feed.Disable3D = part != Part.Shot;
        Card.Visible = part != Part.Shot;
        _roll.Visible = part == Part.Credits;
        _title.Visible = part == Part.Title;

        if (part == Part.Credits)
        {
            // Up from below the picture until the end title reaches the middle, then it stays.
            float endTop = _end.Position.Y;
            float stop = 120 - _end.Size.Y / 2 - endTop;
            _roll.Position = new Vector2(0, Mathf.Max(240 - RollSpeed * t, stop));
        }
        else if (part == Part.Shot)
        {
            Handheld(t);
        }

        ScreenMaterial.SetShaderParameter("segment", part switch
        {
            Part.Shot => 1,
            Part.Snow => 2,
            Part.Blue => 3,
            _ => 0,
        });
        ScreenMaterial.SetShaderParameter("since_splice", part == Part.Shot ? t : 100f);
        ScreenMaterial.SetShaderParameter("frame", Mathf.Floor(_clock * 30));
        ScreenMaterial.SetShaderParameter("time_s", _clock);
    }

    /// <summary>A camera held by hand: a slow drift with a small shake on top, and a slow zoom in.</summary>
    private void Handheld(float t)
    {
        var drift = new Vector3(
            0.03f * Mathf.Sin(t * 0.9f) + 0.011f * Mathf.Sin(t * 2.3f) + 0.004f * Mathf.Sin(t * 5.1f),
            0.02f * Mathf.Sin(t * 0.7f + 1) + 0.008f * Mathf.Sin(t * 2.9f),
            0);
        var turn = new Basis(Vector3.Up, 0.012f * Mathf.Sin(t * 0.6f))
            * new Basis(Vector3.Right, 0.008f * Mathf.Sin(t * 0.8f + 2))
            * new Basis(Vector3.Back, 0.01f * Mathf.Sin(t * 0.8f));
        ShotCamera.Transform = new Transform3D(_shotFrom.Basis * turn, _shotFrom * drift);
        ShotCamera.Fov = _shotFov * (1 - 0.012f * t);
    }

    /// <summary>Watching stopped, at the end or by the player: the TV goes off and the tape comes out.</summary>
    private void Stopped()
    {
        Screen.MaterialOverride = null;
        Feed.RenderTargetUpdateMode = SubViewport.UpdateMode.Disabled;
        _state = State.Ejecting;
        _clock = 0;
    }

    private void BuildCredits(TapeInfo tape)
    {
        foreach (Node child in _roll.GetChildren())
        {
            _roll.RemoveChild(child);
            if (child != _end)
            {
                child.QueueFree();
            }
        }

        float y = 0;
        for (int i = 0; i + 1 < tape.Credits.Length; i += 2)
        {
            Label role = Line(tape.Credits[i], 11);
            role.Position = new Vector2(0, y);
            _roll.AddChild(role);
            Label name = Line(tape.Credits[i + 1], 15);
            name.Position = new Vector2(0, y + RoleHeight);
            _roll.AddChild(name);
            y += RoleHeight + NameHeight + PairGap;
        }
        _end.Position = new Vector2(0, y + PairGap);
        _roll.AddChild(_end);
    }

    /// <summary>A line of centred white text across the picture, by translation key.</summary>
    private static Label Line(string key, int size)
    {
        var label = new Label
        {
            Text = key,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Size = new Vector2(320, size + 6),
        };
        label.AddThemeFontSizeOverride("font_size", size);
        label.AddThemeColorOverride("font_color", new Color(0.92f, 0.92f, 0.9f));
        return label;
    }
}
