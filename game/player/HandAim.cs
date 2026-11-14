using Godot;

namespace FitaSemEtiqueta.Player;

/// <summary>
/// Turns one hand to match a target node, after the arm has reached for it: the target's Y axis is
/// where the fingers point and its Z axis comes out of the palm.
/// <see cref="SkeletonModifier3D.Influence"/> 0 leaves the hand as the arm carries it.
/// </summary>
public partial class HandAim : SkeletonModifier3D
{
    /// <summary>"l" or "r", as in the bone names.</summary>
    [Export] public string Side { get; set; } = "r";

    /// <summary>Whose axes the hand takes. Nothing happens without one.</summary>
    public Node3D? Target { get; set; }

    private int _hand;
    private int _forearm;

    // Share of the hand's turn about the forearm that the forearm takes.
    private const float ForearmShare = 0.6f;

    // The hand's own axes as the target's: X across the back of the hand, Y along it, Z out of the palm.
    private Basis _palmFrame;

    // The hand's rest turn against the forearm.
    private Quaternion _handRest;

    public override void _Ready()
    {
        Skeleton3D skeleton = GetSkeleton();
        _hand = skeleton.FindBone("hand_" + Side);
        _forearm = skeleton.GetBoneParent(_hand);
        _handRest = skeleton.GetBoneRest(_hand).Basis.GetRotationQuaternion();

        // The bone runs along Y. The palm faces where the fingers curl to: every finger joint of the
        // MPFB rig bends towards its own Z axis (see FingerCurl). The middle finger's Z, in the hand's space,
        // straightened against Y, is out of the palm.
        Basis hand = skeleton.GetBoneGlobalRest(_hand).Basis;
        Basis middle = skeleton.GetBoneGlobalRest(skeleton.FindBone("middle_01_" + Side)).Basis;
        Vector3 palm = hand.Inverse() * middle.Z;
        palm = (palm - Vector3.Up * palm.Dot(Vector3.Up)).Normalized();
        _palmFrame = new Basis(Vector3.Up.Cross(palm), Vector3.Up, palm);
    }

    public override void _ProcessModificationWithDelta(double delta)
    {
        if (Target == null)
        {
            return;
        }

        Skeleton3D skeleton = GetSkeleton();
        Basis wanted = skeleton.GlobalBasis.Inverse() * Target.GlobalBasis.Orthonormalized() * _palmFrame.Inverse();
        Basis forearm = skeleton.GetBoneGlobalPose(_forearm).Basis;
        Quaternion hand = (forearm.Inverse() * wanted).GetRotationQuaternion();

        // A real wrist barely turns: the forearm does (pronation). The rig has no twist bones, so a hand
        // turned on its own wrings the skin of the wrist. The forearm takes its share of the turn about
        // its own length, which leaves the wrist where the arm put it.
        Quaternion bend = hand * _handRest.Inverse();
        if (bend.W < 0)
        {
            bend = -bend; // Same turn, the short way round: otherwise the roll comes out past 180 degrees.
        }
        float roll = 2 * Mathf.Atan2(bend.Y, bend.W);
        var share = new Quaternion(Vector3.Up, roll * ForearmShare);
        skeleton.SetBonePoseRotation(_forearm, skeleton.GetBonePoseRotation(_forearm) * share);
        skeleton.SetBonePoseRotation(_hand, share.Inverse() * hand);
    }
}
