using Godot;

namespace FitaSemEtiqueta.Interaction;

/// <summary>
/// A loose object the player can pick up, carry and let go.
/// Needs one child named "CollisionShape3D", centred on the body and not rotated.
/// </summary>
public partial class Pickable : RigidBody3D, IInteractable
{
    private Node _home = null!;
    private uint _collisionLayer;

    public string PromptKey => "INTERACT_PICK_UP";

    /// <summary>The solid outline of the object, in its own space.</summary>
    public Shape3D Shape => GetNode<CollisionShape3D>("CollisionShape3D").Shape;

    public void Interact(Interactor interactor) => interactor.Hold(this);

    /// <summary>Stops being a physics object and rides along with the holder.</summary>
    public void AttachTo(Node3D holder)
    {
        _home = GetParent();
        _collisionLayer = CollisionLayer;

        Freeze = true;
        CollisionLayer = 0; // Nothing bumps into it or aims at it while it is carried.

        Reparent(holder, keepGlobalTransform: false);
        Transform = Transform3D.Identity;
    }

    /// <summary>Goes back to the world, falling from where it was held.</summary>
    public void Release()
    {
        Reparent(_home);

        CollisionLayer = _collisionLayer;
        Freeze = false;
        Sleeping = false;

        // Back under physics interpolation: without this it would glide in from its old spot.
        ResetPhysicsInterpolation();
    }
}
