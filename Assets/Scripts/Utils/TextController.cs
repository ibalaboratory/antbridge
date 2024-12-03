using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

// Textを表示する機能
public class TextController : MonoBehaviour {
  private TextMeshProUGUI textTime;
  private TextMeshProUGUI textFood;
  private TextMeshProUGUI textIsEnd;
  private TextMeshProUGUI textTheta;
  private TextMeshProUGUI textData;

  private float time = 0.0f;
  private bool isEnd = false;
  public int foodNum { get; set; }
  [SerializeField]
  private int maxFoodNum = 1000;

  [SerializeField]
  private TimeController timeController;

  void Start() {
    textTime = transform.Find("Time").GetComponent<TextMeshProUGUI>();
    textFood = transform.Find("Food").GetComponent<TextMeshProUGUI>();
    textIsEnd = transform.Find("IsEnd").GetComponent<TextMeshProUGUI>();
    textTheta = transform.Find("Theta").GetComponent<TextMeshProUGUI>();
    textData = transform.Find("Data").GetComponent<TextMeshProUGUI>();

    foodNum = 0;
  }

  void FixedUpdate() {
    if (!isEnd) {
      time = (float)TimeController.timeCount;
      if (foodNum >= maxFoodNum) {
        isEnd = true;
      }
    }
    textTime.text = $"Time: {time} / {TimeController.end_time}";
    textFood.text = $"Ate food: {foodNum}";
    textIsEnd.text = $"Status: {(isEnd ? "End" : "Simulating")}";
    textTheta.text = $"Theta: {CreateBridge.theta}";
    textData.text =
        $"Ants that became the bridge : {AntBridgeManager.beenBridgeCnt}\n" +
        $"Number of ants * their time as bridge : {AntBridgeManager.elapsedTimeAsBridge}\n" +
        $"Ants that crossed the bridge while searching : {AntBridgeManager.usedBridgeSearchingCnt}\n" +
        $"Ants that crossed the bridge while going home: {AntBridgeManager.usedBridgeGoingHomeCnt}\n" +
        $"Ants that brought home food : {AntBridgeManager.broughtHomeFoodCnt}\n";
  }
}
