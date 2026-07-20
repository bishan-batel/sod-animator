#nullable enable
using Godot;

namespace SecondOrderDynamics.Math;

/// <summary>
/// Second Order System for a 3D Vector
/// </summary>
public class SodVector3 : SecondOrderDynamics<Vector3> {
  /// <inheritdoc />
  public SodVector3(float freq = 1, float zeta = 1, float response = 1, Vector3 x0 = default)
    : base(freq, zeta, response, x0) {
  }

  /// <inheritdoc />
  public SodVector3(SecondOrderDynamics.SodParams @params, Vector3 x0 = default)
    : base(@params, x0) {
  }

  /// <inheritdoc />
  public SodVector3() : this(1) {
  }

  /// <inheritdoc />
  public override Vector3 UpdateInternal(float delta, Vector3 x, Vector3? xd = null) {
    if (Params is null) return Y;

    if (xd is null) {
      xd = (x - XPrev) / delta;
      XPrev = x;
    }

    float k2Stable = Params.GetK2Stable(delta);

    Y += delta * Yd;
    Yd += delta * (x + Params.K3 * (Vector3)xd - Y - Params.K1 * Yd) / k2Stable;
    return Y;
  }

  /// <inheritdoc />
  public override bool IsValid(Vector3 value) {
    return value.IsFinite();
  }
}