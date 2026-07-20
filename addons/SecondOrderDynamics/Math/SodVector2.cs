#nullable enable

using Godot;

namespace SecondOrderDynamics.Math;

/// <summary>
/// Second order system for interpolating a 2D Vector.
/// </summary>
public class SodVector2 : SecondOrderDynamics<Vector2> {
  
  /// <inheritdoc />
  public SodVector2(float freq = 1, float zeta = 1, float response = 1, Vector2 x0 = default)
    : base(freq, zeta, response, x0) {
  }

  /// <inheritdoc />
  public SodVector2(SecondOrderDynamics.SodParams @params, Vector2 x0 = default)
    : base(@params, x0) {
  }

  /// <inheritdoc />
  public SodVector2() : this(1) {
  }

  /// <inheritdoc />
  public override Vector2 UpdateInternal(float delta, Vector2 x, Vector2? xd = null) {
    if (Params is null) return Y;

    if (xd is null) {
      xd = (x - XPrev) / delta;
      XPrev = x;
    }

    float k2Stable = Params.GetK2Stable(delta);
    
    Y += delta * Yd;
    Yd += delta * (x + Params.K3 * (Vector2)xd - Y - Params.K1 * Yd) / k2Stable;
    return Y;
  }

  /// <inheritdoc />
  public override bool IsValid(Vector2 value) {
    return value.IsFinite();
  }
}