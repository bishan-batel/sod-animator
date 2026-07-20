#nullable enable
using System.Diagnostics;
using Godot;

namespace SecondOrderDynamics.Math;

/// <summary>
/// Second order system for animating (almost) any <see cref="Variant"/> type.
/// </summary>
/// <param name="params"></param>
/// <param name="x0"></param>
public class SodVariant(SodParams? @params, Variant x0) : SecondOrderDynamics<Variant>(@params, x0) {
  ISecondOrderDynamics? _sod;

  /// <inheritdoc />
  public override Variant Default {
    get {
      return _sod switch {
        SodFloat => 0,
        SodQuaternion => new Quaternion(),
        SodVector2 => new Vector2(),
        SodVector3 => new Vector3(),
        _ => default
      };
    }
  }

  /// <inheritdoc />
  public override Variant UpdateInternal(float delta, Variant x, Variant? xd = null) {
    if (Params is null || !IsValidType(x.VariantType) || xd.HasValue && !IsValidType(xd.Value.VariantType)) {
      return x;
    }


    if (_requiresReconstruction(x)) {
      _constructInnerSod(x);
    }

    _sod!.Params = Params;

    switch (_sod) {
      case SodFloat sod:
        Y = sod.UpdateInternal(delta, x.AsSingle(), xd?.AsSingle());
        Yd = sod.Yd;
        XPrev = sod.XPrev;
        break;
      case SodVector2 sod:
        Y = sod.UpdateInternal(delta, x.AsVector2(), xd?.AsVector2());
        Yd = sod.Yd;
        XPrev = sod.XPrev;
        break;
      case SodVector3 sod:
        Y = sod.UpdateInternal(delta, x.AsVector3(), xd?.AsVector3());
        Yd = sod.Yd;
        XPrev = sod.XPrev;
        break;
      case SodQuaternion sod:
        Y = sod.UpdateInternal(delta, x.AsQuaternion(), xd?.AsQuaternion());
        Yd = sod.Yd;
        XPrev = sod.XPrev;
        break;
      default:
        throw new UnreachableException();
    }

    return Y;
  }

  bool _requiresReconstruction(Variant x) {
    if (_sod is null) return true;

    return x.VariantType switch {
      Variant.Type.Float => _sod is not SodFloat,
      Variant.Type.Vector2 => _sod is not SodVector2,
      Variant.Type.Vector3 => _sod is not SodVector3,
      Variant.Type.Quaternion => _sod is not SodQuaternion,
      _ => true
    };
  }

  void _constructInnerSod(Variant x) {
    _sod = x.VariantType switch {
      Variant.Type.Float => new SodFloat(Params!, x.AsSingle()),
      Variant.Type.Vector2 => new SodVector2(Params!, x.AsVector2()),
      Variant.Type.Vector3 => new SodVector3(Params!, x.AsVector3()),
      Variant.Type.Quaternion => new SodQuaternion(Params!, x.AsQuaternion()),
      _ => null
    };
  }

  /// <inheritdoc />
  public override bool IsValid(Variant value) {
    return true;
  }

  /// <summary>
  /// Checks whether the given variant type is supported by this Sod
  /// </summary>
  /// <param name="type"></param>
  /// <returns></returns>
  public static bool IsValidType(Variant.Type type) {
    return type switch {
      Variant.Type.Float or
        Variant.Type.Vector2 or
        Variant.Type.Vector3 or
        Variant.Type.Quaternion => true,
      _ => false
    };
  }
}