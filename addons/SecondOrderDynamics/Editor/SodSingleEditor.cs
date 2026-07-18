#if TOOLS
#nullable enable

using Godot;
using SecondOrderDynamics.Math;

namespace SecondOrderDynamics.Editor;

/// <summary>
/// Editor-Only class for displaying the SOD System preview in the inspector.
/// </summary>
/// <param name="godotObject"></param>
/// <param name="name"></param>
[Tool]
public partial class SodSingleEditor(GodotObject? godotObject, string name) : ColorRect {
  const float BaseLine = -1f;
  const float DeltaTime = 1f / 120;

  const float StepUpTime = 0.25f;

  // The main control for editing the property.
  Line2D _line = new();

  Line2D _targetLine = new();

  GodotObject? _godotObject = godotObject;
  string _name = name;


  /// <summary>
  /// Default Constructor
  /// </summary>
  public SodSingleEditor() : this(null!, "") {
  }

  /// <inheritdoc />
  public override void _Ready() {
    Control theme = EditorInterface.Singleton.GetBaseControl();

    Color = theme.GetThemeColor("base_color", "Editor").Darkened(0.2f);
    _targetLine.DefaultColor = theme.GetThemeColor("font_color", "Editor").Darkened(0.1f);

    _targetLine.Width = 2;

    AddChild(_targetLine);

    // _line.Modulate = Colors.AliceBlue;
    _line.Width = 3;
    AddChild(_line);
  }

  /// <inheritdoc />
  public override void _Process(double delta) {
    if (_godotObject is null) {
      _line.ClearPoints();
      return;
    }

    SodFloat state = new((SodParams)_godotObject.Get(_name), BaseLine);

    CustomMinimumSize = new Vector2(0, GetParentAreaSize().X / (16f / 9f));


    Vector2 dimensions = GetRect().Size;

    // float totalTime = dimensions.X / 100f;
    float totalTime = 4;

    _line.ClearPoints();
    _line.Antialiased = true;
    _line.TextureMode = Line2D.LineTextureMode.Stretch;


    float height = dimensions.Y / 3;

    float lastY = state.Y;
    var grad = new Gradient();

    for (float time = 0; time <= totalTime; time += DeltaTime) {
      float y = 1 - state.Update(DeltaTime, time < StepUpTime ? BaseLine : BaseLine + 1);

      // grad.AddPoint();

      float percent = time / totalTime;

      _line.AddPoint(new Vector2(dimensions.X * percent, height * y));


      float slope = (state.Y - lastY) / DeltaTime;

      Control theme = EditorInterface.Singleton.GetBaseControl();

      Color accent = theme.GetThemeColor("accent_color", "Editor");
      grad.AddPoint(percent, accent.Lightened(0.5f).Lerp(accent.Lightened(0.3f), Mathf.Abs(slope)));

      // _line.AddPoint();
      lastY = state.Y;
    }


    _targetLine.ClearPoints();

    _targetLine.AddPoint(new Vector2(
      0, height * (1 - BaseLine)
    ));

    _targetLine.AddPoint(new Vector2(
      dimensions.X * (StepUpTime / totalTime), height * (1 - BaseLine)
    ));

    _targetLine.AddPoint(new Vector2(
      dimensions.X * (StepUpTime / totalTime), height * (-BaseLine)
    ));

    _targetLine.AddPoint(new Vector2(
      dimensions.X, height * -BaseLine
    ));


    grad.InterpolationColorSpace = Gradient.ColorSpace.Oklab;
    grad.InterpolationMode = Gradient.InterpolationModeEnum.Cubic;

    _line.Gradient = grad;
  }
}

#endif