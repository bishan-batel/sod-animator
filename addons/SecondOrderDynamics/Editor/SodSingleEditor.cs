#if TOOLS
#nullable enable

using System;
using Godot;
using SecondOrderDynamics.Math;

namespace SecondOrderDynamics.Editor;

/// <summary>
/// Editor-Only class for displaying the SOD System preview in the inspector.
/// </summary>
/// <param name="godotObject"></param>
/// <param name="name"></param>
[Tool]
public partial class SodSingleEditor(GodotObject? godotObject, string name) : PanelContainer {
  const float TotalSimulationTime = 4;
  const float BaseLine = -1f;
  const float DeltaTime = 1f / 120;

  const float StepUpTime = 0.25f;

  // The main control for editing the property.
  Line2D _line = new();

  Line2D _targetLine = new();

  GodotObject? _godotObject = godotObject;
  string _name = name;

  ColorRect _graphPanel = new();


  /// <summary>
  /// Gets the default step response function
  /// </summary>
  /// <returns></returns>
  public static Curve GetStepResponseCurve() {
    var curve = new Curve();
    curve.AddPoint(new Vector2(0, BaseLine));
    curve.AddPoint(new Vector2(1, BaseLine + 1));
    return curve;
  }

  OptionButton _simulationTypeButton = new() {
    Name = "Test Function",
  };

  enum SimulationCurve {
    Step,
    Sin,
    Triangle,
    Square
  }


  /// <summary>
  /// Default Constructor
  /// </summary>
  public SodSingleEditor() : this(null!, "") {
  }

  /// <inheritdoc />
  public override void _Ready() {
    Control theme = EditorInterface.Singleton.GetBaseControl();

    AddChild(_graphPanel);
    _graphPanel.Color = theme.GetThemeColor("base_color", "Editor").Darkened(0.2f);
    _targetLine.DefaultColor = theme.GetThemeColor("font_color", "Editor").Darkened(0.1f);

    _targetLine.Width = 2;

    _graphPanel.AddChild(_targetLine);

    // _line.Modulate = Colors.AliceBlue;
    _line.Width = 3;
    _graphPanel.AddChild(_line);

    _simulationTypeButton.AddItem("Step Response", (int)SimulationCurve.Step);
    _simulationTypeButton.AddItem("Sin Response", (int)SimulationCurve.Sin);
    _simulationTypeButton.AddItem("Triangle Response", (int)SimulationCurve.Triangle);
    _simulationTypeButton.AddItem("Square Response", (int)SimulationCurve.Square);

    _simulationTypeButton.Selected = (int)SimulationCurve.Step;

    var simulationOptions = new VBoxContainer();

    var simulationTypeContainer = new HBoxContainer();
    simulationTypeContainer.AddChild(new Label { Text = "Simulation Preview Curve" });
    simulationTypeContainer.AddChild(_simulationTypeButton);

    simulationOptions.AddChild(simulationTypeContainer);

    var dropdown = new FoldableContainer { Title = "Editor Preview Settings", Folded = true };
    dropdown.AddChild(simulationOptions);
    dropdown.TitleAlignment = HorizontalAlignment.Right;
    // AddChild(dropdown);
  }

  /// <inheritdoc />
  public override void _Process(double delta) {
    if (_godotObject is null) {
      _line.ClearPoints();
      return;
    }

    SodFloat state = new((SodParams)_godotObject.Get(_name), BaseLine);

    _graphPanel.CustomMinimumSize = new Vector2(0, _graphPanel.GetParentAreaSize().X / (16f / 9f));


    Vector2 dimensions = _graphPanel.GetRect().Size;

    // float totalTime = dimensions.X / 100f;

    _line.ClearPoints();
    _line.Antialiased = true;
    _line.TextureMode = Line2D.LineTextureMode.Stretch;


    float height = dimensions.Y / 3;

    float lastY = state.Y;
    var grad = new Gradient();

    _targetLine.ClearPoints();

    for (float time = 0; time <= TotalSimulationTime; time += DeltaTime) {
      float x = _getX(time);
      float y = 1 - state.Update(DeltaTime, x);

      // grad.AddPoint();

      float percent = time / TotalSimulationTime;

      _targetLine.AddPoint(new Vector2(dimensions.X * percent, height * (1 - x)));
      _line.AddPoint(new Vector2(dimensions.X * percent, height * y));


      float slope = (state.Y - lastY) / DeltaTime;

      Control theme = EditorInterface.Singleton.GetBaseControl();

      Color accent = theme.GetThemeColor("accent_color", "Editor");
      grad.AddPoint(percent, accent.Lightened(0.5f).Lerp(accent.Lightened(0.3f), Mathf.Abs(slope)));

      // _line.AddPoint();
      lastY = state.Y;
    }


    grad.InterpolationColorSpace = Gradient.ColorSpace.Oklab;
    grad.InterpolationMode = Gradient.InterpolationModeEnum.Cubic;

    _line.Gradient = grad;
  }

  float _getX(float time) {
    var type = (SimulationCurve)_simulationTypeButton.Selected;

    return type switch {
      SimulationCurve.Step => time < StepUpTime ? BaseLine : BaseLine + 1,
      SimulationCurve.Sin => Mathf.Abs(BaseLine) * Mathf.Sin(time * 4) * 0.2f,
      SimulationCurve.Triangle => 2 * Mathf.Abs(time - Mathf.Floor(time + 0.5f)),
      SimulationCurve.Square => Mathf.Round(Mathf.Sin(time * Mathf.Tau)) * Mathf.Abs(BaseLine),
      _ => 0
    };
  }
}

#endif