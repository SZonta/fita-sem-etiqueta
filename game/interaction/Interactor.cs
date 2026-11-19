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
    /// <summary>Emitted with the hint to show: a translation key, text already translated, or "" for none.</summary>
    [Signal] public delegate void PromptChangedEventHandler(string text);

    // Physics layer 1: walls, floor and furniture.
    private const uint WorldMask = 1;

    // Physics layer 3: loose objects.
    private const uint PropsMask = 4;

    // Physics layer 5: places a carried item can be put (see IReceiver).
    private const uint SlotsMask = 16;

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

    // Something solid this close above an item (a shelf board, a table top) covers it, metres: the hand
    // comes at it from the front instead of from above, and draws it out before bringing it up.
    private const float CoverHeight = 0.35f;
    // Furthest the item is drawn out from under a cover, and the steps tried up to it, metres.
    private const float MaxPullOut = 0.4f;
    private const float PullOutStep = 0.05f;
    // Share of the way up spent drawing the item out.
    private const float PullOutShare = 0.35f;
    // How far above its resting place the item is tested, so it does not count what it lies on, metres.
    // More than the room to spare on its way up (PathMargin).
    private const float RestGap = 0.02f;
    private const float PathMargin = 0.01f;
    // The frame-to-frame guard on the way up starts this far above the item, metres: sliding out along
    // the board it lay on, it touches that board.
    private const float GuardLift = 0.005f;

    // Share of the reach spent raising the hand from where it hung to the start of its approach.
    private const float RiseShare = 0.4f;

    // Where the wrist hung when the pick-up began, in the view's space: the reach starts from there.
    private Vector3 _hungInView;

    // Where the item was put last frame, in the world (see the guard in _Process).
    private Transform3D _lastCarried;
    // Pieces the way up is tested in, each swept whole (a thin board between two samples still counts).
    private const int PathSteps = 16;

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
    // Where the item in hand is being put, while it is; null otherwise.
    private IReceiver? _receiver;

    // True while watching, as of the last physics tick: the "interact" press that stops watching
    // must not also be read here (the viewer reads it first, in the same tick).
    private bool _wasWatching;
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

    // Where the hand starts from, next to the item (see Approach), and where the wrist was last sent, in the
    // view's space: after letting go, the arm drops back from there as the player moves on.
    private Vector3 _approach;

    // How far and which way the item is drawn out from under a cover before it rises; zero when uncovered.
    private Vector3 _pullOut;
    private Transform3D _gripInView = Transform3D.Identity;

    /// <summary>The body whose hand picks things up, given by whoever owns it. Without it, nothing can be picked up.</summary>
    public IReacher? Body { get; set; }

    /// <summary>Who stops to watch the test TV, given by whoever owns it. While watching, nothing is aimed at.</summary>
    public IViewer? Viewer { get; set; }

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

    /// <summary>True while picking something up or putting it somewhere: the player stands still meanwhile.</summary>
    public bool Busy => _held != null && (!InHand || _receiver != null);

    // From reaching for an item to having it up in the hand, seconds. Putting it somewhere plays it backwards.
    private const float TakeTime = ReachTime + CloseTime + LiftTime;

    // True once the item is up in the hand. Until then it cannot be let go.
    private bool InHand => _clock >= TakeTime;

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
        bool wasWatching = _wasWatching;
        _wasWatching = Viewer?.Watching == true;
        if (_wasWatching || wasWatching)
        {
            // The viewer takes the "interact" action to stop watching.
            SetPrompt(_wasWatching ? "INTERACT_STOP" : "");
            return;
        }

        if (_held != null)
        {
            Carry();
            return;
        }

        IInteractable? target = Aimed();

        // Only what the hand can reach from here, and not while the arm is still dropping back.
        if (target is Pickable item && (ArmReach > 0 || StoopFor(item.GlobalTransform, item.HalfSize) < 0))
        {
            target = null;
        }

        SetPrompt(target?.PromptKey ?? "");
        if (Input.IsActionJustPressed("interact"))
        {
            target?.Interact(this);
        }
    }

    /// <summary>
    /// With the item up in the hand: puts it where the player aims, if it goes there and the hand reaches;
    /// says so if it does not go there; otherwise lets it go. The hint also tells what the item is.
    /// </summary>
    private void Carry()
    {
        if (!InHand || _receiver != null)
        {
            SetPrompt("");
            return;
        }

        Pickable item = _held!;
        string action = Tr("INTERACT_DROP");
        bool drop = true;
        Transform3D? spot = null;
        (IReceiver? receiver, Vector3 aimedAt) = AimedReceiver();
        if (receiver != null)
        {
            if (receiver.Accepts(item, out string hint))
            {
                spot = receiver.SpotFor(item, aimedAt);
                if (spot is Transform3D at && StoopFor(at, item.HalfSize) >= 0)
                {
                    action = hint;
                }
                else
                {
                    // No room there, or out of reach: nothing to do, and the item is not dropped on top.
                    spot = null;
                    action = "";
                }
                drop = false;
            }
            else if (hint != "")
            {
                // It does not go here (a shelf of another genre): the hint says so, and nothing happens.
                action = hint;
                drop = false;
            }
        }

        string caption = item.Caption;
        SetPrompt(caption == "" || action == "" ? caption + action : caption + "\n" + action);
        if (!Input.IsActionJustPressed("interact"))
        {
            return;
        }

        if (spot is Transform3D target)
        {
            Put(receiver!, target);
        }
        else if (drop)
        {
            item.Release();
            _held = null;
        }
    }

    /// <summary>The place the item in hand could be put that the player looks at, and the point looked at.</summary>
    private (IReceiver?, Vector3) AimedReceiver()
    {
        var query = PhysicsRayQueryParameters3D.Create(GlobalPosition, ToGlobal(TargetPosition), WorldMask | SlotsMask);
        query.CollideWithAreas = true;
        Godot.Collections.Dictionary hit = GetWorld3D().DirectSpaceState.IntersectRay(query);
        if (hit.Count > 0 && hit["collider"].AsGodotObject() is IReceiver receiver)
        {
            return (receiver, (Vector3)hit["position"]);
        }
        return (null, Vector3.Zero);
    }

    /// <summary>
    /// Puts the item in hand at the spot: the pick-up played backwards. The body bends down, the hand brings
    /// the item to the spot along a way planned and swept as for picking it up, opens, and the arm comes back.
    /// </summary>
    private void Put(IReceiver receiver, Transform3D spot)
    {
        Pickable item = _held!;
        Vector3 ahead = Ahead(_holdPoint.GlobalBasis);
        bool low = spot.Origin.Y < GlobalPosition.Y - ApproachBelowEyes;
        _approach = !low ? Vector3.Zero : Covered(item.Shape, spot) ? -ahead * Approach : Vector3.Up * Approach;
        _palmOnTop = PalmOnTop(spot.Basis, item.HalfSize);
        Depth = StoopFor(spot, item.HalfSize);
        _from = spot;
        _pullOut = PullOut(item.Shape, ahead);
        _lastCarried = item.GlobalTransform;
        _clock = TakeTime;
        _receiver = receiver;
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

        if (_receiver == null)
        {
            _clock += (float)delta;
        }
        else if ((_clock -= (float)delta) <= 0)
        {
            LetGoAtSpot();
            return;
        }

        float down = Mathf.SmoothStep(0, 1, _clock / ReachTime);
        float grab = Mathf.SmoothStep(0, 1, (_clock - ReachTime) / CloseTime);
        float up = Mathf.SmoothStep(0, 1, (_clock - ReachTime - CloseTime) / LiftTime);
        HandClosed = grab * FirstGrip + up * (1 - FirstGrip);
        // The arm follows its target fully early on: the target starts where the hand hangs (see below).
        ArmReach = Mathf.Min(1, down / RiseShare);
        Bend = down - up;

        Transform3D hold = HoldPlace();
        Basis view = hold.Basis;

        // The item stays where it lay until the hand closes on it, then comes up with the hand.
        // Where the arm falls short (by up to MeetSlack, when the item lies
        // just out of reach; or halfway up, off the straight path) the item goes where the hand really is:
        // how far it missed last frame. First while the fingers close, so the item slides into the hand;
        // not at the end, where the hand reaches, and a sudden turn of the view would throw it off.
        Transform3D carried = Carried(hold, up);
        Vector3 path = carried.Origin;
        float follow = Mathf.Max(grab * (1 - up), 4 * up * (1 - up));
        if (Body != null && follow > 0)
        {
            // Never more than a hand's breadth or two: turning round mid-pick-up, the shoulder swings far
            // from the item, and following it all the way would pull the item through the furniture.
            // Nor through what it lies on or next to: the shift stops where the item would touch something.
            // Swept from a little above, so what it lies on does not stop it from the start.
            Vector3 lifted = carried.Origin + Vector3.Up * RestGap;
            Vector3 shift = carried.Origin + (Body.Wrist - _lastWristTarget).LimitLength(MaxFollow) * follow - lifted;
            var sweep = new PhysicsShapeQueryParameters3D
            {
                Shape = _held.Shape,
                CollisionMask = WorldMask,
                Transform = new Transform3D(carried.Basis, lifted),
                Motion = shift,
            };
            carried.Origin = lifted + shift * GetWorld3D().DirectSpaceState.CastMotion(sweep)[0];
        }

        // Last guard, while the item moves and before it is in hand (then the hold point itself is kept out
        // of the walls): the way was planned from where the eyes were when the pick-up began, and the eyes
        // move as the body stoops, or the player turns. If the item, swept from where it was last frame
        // (as it was turned then), would touch something, or would be inside something where it gets to,
        // it stays where it was this frame. A sweep that starts inside something does not see it, so the
        // place it gets to is tested too. Tested a hair above, so what it lies on does not count.
        // Where it was is kept in the world: the item hangs from the hold point, which moves with the view.
        if ((grab > 0 || up > 0) && up < 1 && Blocked(_held.Shape, _lastCarried, carried))
        {
            carried = _lastCarried;
        }
        _held.GlobalTransform = carried;
        _lastCarried = carried;

        // Reaching: palm down, fingers pointing ahead. Holding: palm to the left, against the item's end,
        // fingers pointing away from the eyes and up (they curl round the item's back).
        Vector3 ahead = Ahead(view);
        var onTop = new Basis(ahead.Cross(Vector3.Down), ahead, Vector3.Down);
        Vector3 fingers = (-view.Z * Mathf.Cos(FingerLift) + view.Y * Mathf.Sin(FingerLift)).Normalized();
        var atEnd = new Basis(fingers.Cross(-view.X), fingers, -view.X);
        Basis hand = onTop.Slerp(atEnd, up);
        // First the hand rises from where it hung to the start of its approach, over whatever edge is in
        // between (a hand swinging straight up from the hip went through the edge of the counter). Then it
        // comes down onto the item from above, so it never cuts through the edge of what it lies on;
        // or, under a shelf board, from the front.
        Vector3 onItem = path + _palmOnTop.Lerp(view * _palmAtEnd, up);
        Vector3 palm;
        if (down < RiseShare)
        {
            Vector3 hung = GlobalTransform * _hungInView + hand.Y * PalmLength;
            palm = hung.Lerp(onItem + _approach, Mathf.SmoothStep(0, 1, down / RiseShare));
        }
        else
        {
            palm = onItem + _approach * (1 - (down - RiseShare) / (1 - RiseShare));
        }
        Grip.GlobalTransform = new Transform3D(hand, palm - hand.Y * PalmLength);
        _gripInView = GlobalTransform.AffineInverse() * Grip.GlobalTransform;
        _lastWristTarget = Grip.GlobalPosition;
    }

    /// <summary>The put is over: the item stays at its spot and the place it was put in takes it.</summary>
    private void LetGoAtSpot()
    {
        Pickable item = _held!;
        IReceiver receiver = _receiver!;
        _held = null;
        _receiver = null;
        ArmReach = 0;
        HandClosed = 0;
        Bend = 0;
        item.SetDown(_from);
        receiver.Receive(item, this);
    }

    /// <summary>
    /// Where the item is held in view now: at the hold point, or short of it where a wall is in the way.
    /// </summary>
    private Transform3D HoldPlace()
    {
        _holdPoint.Transform = new Transform3D(new Basis(Vector3.Right, HoldTilt()), Vector3.Zero) * _holdRest;
        Basis view = _holdPoint.GlobalBasis;

        Vector3 eyes = GlobalPosition;
        _sweep.Transform = new Transform3D(view, eyes);
        _sweep.Motion = _holdPoint.GlobalPosition - eyes;

        // First number: how far along the sweep the item can go without touching, from 0 to 1.
        float clearFraction = GetWorld3D().DirectSpaceState.CastMotion(_sweep)[0];
        return new Transform3D(view, eyes + _sweep.Motion * clearFraction);
    }

    /// <summary>An item freed by something else (a level unloading, say) is no longer in the hand.</summary>
    private void ForgetFreedItem()
    {
        if (_held != null && !IsInstanceValid(_held))
        {
            _held = null;
            _receiver = null;
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

        _palmOnTop = PalmOnTop(item.GlobalBasis, item.HalfSize);
        _palmAtEnd = new Vector3(item.HalfSize.X + PalmToEnd, 0, 0);
        // Only something lying well below the eyes is reached from above: on a shelf at head height,
        // coming down from above would go through the shelf over it. Under a cover, it is reached from the front.
        Vector3 ahead = Ahead(_holdPoint.GlobalBasis);
        bool low = item.GlobalPosition.Y < GlobalPosition.Y - ApproachBelowEyes;
        _approach = !low ? Vector3.Zero : Covered(item.Shape, item.GlobalTransform) ? -ahead * Approach : Vector3.Up * Approach;
        Depth = StoopFor(item.GlobalTransform, item.HalfSize);
        _from = item.GlobalTransform;
        _lastCarried = _from;
        _pullOut = PullOut(item.Shape, ahead);
        _hungInView = Body != null ? GlobalTransform.AffineInverse() * Body.Wrist : Vector3.Zero;
        _clock = 0;

        item.AttachTo(_holdPoint);
        _held = item;
    }

    /// <summary>
    /// How deep to stoop for the hand to reach an item lying at <paramref name="at"/>, or to come within
    /// <see cref="MeetSlack"/> of it; -1 if out of reach.
    /// </summary>
    private float StoopFor(Transform3D at, Vector3 halfSize)
    {
        Vector3 palm = at.Origin + PalmOnTop(at.Basis, halfSize);
        return Body?.StoopFor(palm - Ahead(_holdPoint.GlobalBasis) * PalmLength, MeetSlack) ?? -1;
    }

    /// <summary>
    /// Where the item is on its way from where it lay (up 0) to the hold point (up 1). Uncovered, it rises
    /// along an arc. Covered, it is first drawn out towards the player, level, then goes straight up to the hand.
    /// </summary>
    private Transform3D Carried(Transform3D hold, float up)
    {
        if (_pullOut == Vector3.Zero)
        {
            Transform3D arc = _from.InterpolateWith(hold, up);
            arc.Origin += Vector3.Up * LiftArc * Mathf.Sin(Mathf.Pi * up);
            return arc;
        }

        if (up < PullOutShare)
        {
            return new Transform3D(_from.Basis, _from.Origin + _pullOut * (up / PullOutShare));
        }
        var outside = new Transform3D(_from.Basis, _from.Origin + _pullOut);
        return outside.InterpolateWith(hold, (up - PullOutShare) / (1 - PullOutShare));
    }

    /// <summary>
    /// True if the item, moving from <paramref name="from"/> to <paramref name="to"/>, would touch something
    /// on the way or be inside something there (see the guard in <see cref="_Process"/>).
    /// </summary>
    private bool Blocked(Shape3D shape, Transform3D from, Transform3D to)
    {
        PhysicsDirectSpaceState3D space = GetWorld3D().DirectSpaceState;
        Vector3 lift = Vector3.Up * GuardLift;
        var guard = new PhysicsShapeQueryParameters3D
        {
            Shape = shape,
            CollisionMask = WorldMask,
            Transform = new Transform3D(from.Basis, from.Origin + lift),
            Motion = to.Origin - from.Origin,
        };
        if (space.CastMotion(guard)[0] < 1)
        {
            return true;
        }

        guard.Transform = new Transform3D(to.Basis, to.Origin + lift);
        guard.Motion = Vector3.Zero;
        return space.IntersectShape(guard, 1).Count > 0;
    }

    /// <summary>True if something solid lies within <see cref="CoverHeight"/> above an item at <paramref name="at"/> (a shelf board).</summary>
    private bool Covered(Shape3D shape, Transform3D at)
    {
        var rise = new PhysicsShapeQueryParameters3D
        {
            Shape = shape,
            CollisionMask = WorldMask,
            // Starting a little above where it lies: resting on a board, it already touches that board.
            Transform = new Transform3D(at.Basis, at.Origin + Vector3.Up * RestGap),
            Motion = Vector3.Up * CoverHeight,
        };
        return GetWorld3D().DirectSpaceState.CastMotion(rise)[0] < 1;
    }

    /// <summary>
    /// How far, towards the player, the item must be drawn out before rising for its whole way up
    /// to the hold point to be clear (as the hold point is now). Zero when the plain arc is clear,
    /// or when no way tried is.
    /// </summary>
    private Vector3 PullOut(Shape3D shape, Vector3 ahead)
    {
        Transform3D hold = HoldPlace();
        for (float d = 0; d <= MaxPullOut + 0.001f; d += PullOutStep)
        {
            _pullOut = -ahead * d;
            if (PathClear(shape, hold))
            {
                return _pullOut;
            }
        }
        return Vector3.Zero;
    }

    /// <summary>
    /// True if the item, swept piece by piece along its way up (see <see cref="Carried"/>), touches nothing.
    /// While it still slides on what it lay on, it is tested a little above the real way, so that does not count.
    /// </summary>
    private bool PathClear(Shape3D shape, Transform3D hold)
    {
        PhysicsDirectSpaceState3D space = GetWorld3D().DirectSpaceState;
        // A little room to spare: the camera, and with it the hold point, moves a bit as the body stoops.
        var sweep = new PhysicsShapeQueryParameters3D { Shape = shape, CollisionMask = WorldMask, Margin = PathMargin };
        float sliding = _pullOut == Vector3.Zero ? 1f / PathSteps : PullOutShare;
        Transform3D at = Carried(hold, 0);
        for (int k = 1; k <= PathSteps; k++)
        {
            Transform3D next = Carried(hold, k / (float)PathSteps);
            float lift = (k - 1f) / PathSteps < sliding ? RestGap : 0;
            sweep.Transform = new Transform3D(at.Basis, at.Origin + Vector3.Up * lift);
            sweep.Motion = next.Origin - at.Origin;
            if (space.CastMotion(sweep)[0] < 1)
            {
                return false;
            }

            // The sweep keeps the turn it starts with; the item also turns on the way, so test where it gets to.
            float nextLift = k / (float)PathSteps < sliding ? RestGap : 0;
            sweep.Transform = new Transform3D(next.Basis, next.Origin + Vector3.Up * nextLift);
            if (space.IntersectShape(sweep, 1).Count > 0)
            {
                return false;
            }
            at = next;
        }
        return true;
    }

    /// <summary>Level, where the eyes look: the fingers point this way when reaching.</summary>
    private static Vector3 Ahead(Basis view) => new Vector3(-view.Z.X, 0, -view.Z.Z).Normalized();

    /// <summary>From an item's centre to the middle of the palm laid on top of it, turned as <paramref name="basis"/>.</summary>
    private static Vector3 PalmOnTop(Basis basis, Vector3 half)
    {
        Basis turn = basis.Orthonormalized();
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
