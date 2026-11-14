using System.Collections.Generic;
using Godot;

namespace FitaSemEtiqueta.Vfx;

/// <summary>
/// A puddle on the floor. Loose objects and the player's feet leave rings where they touch
/// the water: one where they come down and one more for every stride they slide after that.
/// The Area3D shape is wider than the water; it only tells what is near.
/// </summary>
public partial class Puddle : Area3D
{
    // Same as MAX_RIPPLES in puddle.gdshader.
    private const int MaxRipples = 8;

    // Average distance from the centre to the water's edge, in metres.
    private const float Radius = 0.8f;

    private const float StrideLength = 0.5f;
    private const float RippleSeconds = 1.6f;

    // Anything higher above the water than this is not touching it: a foot in mid-step, a can in the air.
    // A foot is measured at the ankle, about 7 cm up when it stands on the floor.
    private const float TouchHeight = 0.1f;

    // For each ripple: X and Y are the spot on the puddle, Z goes from 0 (just made) to 1 (gone).
    private readonly Vector3[] _ripples = new Vector3[MaxRipples];
    private int _newest;

    // Where each thing touching the water made its last ripple.
    private readonly Dictionary<Node3D, Vector2> _lastRipple = new();

    private ShaderMaterial _material = null!;

    public override void _Ready()
    {
        _material = (ShaderMaterial)GetNode<MeshInstance3D>("Surface").MaterialOverride;
        _material.SetShaderParameter("radius", Radius);

        System.Array.Fill(_ripples, new Vector3(0, 0, 1));
        _material.SetShaderParameter("ripples", _ripples);

        BodyExited += body => _lastRipple.Remove(body);
        AreaExited += foot => _lastRipple.Remove(foot);
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!HasOverlappingBodies() && !HasOverlappingAreas())
        {
            return; // Nothing near: no lists to build. A street may have many puddles.
        }

        foreach (Node3D body in GetOverlappingBodies())
        {
            Touch(body);
        }

        foreach (Node3D foot in GetOverlappingAreas())
        {
            Touch(foot);
        }
    }

    private void Touch(Node3D thing)
    {
        Vector3 local = ToLocal(thing.GlobalPosition);
        var spot = new Vector2(local.X, local.Z);

        // Height in metres even on a scaled puddle (local.Y would be divided by the scale).
        float height = (thing.GlobalPosition - GlobalPosition).Dot(GlobalBasis.Y.Normalized());

        if (spot.Length() > Radius || height > TouchHeight)
        {
            _lastRipple.Remove(thing); // Near the puddle, but on dry floor or above the water.
            return;
        }

        if (!_lastRipple.TryGetValue(thing, out Vector2 last) || spot.DistanceTo(last) >= StrideLength)
        {
            _lastRipple[thing] = spot;
            AddRipple(spot);
        }
    }

    public override void _Process(double delta)
    {
        if (_ripples[_newest].Z >= 1)
        {
            return; // Still water: the newest ripple is gone, so all of them are.
        }

        for (int i = 0; i < MaxRipples; i++)
        {
            _ripples[i].Z += (float)delta / RippleSeconds;
        }

        _material.SetShaderParameter("ripples", _ripples);
    }

    private void AddRipple(Vector2 spot)
    {
        // Reuses the oldest slot.
        _newest = (_newest + 1) % MaxRipples;
        _ripples[_newest] = new Vector3(spot.X, spot.Y, 0);
    }
}
