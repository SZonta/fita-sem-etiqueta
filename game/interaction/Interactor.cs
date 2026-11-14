using Godot;

namespace FitaSemEtiqueta.Interaction;

/// <summary>
/// The player's reach: a ray out of the camera. Whatever it hits that implements
/// <see cref="IInteractable"/> can be used with the "interact" action.
/// Also carries one <see cref="Pickable"/> at a time: the player bends down as far as needed,
/// the hand reaches the item where it lies and closes on it, and brings it up into view.
/// </summary>
public partial class Interactor : RayCast3D
{
    /// <summary>Emitted with the translation key of the hint to show, or "" for none.</summary>
    [Signal] public delegate void PromptChangedEventHandler(string key);

    // Physics layer 1: walls, floor and furniture.
    private const uint WorldMask = 1;

    // Physics layer 3: loose objects.
    private const uint PropsMask = 4;

    // How far off the exact line of sight something can be and still be aimed at, metres.
    private const float AimRadius = 0.08f;

    // Gap kept between a carried item and the wall it is pulled back from, in metres.
    private const float WallGap = 0.02f;

    // Seconds to bend down and stretch the arm to the item, to close the hand on it,
    // and to stand up with it. Then, after letting go, for the arm to drop and the hand to open.
    private const float ReachTime = 0.45f;
    private const float CloseTime = 0.15f;
    private const float LiftTime = 0.45f;
    private const float ReturnTime = 0.3f;

    // The carried item follows the view up and down only between these angles. Looking further
    // down it would end up inside the belly; looking further up, out of the arm's reach.
    private static readonly float LowestHold = Mathf.DegToRad(-55);
    private static readonly float HighestHold = Mathf.DegToRad(15);

    // Reaching, the palm comes down on top of the item, fingers pointing ahead. Holding, the palm is
    // against the item's right-hand end and the fingers wrap its back, as one holds a tape to read
    // its label. On the way up the hand turns from one to the other. Gaps to the middle of the palm, metres.
    private const float PalmAbove = 0.015f;
    private const float PalmToEnd = 0.025f;

    // Holding, the fingers point away from the eyes and up by this much, along the forearm, which comes
    // from below: the wrist stays nearly straight, and the thumb falls on the front of the item.
    private static readonly float FingerLift = Mathf.DegToRad(40);

    // The hand may stop this short of an item, metres: the item slides the rest of the way into it while
    // the fingers close. Without it, the reach on the floor is a strip a few centimetres wide.
    private const float MeetSlack = 0.1f;

    // The hand starts this far above the item and comes down onto it as the arm stretches, metres.
    private const float Approach = 0.35f;
    // Only an item more than this far below the eyes is reached from above (see Hold), metres.
    private const float ApproachBelowEyes = 0.45f;

    // Furthest the item goes to follow a hand that falls short of it, metres.
    private const float MaxFollow = 0.2f;

    // On the way up the item first rises off what it lay on, along an arc this high at the middle, metres.
    // Straight to the hand, it would be dragged across the surface.
    private const float LiftArc = 0.1f;

    // How closed the hand is when it first takes the item. It closes the rest of the way on the way up.
    private const float FirstGrip = 0.3f;

    // From the wrist to the middle of Nondas's palm, metres. The arm places the wrist.
    private const float PalmLength = 0.07f;

    private Node3D _holdPoint = null!;
    private ShapeCast3D _wideAim = null!;
    private Transform3D _holdRest;
    private Pickable? _held;
    private PhysicsShapeQueryParameters3D _sweep = null!;
    private string _prompt = "";

    // Time since the item was taken and where it lay. The middle of the palm from the item's centre:
    // reaching (in the world) and holding (in the item's own space as seen from the eyes).
    private float _clock;
    private Transform3D _from;
    private Vector3 _palmOnTop;
    private Vector3 _palmAtEnd;

    // Where the wrist was sent last frame, to compare with where it got to.
    private Vector3 _lastWristTarget;

    // How far above the item the hand starts (see Approach), and where the wrist was last sent, in the
    // view's space: after letting go, the arm drops back from there as the player moves on.
    private float _approach;
    private Transform3D _gripInView = Transform3D.Identity;

    /// <summary>The body whose hand picks things up, given by whoever owns it. Without it, nothing can be picked up.</summary>
    public IReacher? Body { get; set; }

    /// <summary>Where the right wrist goes, turned as the hand should be (Y along the fingers, Z out of the palm).</summary>
    public Node3D Grip { get; } = new()
    {
        Name = "Grip",
        TopLevel = true,
        // Placed every frame with the camera. The arm reads its target through physics interpolation,
        // which would hold it back to the last physics tick: walking, the hand would shake.
        PhysicsInterpolationMode = PhysicsInterpolationModeEnum.Off,
    };

    /// <summary>How far the arm is stretched to <see cref="Grip"/>: 0 hanging, 1 there.</summary>
    public float ArmReach { get; private set; }

    /// <summary>How closed the hand is: 0 open, 1 gripping.</summary>
    public float HandClosed { get; private set; }

    /// <summary>How deep the body stoops for the item being picked up (0 to 1).</summary>
    public float Depth { get; private set; }

    /// <summary>How far through bending down and up again: 0 upright, 1 down at <see cref="Depth"/>.</summary>
    public float Bend { get; private set; }

    /// <summary>True while picking something up: the player stands still meanwhile.</summary>
    public bool Busy => _held != null && !InHand;

    // True once the item is up in the hand. Until then it cannot be let go.
    private bool InHand => _clock >= ReachTime + CloseTime + LiftTime;

    public override void _Ready()
    {
        _holdPoint = GetNode<Node3D>("HoldPoint");
        _holdRest = _holdPoint.Transform;
        AddChild(Grip);

        _wideAim = new ShapeCast3D
        {
            Shape = new SphereShape3D { Radius = AimRadius },
            TargetPosition = TargetPosition,
            CollisionMask = PropsMask,
        };
        AddChild(_wideAim);
    }

    public override void _PhysicsProcess(double delta)
    {
        ForgetFreedItem();
        IInteractable? target = _held == null ? Aimed() : null;

        // Only what the hand can reach from here, and not while the arm is still dropping back.
        if (target is Pickable item && (ArmReach > 0 || StoopFor(item) < 0))
        {
            target = null;
        }

        SetPrompt(_held == null ? target?.PromptKey ?? "" : InHand ? "INTERACT_DROP" : "");

        if (!Input.IsActionJustPressed("interact"))
        {
            return;
        }

        if (_held == null)
        {
            target?.Interact(this);
        }
        else if (InHand)
        {
            _held.Release();
            _held = null;
        }
    }

    /// <summary>
    /// What the player is looking at. The ray is exact, and easily slips past something small or flat
    /// (a tape lying on the floor is 2.5 cm tall). When it finds nothing to use, a ball as wide as
    /// <see cref="AimRadius"/> swept along it takes the first loose object it touches, if nothing
    /// solid stands between it and the eyes.
    /// </summary>
    private IInteractable? Aimed()
    {
        if (GetCollider() is IInteractable exact)
        {
            return exact;
        }

        for (int i = 0; i < _wideAim.GetCollisionCount(); i++)
        {
            if (_wideAim.GetCollider(i) is IInteractable near and Node3D body)
            {
                var sight = PhysicsRayQueryParameters3D.Create(GlobalPosition, body.GlobalPosition, WorldMask);
                if (GetWorld3D().DirectSpaceState.IntersectRay(sight).Count == 0)
                {
                    return near;
                }
            }
        }
        return null;
    }

    /// <summary>
    /// Bends down, reaches, closes the hand and stands up with the item, then keeps it in view.
    /// The hold point is kept out of the walls: the item is swept from the eyes towards it and
    /// stops where it would touch something, like an arm folding back.
    /// Runs every frame because the camera moves every frame.
    /// </summary>
    public override void _Process(double delta)
    {
        ForgetFreedItem();
        if (_held == null)
        {
            // Let go: the arm drops back and the hand opens, from wherever they were. The target goes on
            // with the view meanwhile, or walking away the arm would be pulled back towards it.
            float step = (float)delta / ReturnTime;
            ArmReach = Mathf.MoveToward(ArmReach, 0, step);
            HandClosed = Mathf.MoveToward(HandClosed, 0, step);
            Grip.GlobalTransform = GlobalTransform * _gripInView;
            return;
        }

        _clock += (float)delta;
        float down = Mathf.SmoothStep(0, 1, _clock / ReachTime);
        float grab = Mathf.SmoothStep(0, 1, (_clock - ReachTime) / CloseTime);
        float up = Mathf.SmoothStep(0, 1, (_clock - ReachTime - CloseTime) / LiftTime);
        HandClosed = grab * FirstGrip + up * (1 - FirstGrip);
        ArmReach = down;
        Bend = down - up;

        _holdPoint.Transform = new Transform3D(new Basis(Vector3.Right, HoldTilt()), Vector3.Zero) * _holdRest;
        Basis view = _holdPoint.GlobalBasis;

        Vector3 eyes = GlobalPosition;
        _sweep.Transform = new Transform3D(view, eyes);
        _sweep.Motion = _holdPoint.GlobalPosition - eyes;

        // First number: how far along the sweep the item can go without touching, from 0 to 1.
        float clearFraction = GetWorld3D().DirectSpaceState.CastMotion(_sweep)[0];
        var hold = new Transform3D(view, eyes + _sweep.Motion * clearFraction);

        // The item stays where it lay until the hand closes on it, then comes up with the hand.
        // Where the arm falls short (by up to MeetSlack, when the item lies
        // just out of reach; or halfway up, off the straight path) the item goes where the hand really is:
        // how far it missed last frame. First while the fingers close, so the item slides into the hand;
        // not at the end, where the hand reaches, and a sudden turn of the view would throw it off.
        Transform3D carried = _from.InterpolateWith(hold, up);
        carried.Origin += Vector3.Up * LiftArc * Mathf.Sin(Mathf.Pi * up);
        Vector3 path = carried.Origin;
        if (Body != null)
        {
            float follow = Mathf.Max(grab * (1 - up), 4 * up * (1 - up));
            // Never more than a hand's breadth or two: turning round mid-pick-up, the shoulder swings far
            // from the item, and following it all the way would pull the item through the furniture.
            carried.Origin += (Body.Wrist - _lastWristTarget).LimitLength(MaxFollow) * follow;
        }
        _held.GlobalTransform = carried;

        // Reaching: palm down, fingers pointing ahead. Holding: palm to the left, against the item's end,
        // fingers pointing away from the eyes and up (they curl round the item's back).
        Vector3 ahead = Ahead(view);
        var onTop = new Basis(ahead.Cross(Vector3.Down), ahead, Vector3.Down);
        Vector3 fingers = (-view.Z * Mathf.Cos(FingerLift) + view.Y * Mathf.Sin(FingerLift)).Normalized();
        var atEnd = new Basis(fingers.Cross(-view.X), fingers, -view.X);
        Basis hand = onTop.Slerp(atEnd, up);
        // The hand comes down onto the item from above, so it never cuts through the edge of what it lies on.
        Vector3 palm = path + _palmOnTop.Lerp(view * _palmAtEnd, up) + Vector3.Up * _approach * (1 - down);
        Grip.GlobalTransform = new Transform3D(hand, palm - hand.Y * PalmLength);
        _gripInView = GlobalTransform.AffineInverse() * Grip.GlobalTransform;
        _lastWristTarget = Grip.GlobalPosition;
    }

    /// <summary>An item freed by something else (a level unloading, say) is no longer in the hand.</summary>
    private void ForgetFreedItem()
    {
        if (_held != null && !IsInstanceValid(_held))
        {
            _held = null;
        }
    }

    /// <summary>Takes the item: the body bends down to it and brings it up.</summary>
    public void Hold(Pickable item)
    {
        _sweep = new PhysicsShapeQueryParameters3D
        {
            Shape = item.Shape,
            CollisionMask = WorldMask,
            Margin = WallGap,
        };

        _palmOnTop = PalmOnTop(item);
        _palmAtEnd = new Vector3(item.HalfSize.X + PalmToEnd, 0, 0);
        // Only something lying well below the eyes is reached from above: on a shelf at head height,
        // coming down from above would go through the shelf over it.
        _approach = item.GlobalPosition.Y < GlobalPosition.Y - ApproachBelowEyes ? Approach : 0;
        Depth = StoopFor(item);
        _from = item.GlobalTransform;
        _clock = 0;

        item.AttachTo(_holdPoint);
        _held = item;
    }

    /// <summary>
    /// How deep to stoop for the hand to reach the item where it lies, or to come within
    /// <see cref="MeetSlack"/> of it; -1 if out of reach.
    /// </summary>
    private float StoopFor(Pickable item)
    {
        Vector3 palm = item.GlobalPosition + PalmOnTop(item);
        return Body?.StoopFor(palm - Ahead(_holdPoint.GlobalBasis) * PalmLength, MeetSlack) ?? -1;
    }

    /// <summary>Level, where the eyes look: the fingers point this way when reaching.</summary>
    private static Vector3 Ahead(Basis view) => new Vector3(-view.Z.X, 0, -view.Z.Z).Normalized();

    /// <summary>From the item's centre to the middle of the palm laid on top of it, as it lies now.</summary>
    private static Vector3 PalmOnTop(Pickable item)
    {
        Basis turn = item.GlobalBasis.Orthonormalized();
        Vector3 half = item.HalfSize;
        float halfHeight = Mathf.Abs(turn.X.Y) * half.X + Mathf.Abs(turn.Y.Y) * half.Y + Mathf.Abs(turn.Z.Y) * half.Z;
        return Vector3.Up * (halfHeight + PalmAbove);
    }

    /// <summary>
    /// Turn that keeps the hold point between the lowest and highest angles,
    /// whatever angle the camera looks at (radians, about the camera's own X axis).
    /// </summary>
    private float HoldTilt()
    {
        float pitch = Mathf.Asin(-GlobalBasis.Z.Normalized().Y);
        return Mathf.Clamp(pitch, LowestHold, HighestHold) - pitch;
    }

    private void SetPrompt(string key)
    {
        if (key == _prompt)
        {
            return;
        }

        _prompt = key;
        EmitSignal(SignalName.PromptChanged, key);
    }
}
