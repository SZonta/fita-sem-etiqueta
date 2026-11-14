using System.Collections.Generic;
using Godot;

namespace FitaSemEtiqueta.Player;

/// <summary>
/// Closes the fingers of one hand around something held. Applied on top of the animation
/// like the arm's reach: <see cref="SkeletonModifier3D.Influence"/> 0 is the open, animated hand,
/// 1 is the closed grip.
/// </summary>
public partial class FingerCurl : SkeletonModifier3D
{
    // Bend of each joint at a full grip, degrees: knuckle, middle joint, fingertip joint.
    // Holding a tape by its end: the fingers bend at the knuckles and lie flat along its back.
    private static readonly float[] FingerBend = { 70, 15, 5 };
    private static readonly float[] ThumbBend = { 0, 25, 25 };

    private static readonly string[] Fingers = { "index", "middle", "ring", "pinky" };

    // Skeleton bone and the turn that closes it. Every finger joint of the MPFB rig
    // bends towards the palm around its own X axis.
    private readonly List<(int Bone, Quaternion Turn)> _joints = new();

    /// <summary>"l" or "r", as in the bone names.</summary>
    [Export] public string Side { get; set; } = "r";

    public override void _Ready()
    {
        Skeleton3D skeleton = GetSkeleton();
        foreach (string finger in Fingers)
        {
            AddJoints(skeleton, finger, FingerBend);
        }
        AddJoints(skeleton, "thumb", ThumbBend);
    }

    public override void _ProcessModificationWithDelta(double delta)
    {
        Skeleton3D skeleton = GetSkeleton();
        foreach ((int bone, Quaternion turn) in _joints)
        {
            skeleton.SetBonePoseRotation(bone, skeleton.GetBonePoseRotation(bone) * turn);
        }
    }

    private void AddJoints(Skeleton3D skeleton, string finger, float[] bend)
    {
        for (int i = 0; i < bend.Length; i++)
        {
            int bone = skeleton.FindBone($"{finger}_0{i + 1}_{Side}");
            _joints.Add((bone, new Quaternion(Vector3.Right, Mathf.DegToRad(bend[i]))));
        }
    }
}
