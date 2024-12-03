using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ThetaController : MonoBehaviour {
  Slider slider;
  private TimeController timeController;

  // Start is called before the first frame update
  void Start() {
    slider = GetComponent<Slider>();
    slider.value = CreateBridge.theta;
    slider.onValueChanged.AddListener(OnSliderValueChanged);
  }

  void OnSliderValueChanged(float value) { CreateBridge.theta = value; }
}
