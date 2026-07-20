#nullable enable

using Godot;

namespace SecondOrderDynamics.Math;

/// <summary>
/// Second Order System operating on a single number
/// </summary>
public class SodFloat : SecondOrderDynamics<float> {
  /// <summary>
  /// A flag that, when active, modifies the simulation to use angular differences - this is desirable when interpolating angles.
  /// For more complex rotations, consider the use of <see cref="SodQuaternion"/>
  /// </summary>
  public bool IsAngular { set; get; }

  /// <inheritdoc />
  public SodFloat(float freq = 1, float zeta = 1, float response = 1, float x0 = 0)
    : base(freq, zeta, response, x0) {
  }

  /// <inheritdoc />
  public SodFloat(SodParams @params, float x0 = 0)
    : base(@params, x0) {
  }

  /// <inheritdoc />
  public override float UpdateInternal(float delta, float x, float? xd = null) {
    return IsAngular
      ? _updateInternalAngular(delta, x, xd)
      : _updateInternalNormal(delta, x, xd);
  }

  float _updateInternalNormal(float delta, float x, float? xd = null) {
    if (Params is null) return Y;

    if (xd is null) {
      xd = (x - XPrev) / delta;
      XPrev = x;
    }

    float k2Stable = Params.GetK2Stable(delta);
    
    Y += delta * Yd;
    Yd += delta * (x + Params.K3 * (float)xd - Y - Params.K1 * Yd) / k2Stable;
    return Y;
  }

  float _updateInternalAngular(float delta, float x, float? xd = null) {
    if (Params is null) return Y;

    if (xd is null) {
      xd = Mathf.AngleDifference(XPrev, x) / delta;
      XPrev = x;
    }

    float k2Stable = Mathf.Max(Params.K2, Mathf.Max(delta * delta / 2f + delta * Params.K1 / 2f, delta * Params.K1));
    Y += delta * Yd;
    // Yd += delta * (x + Params.K3 * (float)xd - Y - Params.K1 * Yd) / k2Stable;
    Yd += delta * (x + Mathf.AngleDifference(Params.K1 * Yd, Mathf.AngleDifference(Y, Params.K3 * (float)xd))) / k2Stable;
    return Y;
  }

  /// <inheritdoc />
  public override bool IsValid(float value) {
    return float.IsFinite(value);
  }
}