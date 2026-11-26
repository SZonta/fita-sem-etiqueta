using Godot;

namespace FitaSemEtiqueta.Interaction;

/// <summary>
/// Cadeira do refúgio na calçada do bar: permite sentar olhando exatamente o enquadramento
/// da primeira fita e beber cerveja (pressionando "interact") até levantar com WASD.
/// </summary>
public partial class RefugeChair : StaticBody3D, IInteractable
{
    private const float SipDuration = 1.4f;
    private const float ReenableDistance = 0.65f;

    [Export] public Marker3D SeatEyes { get; set; } = null!;
    [Export] public Marker3D LookTarget { get; set; } = null!;
    [Export] public Node3D? BeerCup { get; set; }
    [Export] public AudioStream? SipSound { get; set; }

    private CollisionShape3D _collider = null!;
    private AudioStreamPlayer3D _audio = null!;
    private Node3D? _player;
    private Transform3D _cupRest;
    private bool _hasCupRest;
    private bool _seated;
    private bool _pendingCollider;
    private float _sipClock = -1.0f;

    public string PromptKey => _seated ? "" : "INTERACT_SIT";

    public override void _Ready()
    {
        _collider = GetNode<CollisionShape3D>("CollisionShape3D");
        _audio = new AudioStreamPlayer3D
        {
            Name = "SipAudio",
            MaxDistance = 8.0f,
            UnitSize = 1.5f,
        };
        AddChild(_audio);

        if (BeerCup != null)
        {
            _cupRest = BeerCup.GlobalTransform;
            _hasCupRest = true;
        }
    }

    public void Interact(Interactor interactor)
    {
        if (_seated || interactor.Viewer == null)
        {
            return;
        }

        if (BeerCup != null && !_hasCupRest)
        {
            _cupRest = BeerCup.GlobalTransform;
            _hasCupRest = true;
        }

        _player = interactor.Viewer as Node3D;
        _seated = true;
        _pendingCollider = false;
        _collider.Disabled = true;

        interactor.Viewer.Watch(
            SeatEyes.GlobalPosition,
            LookTarget.GlobalPosition,
            OnStoodUp,
            OnSip,
            "INTERACT_DRINK_OR_STAND");
    }

    private void OnSip()
    {
        if (_sipClock >= 0.0f)
        {
            return;
        }

        _sipClock = 0.0f;
        if (SipSound != null && BeerCup != null)
        {
            _audio.GlobalPosition = BeerCup.GlobalPosition;
            _audio.Stream = SipSound;
            _audio.Play();
        }
    }

    private void OnStoodUp()
    {
        _seated = false;
        _sipClock = -1.0f;
        RestoreCup();
        _pendingCollider = true;
    }

    public override void _Process(double delta)
    {
        if (_pendingCollider)
        {
            bool clear = !IsInstanceValid(_player);
            if (!clear)
            {
                Vector3 flat = (_player!.GlobalPosition - GlobalPosition) with { Y = 0.0f };
                clear = flat.Length() > ReenableDistance;
            }
            if (clear)
            {
                _collider.Disabled = false;
                _pendingCollider = false;
            }
        }

        if (_sipClock < 0.0f || BeerCup == null || !_hasCupRest)
        {
            return;
        }

        _sipClock += (float)delta;
        if (_sipClock >= SipDuration)
        {
            _sipClock = -1.0f;
            RestoreCup();
            return;
        }

        float t = _sipClock / SipDuration;
        // Sobe suavemente ate perto da boca no referencial da cadeira e retorna a mesa.
        float arc = Mathf.Sin(Mathf.Pi * t);
        Vector3 sipTarget = SeatEyes.GlobalTransform * new Vector3(-0.08f, -0.16f, -0.26f);
        Transform3D lifted = new Transform3D(_cupRest.Basis, sipTarget);
        BeerCup.GlobalTransform = _cupRest.InterpolateWith(lifted, arc);
    }

    private void RestoreCup()
    {
        if (BeerCup != null && _hasCupRest)
        {
            BeerCup.GlobalTransform = _cupRest;
        }
    }
}
