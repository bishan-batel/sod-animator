#nullable enable

using System.Diagnostics.CodeAnalysis;

namespace SecondOrderDynamics.Math;

/// <summary>
/// Abstract provider for a second order dynamics system of some variable type T
/// Common usages would be:
/// - <see cref="SodFloat"/>
/// - <see cref="SodQuaternion"/>
/// - <see cref="SodVector2"/>
/// - <see cref="SodVector3"/>
/// </summary>
/// <typeparam name="T">The type of variable this system operates on</typeparam>
[SuppressMessage("ReSharper", "MemberCanBeProtected.Global")]
public abstract class SecondOrderDynamics<T> : ISecondOrderDynamics where T : struct {
  /// <inheritdoc />
  public SodParams? Params { get; set; }

  /// <summary>
  /// Current 'positional state' of our system
  /// </summary>
  public T Y;

  /// <summary>
  /// The last target value (called with <see cref="Update"/>)
  /// </summary>
  public T XPrev;

  /// <summary>
  /// The derivative of our 'positional state'
  /// </summary>
  public T Yd;

  /// <summary>
  /// Factory for a default value, this is used if the system ever blows up (<see cref="IsValid"/>), in which certain variables may be reset to this value.
  /// </summary>
  public virtual T Default => default;


  /// <summary>
  /// Constructor with a given SodParams and initial x value
  /// </summary>
  /// <param name="params"></param>
  /// <param name="x0"></param>
  protected SecondOrderDynamics(SodParams? @params, T x0) {
    Params = @params;
    ConstrainTo(x0);
  }

  /// <summary>
  /// Default constructor, note that this leaves Params null, which will prevent simulation
  /// </summary>
  protected SecondOrderDynamics() : this(null, default) {
  }


  /// <summary>
  /// Locks this system / snaps it to a given target position
  /// </summary>
  /// <param name="x"></param>
  public void ConstrainTo(T x) {
    XPrev = x;
    Y = x;
    Yd = default;
  }

  /// <summary>
  /// Internal update for the system, note this will not ensure that this system blows up to infinity or some invalid state.
  /// </summary>
  /// <param name="delta">Time in seconds since last update</param>
  /// <param name="x">Current target value</param>
  /// <param name="xd">The derivative of the target value, if null this will be numerically calculated</param>
  /// <returns></returns>
  public abstract T UpdateInternal(float delta, T x, T? xd = null);

  /// <summary>
  /// Updates this current system while ensuring that all state variables are kept finite / valid (determined by <see cref="IsValid"/>)
  /// </summary>
  /// <param name="delta">Time in second since last update</param>
  /// <param name="x">Current target value</param>
  /// <param name="xd">The derivative of the target value, if null this will be numerically calculated</param>
  /// <returns></returns>
  public T Update(float delta, T x, T? xd = null) {
    T v = UpdateInternal(delta, x, xd);

    if (!IsValid(Yd)) {
      Yd = Default;
    }

    if (!IsValid(Y)) {
      Y = Default;
    }

    if (!IsValid(XPrev)) {
      XPrev = x;
    }

    if (!IsValid(v)) {
      v = Default;
    }

    return v;
  }

  /// <summary>
  /// Checks if a value of this type T is a 'valid' state for this system to be in
  /// </summary>
  /// <param name="value">The given value to check validity</param>
  /// <returns></returns>
  public abstract bool IsValid(T value);

  /// <inheritdoc />
  public bool IsValidTypeErased(object? obj) {
    if (obj is T value) {
      return IsValid(value);
    }

    return false;
  }


  /// <inheritdoc />
  public object UpdateTypeErased(float delta, object x, object? xd = null) {
    return Update(delta, (T)x, xd is null ? null : (T)xd);
  }
}