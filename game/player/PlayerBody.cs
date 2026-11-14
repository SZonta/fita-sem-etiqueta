using System.Collections.Generic;
using Godot;

namespace FitaSemEtiqueta.Player;

/// <summary>
/// Nondas's own body, seen from inside: legs and arms that walk with the player.
/// The head is drawn as shadow only, so it never covers the camera.
/// Each foot carries an area the size of the shoe: puddles feel it, and it finds loose objects to kick.
/// </summary>
public partial class PlayerBody : Node3D
{
    /// <summary>One area per foot, placed at the ankle and covering the shoe.</summary>
    public IReadOnlyList<Area3D> Feet => _feet;

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

    // Animation tree parameters, set every physics tick.
    private static readonly StringName BlendPosition = "parameters/walk/blend_position";
    private static readonly StringName PaceScale = "parameters/pace/scale";

    private readonly List<Area3D> _feet = new();
    private AnimationTree _animation = null!;
    private Vector2 _direction;

    public override void _Ready()
    {
        var model = GetNode<Node3D>("Model");
        model.GetNode<MeshInstance3D>("Nondas/Skeleton3D/Head").CastShadow =
            GeometryInstance3D.ShadowCastingSetting.ShadowsOnly;

        AddFeet(model.GetNode<Skeleton3D>("Nondas/Skeleton3D"));
        _animation = CreateAnimationTree(model.GetNode<AnimationPlayer>("AnimationPlayer"));
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
    /// One animation per direction, blended by the direction of travel (like a 2D joystick),
    /// then played faster or slower to match the speed.
    /// </summary>
    private AnimationTree CreateAnimationTree(AnimationPlayer player)
    {
        foreach (StringName name in player.GetAnimationList())
        {
            player.GetAnimation(name).LoopMode = Animation.LoopModeEnum.Linear;
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
        root.ConnectNode("output", 0, "pace");

        var tree = new AnimationTree { TreeRoot = root, AnimPlayer = "../AnimationPlayer" };
        player.GetParent().AddChild(tree);
        return tree;
    }

    private static void AddBlendPoint(AnimationNodeBlendSpace2D space, string animation, Vector2 position)
    {
        space.AddBlendPoint(new AnimationNodeAnimation { Animation = animation }, position, name: animation);
    }
}
