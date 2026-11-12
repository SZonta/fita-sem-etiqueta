using System.Collections.Generic;
using Godot;

namespace FitaSemEtiqueta.Vfx;

/// <summary>
/// A puddle on the floor. Any body that moves through the water leaves rings:
/// one where it enters and one more for every stride after that.
/// The Area3D shape is wider than the water; it only tells which bodies are near.
/// </summary>
public partial class Puddle : Area3D
{
    // Same as MAX_RIPPLES in puddle.gdshader.
    private const int MaxRipples = 8;

    // Average distance from the centre to the water's edge, in metres.
    private const float Radius = 0.8f;

    private const float StrideLength = 0.5f;
    private const float RippleSeconds = 1.6f;

    // For each ripple: X and Y are the spot on the puddle, Z goes from 0 (just made) to 1 (gone).
    private readonly Vector3[] _ripples = new Vector3[MaxRipples];
    private int _newest;

    // Where each body in the water made its last ripple.
    private readonly Dictionary<Node3D, Vector2> _lastRipple = new();

    private ShaderMaterial _material = null!;

    public override void _Ready()
    {
        _material = (ShaderMaterial)GetNode<MeshInstance3D>("Surface").MaterialOverride;
        _material.SetShaderParameter("radius", Radius);

        System.Array.Fill(_ripples, new Vector3(0, 0, 1));
        _material.SetShaderParameter("ripples", _ripples);

        BodyExited += body => _lastRipple.Remove(body);
    }

    public override void _PhysicsProcess(double delta)
    {
        foreach (Node3D body in GetOverlappingBodies())
        {
            Vector3 local = ToLocal(body.GlobalPosition);
            var spot = new Vector2(local.X, local.Z);

            if (spot.Length() > Radius)
            {
                _lastRipple.Remove(body); // Near the puddle, but on dry floor.
                continue;
            }

            if (!_lastRipple.TryGetValue(body, out Vector2 last) || spot.DistanceTo(last) >= StrideLength)
            {
                _lastRipple[body] = spot;
                AddRipple(spot);
            }
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
