#if TOOLS
#nullable enable

using Godot;

namespace SecondOrderDynamics;

/// <summary>
/// Editor Plugin Singleton for all the SOD Nodes
/// </summary>
[Tool]
public partial class SecondOrderDynamicsPlugin : EditorPlugin {
  /// <summary>
  /// Global Instance, will be null if the plugin is disabled
  /// </summary>
  public static SecondOrderDynamicsPlugin Singleton { private set; get; } = null!;

  Editor.SodInspectorPlugin _inspectorPlugin = null!;

  /// <inheritdoc />
  public override void _EnterTree() {
    Singleton = this;

    _inspectorPlugin = new Editor.SodInspectorPlugin();
    AddInspectorPlugin(_inspectorPlugin);
  }

  /// <inheritdoc />
  public override void _ExitTree() {
    RemoveInspectorPlugin(_inspectorPlugin);
  }
}
#endif