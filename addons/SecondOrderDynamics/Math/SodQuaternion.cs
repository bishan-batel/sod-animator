#nullable enable

using Godot;

namespace SecondOrderDynamics.Math;

/// <summary>
/// Second Order System operating on a quaternion
/// </summary>
public class SodQuaternion : SecondOrderDynamics<Quaternion> {
  /// <inheritdoc />
  public override Quaternion Default => Quaternion.Identity;

  /// <inheritdoc />
  public SodQuaternion(SodParams? @params = null, Quaternion x0 = default)
    : base(@params, x0) {
  }

  /// <inheritdoc />
  public override Quaternion UpdateInternal(float delta, Quaternion x, Quaternion? xd = null) {
    if (Params is null || delta <= 0f) return Y;

    // unpack the Quaternion into Vector3 because 'Yd' in this case is not being stored as a 'quaternion' per-say
    Vector3 currentYdVel = new(Yd.X, Yd.Y, Yd.Z);
    Vector3 currentXdVel;

    if (xd is null) {
      // find rotation from XPrev to x and convert it to angular velocity representation
      currentXdVel = QuaternionToAngularVelocity(x * XPrev.Inverse(), delta);
      XPrev = x;
    }
    else {
      // convert to angular velocity representation
      currentXdVel = new Vector3(xd.Value.X, xd.Value.Y, xd.Value.Z);
    }

    // get the error vector (delta from our current rotation Y to the target rotation X)
    Vector3 error = QuaternionToRotationVector(x * Y.Inverse());

    // this is the same as every others
    // TODO: consider 

    float k2Stable = Params.GetK2Stable(delta);

    // 4. Update Angular Velocity using pure 3D vector space operations
    Vector3 acceleration = (error + Params.K3 * currentXdVel - Params.K1 * currentYdVel) / k2Stable;
    currentYdVel += delta * acceleration;

    // Pack the updated 3D angular velocity vector back into the base class Yd Quaternion
    Yd = new Quaternion(currentYdVel.X, currentYdVel.Y, currentYdVel.Z, 0f);

    // 5. Integrate Angular Velocity into the Output Quaternion (Y)
    Vector3 velocityStep = delta * currentYdVel;
    Quaternion stepQ = RotationVectorToQuaternion(velocityStep);

    // Apply the rotation step to our current state
    Y = (stepQ * Y).Normalized();

    return Y;
  }


  /// <inheritdoc />
  public override bool IsValid(Quaternion value) {
    return value.IsFinite() && value.LengthSquared() > 0;
  }


  /// <summary>
  /// Converts a minor rotation quaternion to an angular velocity vector in rad/s
  /// </summary>
  public static Vector3 QuaternionToAngularVelocity(Quaternion q, float delta) {
    float angle = q.GetAngle();
    Vector3 axis = q.GetAxis();

    // Keep angle bounded within the shortest path (-PI to PI)
    if (angle > Mathf.Pi) angle -= 2.0f * Mathf.Pi;

    if (Mathf.IsZeroApprox(angle)) return Vector3.Zero;

    return axis * (angle / delta);
  }

  /// <summary>
  /// Converts a quaternion into a 3D rotation vector w/ exponential map representation
  /// </summary>
  public static Vector3 QuaternionToRotationVector(Quaternion q) {
    float angle = q.GetAngle();
    Vector3 axis = q.GetAxis();

    if (angle > Mathf.Pi) angle -= 2.0f * Mathf.Pi;

    if (Mathf.IsZeroApprox(angle)) return Vector3.Zero;

    return axis * angle;
  }

  /// <summary>
  /// Converts a 3D rotation vector (exponential map) back into a step quaternion.
  /// </summary>
  public static Quaternion RotationVectorToQuaternion(Vector3 rotVec) {
    float angleRad = rotVec.Length();
    if (Mathf.IsZeroApprox(angleRad) || !rotVec.IsFinite()) return Quaternion.Identity;

    return new Quaternion(rotVec.Normalized(), angleRad);
  }
}