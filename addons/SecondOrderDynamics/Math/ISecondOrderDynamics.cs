using System.Diagnostics.CodeAnalysis;
using Godot;

#nullable enable

namespace SecondOrderDynamics.Math;

/// <summary>
/// I
/// </summary>
[SuppressMessage("ReSharper", "MemberCanBeProtected.Global")]
public interface ISecondOrderDynamics {
  /// <summary>
  /// Parameters for the second order system (K1, K2, K3), for more info see <see cref="SodParams"/>
  /// </summary>
  [Export]
  public SodParams? Params { set; get; }

  /// <summary>
  /// Type erased version of <see cref="SecondOrderDynamics{T}.IsValid"/>, this is mainly for internal use in <see cref="SodVariant"/>
  /// </summary>
  /// <param name="obj">Object to validate</param>
  /// <returns></returns>
  public bool IsValidTypeErased(object? obj);

  /// <summary>
  /// Type erased version of <see cref="SecondOrderDynamics{T}.UpdateTypeErased"/>, this is mainly for internal use in <see cref="SodVariant"/>
  /// </summary>
  /// <param name="delta">Delta time in seconds</param>
  /// <param name="x">Target value</param>
  /// <param name="xd">Target value's derivative, if null this will be aproximated</param>
  /// <returns></returns>
  public object? UpdateTypeErased(float delta, object x, object? xd = null);
}