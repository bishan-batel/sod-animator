#nullable enable

using Godot;

namespace SecondOrderDynamics;

/// <summary>
/// Parameter constants to animate with second order dynamics.
/// </summary>
[GlobalClass, Tool, Icon("res://addons/SecondOrderDynamics/Icon/SodParams.svg")]
public partial class SodParams : Resource {
  /// <summary>
  /// 
  /// </summary>
  public float K1 { private set; get; }

  /// <summary>
  /// 
  /// </summary>
  public float K2 { private set; get; }

  /// <summary>
  /// 
  /// </summary>
  public float K3 { private set; get; }

  /// <summary>
  /// Returns a stabilized version of K2 that prevents second order systems from blowing up due to low delta values
  /// </summary>
  public float GetK2Stable(float delta) {
    return Mathf.Max(K2, Mathf.Max(delta * delta / 2f + delta * K1 / 2f, delta * K1));
  }

  /// <summary>
  /// The frequency of the system, this dictates how fast the system will react to changes in target position. If <see cref="Zeta"/> is set low,
  /// then this will determine the frequency at which this may 'oscillate' as it settles to the target position.
  ///
  /// If this is set to below or equal to zero, the system will behave chaotically and will gain more energy over time.
  /// </summary>
  [Export(PropertyHint.Range, "0,10,0.001,or_greater,suffix:Hz")]
  public float Frequency {
    set {
      _freq = value;

      _updateConstants();
    }
    get => _freq;
  }

  /// <summary>
  /// The Damping Coefficient, 0 means energy is never lost while higher values make the system lose energy faster.
  /// </summary>
  [Export(PropertyHint.Range, "0,5,0.001,or_greater")]
  public float Zeta {
    set {
      _zeta = value;
      _updateConstants();
    }
    get => _zeta;
  }

  /// <summary>
  /// Response Coefficient, this determines how much the system will 'anticipate' or 'overbound' when interpolating to the taret.
  /// </summary>
  [Export(PropertyHint.Range, "-5,5,0.001,or_greater,suffix:px/s")]
  public float Response {
    set {
      _response = value;
      _updateConstants();
    }
    get => _response;
  }

  float _zeta = 1.0f, _freq = 1.0f, _response = 0.1f;


  /// <summary>
  /// Construct from Frequency, Zeta, and Response Values
  /// </summary>
  public SodParams(float freq = 1, float zeta = 1, float response = 1) {
    SetDirectly(freq, zeta, response);
  }

  /// <summary>
  /// Default Constructor, sets Freq=1, Zeta=1, Response=1
  /// </summary>
  public SodParams() : this(1) {
  }

  /// <summary>
  /// Sets the frequency, zeta, and response directly all at once.
  /// </summary>
  public void SetDirectly(float freq, float zeta, float resp) {
    _freq = freq;
    _zeta = zeta;
    _response = resp;
    _updateConstants();
  }

  void _updateConstants() {
    K1 = Zeta / (Mathf.Pi * Frequency);
    K2 = 1f / (Mathf.Tau * Frequency * Mathf.Tau * Frequency);
    K3 = Response * Zeta / (Mathf.Tau * Frequency);
  }
}