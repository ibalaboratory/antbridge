using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StartController : MonoBehaviour {
  public Button button;
  private TimeController timeController;

  // Start is called before the first frame update
  void Start() {
    GameObject timeControllerObject = GameObject.Find("Slider");
    timeController = timeControllerObject.GetComponent<TimeController>();

    button = GetComponent<Button>();
    button.onClick.AddListener(OnButtonClick);
  }

  void OnButtonClick() {
    timeController.ResetTimeCount();
    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex,
                           LoadSceneMode.Single);
  }
}
