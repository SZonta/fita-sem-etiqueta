using System.Collections.Generic;
using FitaSemEtiqueta.Dialogue;
using FitaSemEtiqueta.Interaction;
using FitaSemEtiqueta.Player;
using Godot;

namespace FitaSemEtiqueta.Npc;

/// <summary>
/// Dona Neide na Noite 1: entra pela calçada sob a meia-porta, pede uma fita de romance no balcão,
/// espera o registro no caderno e a entrega da fita certa, agradece com fofoca e vai embora pela rua.
/// </summary>
public partial class CustomerNeide : AnimatableBody3D, IInteractable
{
    private enum State { Entering, ReadyToGreet, Greeting, Waiting, ReadyToThank, Thanking, Leaving, Gone }

    private const float WalkSpeed = 1.10f;
    private const float WalkCycleLength = 0.75f;
    private const float TurnSpeed = 7.0f;
    private const float BlendTime = 0.2f;
    private const float PlayerYieldAhead = 0.80f;
    private const float PlayerYieldBehind = -0.15f;
    private const float PlayerYieldSide = 0.60f;
    private const float SalonMinZ = -6.75f;

    [Export] public DialoguePlayer Dialogue { get; set; } = null!;
    [Export] public RentalLedger Ledger { get; set; } = null!;
    [Export] public CounterSpot Counter { get; set; } = null!;
    [Export] public PlayerController Player { get; set; } = null!;
    [Export] public AudioStream[] NeideVoice { get; set; } = [];
    [Export] public AudioStream[] NondasVoice { get; set; } = [];

    private static readonly Vector3[] EntryPath =
    [
        new(2.10f, -0.03f, 1.15f),
        new(2.10f, 0.0f, -1.65f),
        new(3.45f, 0.0f, -2.15f),
    ];

    private static readonly Vector3[] ExitPath =
    [
        new(2.10f, 0.0f, -1.65f),
        new(2.10f, -0.03f, 1.15f),
        new(-7.50f, -0.03f, 1.15f),
    ];

    private static readonly Vector3 CounterLookTarget = new(4.60f, 0.0f, -2.35f);

    private AnimationPlayer _anim = null!;
    private Node3D _mouth = null!;
    private CollisionShape3D _collider = null!;

    private State _state = State.Entering;
    private readonly Queue<Vector3> _waypoints = new();
    private string _currentAnim = "";
    private bool _gesturing;
    private bool _logged;
    private bool _handedOver;
    private bool _remindedLedger;

    public bool WorksWhileHolding => true;

    public bool IsFinished => _state == State.Gone;

    public string PromptKey => _state == State.Waiting && !Dialogue.IsPlaying ? "INTERACT_TALK_NEIDE" : "";

    public override void _Ready()
    {
        SyncToPhysics = false;
        _anim = GetNode<AnimationPlayer>("Model/AnimationPlayer");
        _mouth = GetNode<Node3D>("Mouth");
        _collider = GetNode<CollisionShape3D>("CollisionShape3D");

        foreach (string name in new[] { "idle", "walk", "talk" })
        {
            if (_anim.HasAnimation(name))
            {
                _anim.GetAnimation(name).LoopMode = Animation.LoopModeEnum.Linear;
            }
        }

        Ledger.Logged += OnLogged;
        Counter.TapeHandedOver += OnTapeHandedOver;
        Dialogue.Finished += OnDialogueFinished;
        Dialogue.GestureChanged += OnGesture;

        foreach (Vector3 pt in EntryPath)
        {
            _waypoints.Enqueue(pt);
        }
        PlayAnim("walk");
    }

    public override void _ExitTree()
    {
        if (Ledger != null)
        {
            Ledger.Logged -= OnLogged;
        }
        if (Counter != null)
        {
            Counter.TapeHandedOver -= OnTapeHandedOver;
        }
        if (Dialogue != null)
        {
            Dialogue.Finished -= OnDialogueFinished;
            Dialogue.GestureChanged -= OnGesture;
        }
    }

    public void Interact(Interactor interactor)
    {
        if (_state != State.Waiting || Dialogue.IsPlaying)
        {
            return;
        }
        string key = !_handedOver ? "N1_NEIDE_REMIND_TAPE" : "N1_NEIDE_REMIND_LEDGER";
        Dialogue.Play(NeideLine(key));
    }

    public override void _PhysicsProcess(double delta)
    {
        float dt = (float)delta;
        switch (_state)
        {
            case State.Entering:
                if (FollowPath(dt))
                {
                    _state = State.ReadyToGreet;
                    PlayAnim("idle");
                }
                break;

            case State.ReadyToGreet:
                FaceToward(CounterLookTarget, dt);
                if (!Player.Watching && Player.GlobalPosition.Z > SalonMinZ)
                {
                    StartGreeting();
                }
                break;

            case State.Greeting:
            case State.Waiting:
            case State.Thanking:
                FaceToward(CounterLookTarget, dt);
                PlayAnim(_gesturing ? "talk" : "idle");
                break;

            case State.ReadyToThank:
                FaceToward(CounterLookTarget, dt);
                PlayAnim(_gesturing ? "talk" : "idle");
                if (!Dialogue.IsPlaying && !Player.Watching)
                {
                    StartThanking();
                }
                break;

            case State.Leaving:
                if (FollowPath(dt))
                {
                    FinishVisit();
                }
                break;
        }
    }

    private void StartGreeting()
    {
        _state = State.Greeting;
        Dialogue.Play(
            NeideLine("N1_NEIDE_HELLO"),
            NondasLine("N1_NONDAS_HELLO"),
            NeideLine("N1_NEIDE_REQUEST"));
    }

    private void StartThanking()
    {
        _state = State.Thanking;
        Dialogue.Play(
            NeideLine("N1_NEIDE_THANKS"),
            NeideLine("N1_NEIDE_GOSSIP"),
            NondasLine("N1_NONDAS_BYE"));
    }

    private void OnLogged()
    {
        _logged = true;
        CheckReadyToThank();
    }

    private void OnTapeHandedOver(Pickable tape)
    {
        _handedOver = true;
        Counter.Active = false;
        if (!_logged && !_remindedLedger)
        {
            _remindedLedger = true;
            Dialogue.Play(NeideLine("N1_NEIDE_REMIND_LEDGER"));
        }
        CheckReadyToThank();
    }

    private void CheckReadyToThank()
    {
        if (_logged && _handedOver && _state == State.Waiting)
        {
            _state = State.ReadyToThank;
        }
    }

    private void OnDialogueFinished()
    {
        if (_state == State.Greeting)
        {
            _state = State.Waiting;
            Ledger.Activate();
            Counter.Active = true;
            CheckReadyToThank();
        }
        else if (_state == State.Thanking)
        {
            Counter.TakePlaced();
            _state = State.Leaving;
            _waypoints.Clear();
            foreach (Vector3 pt in ExitPath)
            {
                _waypoints.Enqueue(pt);
            }
            PlayAnim("walk");
        }
    }

    private void OnGesture(string speakerName, bool active)
    {
        if (speakerName == Name.ToString())
        {
            _gesturing = active;
        }
    }

    private bool FollowPath(float dt)
    {
        if (_waypoints.Count == 0)
        {
            return true;
        }

        Vector3 target = _waypoints.Peek();
        Vector3 toTarget = target - GlobalPosition;
        Vector3 flat = toTarget with { Y = 0.0f };
        float dist = flat.Length();
        if (dist < 0.04f)
        {
            GlobalPosition = target;
            _waypoints.Dequeue();
            return _waypoints.Count == 0;
        }

        Vector3 dir = flat / dist;
        FaceToward(GlobalPosition + dir, dt);

        // Pausa dentro da loja se o jogador estiver bem na frente para nunca empurrá-lo.
        if (GlobalPosition.Z < 0.2f && PlayerBlocksWay(dir))
        {
            PlayAnim("idle");
            return false;
        }

        float step = Mathf.Min(WalkSpeed * dt, dist);
        float nextY = Mathf.MoveToward(GlobalPosition.Y, target.Y, dt * 0.5f);
        Vector3 next = GlobalPosition + dir * step;
        next.Y = nextY;
        GlobalPosition = next;

        PlayAnim("walk", WalkSpeed / WalkCycleLength);
        return false;
    }

    private bool PlayerBlocksWay(Vector3 moveDir)
    {
        Vector3 toPlayer = (Player.GlobalPosition - GlobalPosition) with { Y = 0.0f };
        float ahead = toPlayer.Dot(moveDir);
        if (ahead <= PlayerYieldBehind || ahead >= PlayerYieldAhead)
        {
            return false;
        }
        float lateral = (toPlayer - moveDir * ahead).Length();
        return lateral < PlayerYieldSide;
    }

    private void FaceToward(Vector3 worldPoint, float dt)
    {
        Vector3 look = (worldPoint - GlobalPosition) with { Y = 0.0f };
        if (look.LengthSquared() < 0.0001f)
        {
            return;
        }
        float desiredYaw = Mathf.Atan2(-look.X, -look.Z);
        Rotation = new Vector3(0.0f, Mathf.LerpAngle(Rotation.Y, desiredYaw, 1.0f - Mathf.Exp(-TurnSpeed * dt)), 0.0f);
    }

    private void PlayAnim(string name, float speed = 1.0f)
    {
        if (!_anim.HasAnimation(name))
        {
            return;
        }
        _anim.SpeedScale = speed;
        if (_currentAnim == name)
        {
            return;
        }
        _currentAnim = name;
        _anim.Play(name, BlendTime);
    }

    private void FinishVisit()
    {
        _state = State.Gone;
        _collider.Disabled = true;
        Visible = false;
        ProcessMode = ProcessModeEnum.Disabled;
    }

    private DialogueLine NeideLine(string textKey) => new("SPEAKER_NEIDE", textKey, NeideVoice, _mouth, Name);

    private DialogueLine NondasLine(string textKey) => new("SPEAKER_NONDAS", textKey, NondasVoice);
}
