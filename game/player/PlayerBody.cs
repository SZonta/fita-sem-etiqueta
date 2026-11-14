using System.Collections.Generic;
using FitaSemEtiqueta.Interaction;
using Godot;

namespace FitaSemEtiqueta.Player;

/// <summary>
/// Nondas's own body, seen from inside: legs and arms that walk with the player.
/// The head is drawn as shadow only, so it never covers the camera.
/// Each foot carries an area the size of the shoe: puddles feel it, and it finds loose objects to kick.
/// The right arm can reach for a point and the right hand can close around what it holds.
/// </summary>
public partial class PlayerBody : Node3D, IReacher
{
    /// <summary>One area per foot, placed at the ankle and covering the shoe.</summary>
    public IReadOnlyList<Area3D> Feet => _feet;

    // How far the body stands behind the eyes, metres. Looking ahead it is about where a real
    // body would be, so the hand can hold things in sight. Looking down it moves back: closer,
    // the camera would see down the neck and the chest would fill half the view.
    private const float AheadOffset = 0.12f;
    private const float DownOffset = 0.32f;

    // Looking down between these angles the body moves from one offset to the other.
    // It is done before the feet come into view (about 50 degrees down), so they never seem to slide.
    private static readonly float OffsetBlendStart = Mathf.DegToRad(-10);
    private static readonly float OffsetBlendEnd = Mathf.DegToRad(-40);

    // Once a wall behind the body is gone, it moves back at most this fast, metres per second.
    // Towards a wall it moves at once, or it would sink into it.
    private const float SlideBackSpeed = 1f;

    // Rough outline of the body seen from above, around its centre (metres, -Z ahead): the hanging
    // arms make it wide at the middle, the back and the chest make it deep at the centre.
    // Every point is within 0.29 m of the centre, inside the player's capsule (0.3 m), so the sweep
    // never starts inside a wall.
    private static readonly Vector2[] Outline =
    {
        new(-0.28f, -0.06f), new(-0.28f, 0.03f), new(-0.15f, 0.11f), new(0.15f, 0.11f),
        new(0.28f, 0.03f), new(0.28f, -0.06f), new(0.15f, -0.12f), new(-0.15f, -0.12f),
    };

    // The outline covers the body from the knees to the shoulders. Lower down, a step or a kerb
    // the capsule's round bottom can stand over would stop the sweep.
    private const float OutlineBottom = 0.3f;
    private const float OutlineTop = 1.45f;

    // Physics layer 1: walls, floor and furniture.
    private const uint WorldMask = 1;
    private const float WallGap = 0.01f;

    // Bones of the reaching arm, and the point the elbow bends towards, in the body's own space:
    // to the right, low and behind, so the elbow points down and out like a real one.
    private const string ShoulderBone = "upperarm_r";
    private const string ElbowBone = "lowerarm_r";
    private const string WristBone = "hand_r";
    private static readonly Vector3 ElbowPole = new(0.5f, 0.5f, 0.4f);

    // From the shoulder to the wrist with the arm straight, metres: as far as the wrist reaches.
    private const float ArmLength = 0.49f;

    // The shoulder stays at least this far above the item, metres: the body leans over a counter,
    // not into it, and the arm reaches down onto the item instead of through what it lies on.
    private const float ShoulderAboveItem = 0.15f;

    // The "stoop" action is sampled at this many depths, evenly from standing to the deepest,
    // to know where the shoulder and the head are at any depth. Steps of the reach search: the same.
    private const int StoopSamples = 9;

    // Metres the body covers in one walk cycle (the forward gait in tools/blender/nondas_body.py).
    // Playing the cycle at speed / StrideLength keeps the feet from sliding when walking forward.
    private const float StrideLength = 1.05f;

    // How quickly the pose follows a change of direction, per second: higher turns sooner.
    private const float TurnRate = 10f;

    // Physics layer 4, "feet". They look for layer 3, "props".
    private const uint FeetLayer = 8;
    private const uint PropsMask = 4;

    // A shoe, roughly, in the foot bone's own axes (metres): X is the width, Y runs from the
    // heel to the toes along the bone, Z is the height (the bone's Z points down through the sole).
    private static readonly Vector3 ShoeSize = new(0.11f, 0.28f, 0.09f);

    // Animation tree parameters, set every frame.
    private static readonly StringName BlendPosition = "parameters/walk/blend_position";
    private static readonly StringName PaceScale = "parameters/pace/scale";
    private static readonly StringName StoopSeek = "parameters/stoop_at/seek_request";
    private static readonly StringName StoopBlend = "parameters/bend/blend_amount";

    // While stooping starts, the walk fades out over this share of the bend (a quarter: about 0.1 s).
    private const float WalkFadeShare = 0.25f;

    private readonly List<Area3D> _feet = new();
    private AnimationTree _animation = null!;
    private Vector2 _direction;
    private PhysicsShapeQueryParameters3D _sweep = null!;
    private float _room = DownOffset;
    private TwoBoneIK3D _reach = null!;
    private HandAim _aim = null!;
    private FingerCurl _grip = null!;

    // Where the right shoulder and the head are at each sampled depth of the stoop, in the body's own space.
    private readonly Vector3[] _stoopShoulder = new Vector3[StoopSamples];
    private readonly Vector3[] _stoopHead = new Vector3[StoopSamples];

    // Feet and turn of the last placement, for the reach check.
    private Vector3 _feetAt;
    private Basis _turn = Basis.Identity;

    /// <inheritdoc/>
    public Vector3 Wrist { get; private set; }

    public override void _Ready()
    {
        var model = GetNode<Node3D>("Model");
        model.GetNode<MeshInstance3D>("Nondas/Skeleton3D/Head").CastShadow =
            GeometryInstance3D.ShadowCastingSetting.ShadowsOnly;

        var skeleton = model.GetNode<Skeleton3D>("Nondas/Skeleton3D");
        var player = model.GetNode<AnimationPlayer>("AnimationPlayer");
        AddFeet(skeleton);
        AddRightHand(skeleton);
        int wrist = skeleton.FindBone(WristBone);
        skeleton.SkeletonUpdated += () => Wrist = skeleton.GlobalTransform * skeleton.GetBoneGlobalPose(wrist).Origin;
        SampleStoop(player, skeleton, model);
        _animation = CreateAnimationTree(player);
        _sweep = CreateSweep();
    }

    /// <summary>
    /// Stands the body under the eyes, turned with the view, behind them by an amount that depends on
    /// how far down the player looks (pitch in radians, negative is down). A wall behind or beside
    /// the body pushes it forwards, under the eyes, instead of letting it sink into the wall.
    /// While bending to pick something up (bend 0 to 1), it moves right under the eyes: the camera
    /// follows the head then, and the arm reaches as far as a real one.
    /// </summary>
    public void Place(Vector3 feet, float yaw, float pitch, float bend, double delta)
    {
        var basis = new Basis(Vector3.Up, yaw);
        _feetAt = feet;
        _turn = basis;

        // How far back the body can go before it touches something.
        Vector3 clear = SweepBack(basis, feet);
        float clearLength = clear.Length();
        _room = Mathf.Min(clearLength, Mathf.MoveToward(_room, clearLength, SlideBackSpeed * (float)delta));

        float wanted = Mathf.Lerp(AheadOffset, DownOffset, Mathf.SmoothStep(OffsetBlendStart, OffsetBlendEnd, pitch));
        wanted = Mathf.Lerp(wanted, 0, bend);
        GlobalTransform = new Transform3D(basis, feet + clear.Normalized() * Mathf.Min(wanted, _room));
    }

    /// <summary>
    /// How deep the body has to stoop (0 standing to 1 deepest) for the right wrist to reach this point,
    /// or -1 if it cannot. Measured from where the player stands now. The shallowest stoop that reaches
    /// it exactly wins; failing that, the shallowest that comes within <paramref name="slack"/> of it.
    /// </summary>
    public float StoopFor(Vector3 point, float slack)
    {
        // The point in the body's own space, with the body where it stands while reaching: under the eyes.
        Vector3 local = _turn.Inverse() * (point - _feetAt);
        float nearly = -1;
        for (int i = 0; i < StoopSamples; i++)
        {
            // Stooping, the shoulder only gets lower: once it would dip below what the item lies on, give up.
            // Standing (the first sample) the arm may reach up, to a shelf above the shoulder.
            if (i > 0 && _stoopShoulder[i].Y < local.Y + ShoulderAboveItem)
            {
                break;
            }

            float depth = i / (StoopSamples - 1f);
            float distance = _stoopShoulder[i].DistanceTo(local);
            if (distance <= ArmLength)
            {
                return depth;
            }
            if (nearly < 0 && distance <= ArmLength + slack)
            {
                nearly = depth;
            }
        }
        return nearly;
    }

    /// <summary>
    /// How far the head has moved, stooped this deep, from where it is standing,
    /// in the body's own space (x right, y up, z back). The camera follows it.
    /// </summary>
    public Vector3 HeadShift(float depth) => Sample(_stoopHead, depth) - _stoopHead[0];

    /// <summary>
    /// Bends down (depth 0 standing to 1 deepest). Weight 0 leaves the walk as it is;
    /// above a quarter the stoop has fully taken over.
    /// </summary>
    public void Stoop(float depth, float weight)
    {
        _animation.Set(StoopSeek, depth);
        _animation.Set(StoopBlend, Mathf.Min(1, weight / WalkFadeShare));
    }

    /// <summary>
    /// Sweeps the body's outline from under the eyes to its spot furthest back. A wall in the way
    /// stops it, and it slides along that wall for the rest of the way, like the player along a wall:
    /// with a wall almost beside it, the body still gets most of the way back.
    /// Returns how far it got from under the eyes.
    /// </summary>
    private Vector3 SweepBack(Basis basis, Vector3 feet)
    {
        PhysicsDirectSpaceState3D space = GetWorld3D().DirectSpaceState;
        Vector3 motion = basis.Z * DownOffset;

        _sweep.Transform = new Transform3D(basis, feet);
        _sweep.Motion = motion;
        float[] cast = space.CastMotion(_sweep);
        Vector3 moved = motion * cast[0];
        if (cast[0] >= 1)
        {
            return moved;
        }

        // Just past the touching point, to read the wall's normal.
        _sweep.Transform = new Transform3D(basis, feet + motion * cast[1]);
        _sweep.Motion = Vector3.Zero;
        Godot.Collections.Dictionary contact = space.GetRestInfo(_sweep);
        if (contact.Count == 0)
        {
            return moved;
        }

        _sweep.Transform = new Transform3D(basis, feet + moved);
        _sweep.Motion = (motion - moved).Slide((Vector3)contact["normal"]);
        return moved + _sweep.Motion * space.CastMotion(_sweep)[0];
    }

    /// <summary>
    /// The right wrist goes to this node's position, as far as the arm reaches, and the hand
    /// turns to its axes: Y along the fingers, Z out of the palm (see <see cref="HandAim"/>).
    /// </summary>
    public void SetHandTarget(Node3D target)
    {
        _reach.SetTargetNode(0, _reach.GetPathTo(target));
        _aim.Target = target;
    }

    /// <summary>
    /// How much the right arm reaches for its target (0 hangs as animated, 1 on the target),
    /// and how closed the right hand is (0 open, 1 gripping).
    /// </summary>
    public void PoseRightHand(float reach, float closed)
    {
        _reach.Influence = reach;
        _aim.Influence = reach;
        _grip.Influence = closed;
    }

    /// <summary>
    /// Poses the legs for how the player is moving, in the player's own space
    /// (-Z ahead, +X to the right). Called on every physics tick.
    /// </summary>
    public void Walk(Vector3 localVelocity, double delta)
    {
        var flat = new Vector2(localVelocity.X, -localVelocity.Z);

        // Below 1 m/s the walk fades into standing still.
        Vector2 target = flat.LimitLength(1);
        _direction = _direction.Lerp(target, 1 - Mathf.Exp(-TurnRate * (float)delta));

        _animation.Set(BlendPosition, _direction);
        _animation.Set(PaceScale, flat.Length() / StrideLength);
    }

    private void AddFeet(Skeleton3D skeleton)
    {
        foreach (string side in new[] { "l", "r" })
        {
            // The foot bone runs from the ankle to the toe joint ("ball"). The shoe is centred
            // halfway along it, which reaches from the heel to the tip of the toes.
            Transform3D ankle = skeleton.GetBoneGlobalRest(skeleton.FindBone("foot_" + side));
            Vector3 toeJoint = ankle.AffineInverse() * skeleton.GetBoneGlobalRest(skeleton.FindBone("ball_" + side)).Origin;
            var shoe = new CollisionShape3D
            {
                Shape = new BoxShape3D { Size = ShoeSize },
                Position = toeJoint / 2,
            };

            var area = new Area3D { CollisionLayer = FeetLayer, CollisionMask = PropsMask };
            area.AddChild(shoe);
            _feet.Add(area);

            var foot = new BoneAttachment3D { Name = "foot_" + side, BoneName = "foot_" + side };
            foot.AddChild(area);
            skeleton.AddChild(foot);
        }
    }

    /// <summary>
    /// Two-bone reach (shoulder and elbow turn, the wrist lands on the target), then the hand turns,
    /// then the fingers close. All run after the animation, in this order, and start switched off.
    /// </summary>
    private void AddRightHand(Skeleton3D skeleton)
    {
        var pole = new Node3D { Name = "ElbowPole", Position = ElbowPole };
        AddChild(pole);

        _reach = new TwoBoneIK3D { Name = "RightArmReach", Influence = 0 };
        skeleton.AddChild(_reach);
        _reach.SettingCount = 1;
        _reach.SetRootBoneName(0, ShoulderBone);
        _reach.SetMiddleBoneName(0, ElbowBone);
        _reach.SetEndBoneName(0, WristBone);
        _reach.SetPoleNode(0, _reach.GetPathTo(pole));

        _aim = new HandAim { Name = "RightHandAim", Side = "r", Influence = 0 };
        skeleton.AddChild(_aim);

        _grip = new FingerCurl { Name = "RightGrip", Side = "r", Influence = 0 };
        skeleton.AddChild(_grip);
    }

    /// <summary>
    /// Plays the "stoop" action at evenly spaced depths and notes where the shoulder and the head go,
    /// so <see cref="StoopFor"/> and <see cref="HeadShift"/> follow whatever the generator made.
    /// </summary>
    private void SampleStoop(AnimationPlayer player, Skeleton3D skeleton, Node3D model)
    {
        int shoulder = skeleton.FindBone(ShoulderBone);
        int head = skeleton.FindBone("head");
        player.Play("stoop");
        for (int i = 0; i < StoopSamples; i++)
        {
            player.Seek(i / (StoopSamples - 1.0), update: true);
            _stoopShoulder[i] = model.Transform * skeleton.GetBoneGlobalPose(shoulder).Origin;
            _stoopHead[i] = model.Transform * skeleton.GetBoneGlobalPose(head).Origin;
        }
        player.Stop();
    }

    private static Vector3 Sample(Vector3[] samples, float depth)
    {
        float at = Mathf.Clamp(depth, 0, 1) * (samples.Length - 1);
        int below = Mathf.Min((int)at, samples.Length - 2);
        return samples[below].Lerp(samples[below + 1], at - below);
    }

    /// <summary>A prism with the body's outline, standing on the feet, for the wall sweep in <see cref="Place"/>.</summary>
    private static PhysicsShapeQueryParameters3D CreateSweep()
    {
        var points = new List<Vector3>();
        foreach (Vector2 p in Outline)
        {
            points.Add(new Vector3(p.X, OutlineBottom, p.Y));
            points.Add(new Vector3(p.X, OutlineTop, p.Y));
        }

        return new PhysicsShapeQueryParameters3D
        {
            Shape = new ConvexPolygonShape3D { Points = points.ToArray() },
            CollisionMask = WorldMask,
            Margin = WallGap,
        };
    }

    /// <summary>
    /// One animation per direction, blended by the direction of travel (like a 2D joystick),
    /// then played faster or slower to match the speed. Over it, when picking something up,
    /// the stoop, held at a chosen depth (its time, from 0 to 1 s, is the depth).
    /// </summary>
    private AnimationTree CreateAnimationTree(AnimationPlayer player)
    {
        foreach (StringName name in player.GetAnimationList())
        {
            player.GetAnimation(name).LoopMode = name == "stoop" ? Animation.LoopModeEnum.None : Animation.LoopModeEnum.Linear;
        }

        // Sync keeps every cycle at the same moment, so blended steps line up.
        var walk = new AnimationNodeBlendSpace2D { SyncMode = AnimationNodeBlendSpace2D.SyncModeEnum.Independent };
        AddBlendPoint(walk, "idle", new Vector2(0, 0));
        AddBlendPoint(walk, "walk_forward", new Vector2(0, 1));
        AddBlendPoint(walk, "walk_backward", new Vector2(0, -1));
        AddBlendPoint(walk, "walk_left", new Vector2(-1, 0));
        AddBlendPoint(walk, "walk_right", new Vector2(1, 0));

        var root = new AnimationNodeBlendTree();
        root.AddNode("walk", walk);
        root.AddNode("pace", new AnimationNodeTimeScale());
        root.ConnectNode("pace", 0, "walk");

        // The stoop never plays on by itself (time scale 0): it only jumps to the depth asked for.
        root.AddNode("stoop", new AnimationNodeAnimation { Animation = "stoop" });
        root.AddNode("stoop_at", new AnimationNodeTimeSeek());
        root.AddNode("stoop_still", new AnimationNodeTimeScale());
        root.ConnectNode("stoop_at", 0, "stoop");
        root.ConnectNode("stoop_still", 0, "stoop_at");

        root.AddNode("bend", new AnimationNodeBlend2());
        root.ConnectNode("bend", 0, "pace");
        root.ConnectNode("bend", 1, "stoop_still");
        root.ConnectNode("output", 0, "bend");

        var tree = new AnimationTree { TreeRoot = root, AnimPlayer = "../AnimationPlayer" };
        player.GetParent().AddChild(tree);
        tree.Set("parameters/stoop_still/scale", 0f);
        return tree;
    }

    private static void AddBlendPoint(AnimationNodeBlendSpace2D space, string animation, Vector2 position)
    {
        space.AddBlendPoint(new AnimationNodeAnimation { Animation = animation }, position, name: animation);
    }
}
