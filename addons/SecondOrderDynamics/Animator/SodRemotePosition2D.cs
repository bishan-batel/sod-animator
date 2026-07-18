#nullable enable

using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Collections;
using SecondOrderDynamics.Math;

namespace SecondOrderDynamics.Animator;

/// <summary>
/// An animator node that applies a SOD Simulation for GlobalPosition, allowing you to make one node procedurally animated to another node 
/// </summary>
[GlobalClass, Tool, Icon("res://addons/SecondOrderDynamics/Icons/SodRemotePosition2D.svg")]
public partial class SodRemotePosition2D : Node2D {
  #region Exports

  /// <summary>
  /// The node that this will be interpolating to. Note that this is the opposite of how RemoteTransform2D works.
  /// </summary>
  [Export(PropertyHint.NodeType, nameof(Node2D))]
  public NodePath? Following { set; get; }

  /// <summary>
  /// Whether to simulate this system when running in-editor. Note if this can run in the editor this forces
  /// <see cref="UsePhysicsProcess"/> to be disabled.
  /// </summary>
  [Export]
  public bool RunInEditor {
    set {
      _runInEditor = value;
      if (_runInEditor) _usePhysicsProcess = false;

      #if TOOLS
      NotifyPropertyListChanged();
      #endif
    }
    get => _runInEditor;
  }

  /// <summary>
  /// Whether to only simulate this on physics process, if this is false then this will run every rendered frame
  /// </summary>
  [Export]
  public bool UsePhysicsProcess {
    set {
      _usePhysicsProcess = value;
      _updateProcessMode();
    }
    get => _usePhysicsProcess;
  }

  /// <summary>
  /// Whether to use Global units or Local units (eg. GlobalPosition vs Position)
  /// </summary>
  [ExportGroup("Transform")]
  [Export]
  public bool Local {
    set {
      if (_local != value) {
        _sodPosition = null;
        _sodRotation = null;
        _sodScale = null;
      }

      _local = value;
    }
    get => _local;
  }


  /// <summary>
  /// Whether to simulate / pull position from <see cref="Following"/>
  /// </summary>
  [ExportSubgroup("Position")]
  [Export(PropertyHint.GroupEnable)]
  public bool UpdatePosition {
    set {
      _shouldUpdatePosition = value;
      #if TOOLS
      UpdateConfigurationWarnings();
      #endif
    }
    get => _shouldUpdatePosition;
  }

  /// <summary>
  /// Sod parameters for interpolating position.
  /// </summary>
  [Export]
  public SodParams? PositionParams {
    set {
      _positionParams = value;
      #if TOOLS
      UpdateConfigurationWarnings();
      #endif
    }
    get => _positionParams;
  }

  /// <summary>
  /// Whether to simulate / pull rotation from <see cref="Following"/>
  /// </summary>
  [ExportSubgroup("Rotation")]
  [Export(PropertyHint.GroupEnable)]
  public bool UpdateRotation {
    set {
      _shouldUpdateRotation = value;
      #if TOOLS
      UpdateConfigurationWarnings();
      #endif
    }
    get => _shouldUpdateRotation;
  }

  /// <summary>
  /// Sod parameters for interpolating rotation.
  /// </summary>
  [Export]
  public SodParams? RotationParams {
    set {
      _rotationParams = value;
      #if TOOLS
      UpdateConfigurationWarnings();
      #endif
    }
    get => _rotationParams;
  }

  /// <summary>
  /// Whether to simulate / pull scale from <see cref="Following"/>
  /// </summary>
  [ExportSubgroup("Scale")]
  [Export(PropertyHint.GroupEnable)]
  public bool UpdateScale {
    set {
      _shouldUpdateScale = value;
      #if TOOLS
      UpdateConfigurationWarnings();
      #endif
    }
    get => _shouldUpdateScale;
  }

  /// <summary>
  /// Sod parameters for interpolating scale.
  /// </summary>
  [Export]
  public SodParams? ScaleParams {
    set {
      _scaleParams = value;
      #if TOOLS
      UpdateConfigurationWarnings();
      #endif
    }
    get => _scaleParams;
  }

  #endregion

  bool _usePhysicsProcess;
  bool _runInEditor;

  SodVector2? _sodPosition;
  bool _shouldUpdatePosition = true;
  SodParams? _positionParams;

  SodFloat? _sodRotation;
  bool _shouldUpdateRotation = true;
  SodParams? _rotationParams;

  SodVector2? _sodScale;
  bool _shouldUpdateScale;
  SodParams? _scaleParams;
  bool _local;

  void _updateProcessMode() {
    bool anyActive = UpdatePosition || UpdateRotation || UpdateScale;
    SetProcess(!_usePhysicsProcess && anyActive);
    SetPhysicsProcess(_usePhysicsProcess && anyActive);
  }

  /// <summary>
  /// Tries to find the node being followed, will fail if <see cref="Following"/> is an invalid path or said node is not a Node2D.
  /// </summary>
  /// <param name="following"></param>
  /// <returns>Success, do not read the out parameter if this is false.</returns>
  public bool TryGetFollowing(out Node2D following) {
    if (GetNodeOrNull<Node2D>(Following) is { } node) {
      following = node;
      return true;
    }

    following = null!;
    return false;
  }

  /// <inheritdoc />
  public override void _EnterTree() {
    _sodPosition = null;
    _sodRotation = null;
    _sodScale = null;
  }

  /// <inheritdoc />
  public override void _ExitTree() {
    _sodPosition = null;
    _sodRotation = null;
    _sodScale = null;
  }

  /// <inheritdoc />
  public override void _Process(double delta) {
    if (!UsePhysicsProcess) {
      _update((float)delta);
    }
  }

  /// <inheritdoc />
  public override void _PhysicsProcess(double delta) {
    if (UsePhysicsProcess) {
      _update((float)delta);
    }
  }

  void _update(float delta) {
    if (!TryGetFollowing(out Node2D following)) return;

    if (UpdatePosition && _updatePosition(delta, out Vector2 position, Local ? following.Position : following.GlobalPosition)) {
      if (Local) Position = position;
      else GlobalPosition = position;
    }

    if (UpdateRotation && _updateRotation(delta, out float rotation, Local ? following.Rotation : following.GlobalRotation)) {
      if (Local) Rotation = rotation;
      else GlobalRotation = rotation;
    }

    if (UpdateScale && _updateScale(delta, out Vector2 scale, Local ? following.Scale : following.GlobalScale)) {
      if (Local) Scale = scale;
      else GlobalScale = scale;
    }
  }

  bool _updatePosition(float delta, out Vector2 position, Vector2 target) {
    if (PositionParams is null) {
      position = default;
      return false;
    }

    _sodPosition ??= new SodVector2(PositionParams, target);

    position = _sodPosition.Update(delta, target);
    return true;
  }

  bool _updateRotation(float delta, out float rotation, float target) {
    if (RotationParams is null) {
      rotation = 0;
      return false;
    }

    _sodRotation ??= new SodFloat(RotationParams, target) { IsAngular = true };

    rotation = _sodRotation.Update(delta, target);
    return true;
  }

  bool _updateScale(float delta, out Vector2 scale, Vector2 target) {
    if (ScaleParams is null) {
      scale = Vector2.One;
      return false;
    }

    _sodScale ??= new SodVector2(ScaleParams, target);
    scale = _sodScale.Update(delta, target);
    return true;
  }


  /// <inheritdoc />
  public override void _ValidateProperty(Dictionary property) {
    StringName name = property["name"].AsStringName();

    if (name == PropertyName.UsePhysicsProcess) {
      if (RunInEditor) {
        property["usage"] = (int)PropertyUsageFlags.NoEditor;
      }

      return;
    }

    base._ValidateProperty(property);
  }

  /// <inheritdoc />
  public override string[] _GetConfigurationWarnings() {
    List<string> warnings = base._GetConfigurationWarnings()?.ToList() ?? [];

    if (!TryGetFollowing(out Node2D _)) {
      warnings.Add("For the simulation to work, the Following path must be set.");
    }

    if (UpdatePosition && PositionParams is null) {
      warnings.Add("UpdatePosition requires PositionParams to be set.");
    }

    if (UpdateRotation && RotationParams is null) {
      warnings.Add("UpdateRotation requires RotationParams to be set.");
    }

    if (UpdateScale && ScaleParams is null) {
      warnings.Add("UpdateScale requires ScaleParams to be set.");
    }

    return warnings.ToArray();
  }
}