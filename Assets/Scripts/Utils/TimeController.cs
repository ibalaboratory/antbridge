using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO;
using System;

// Sliderを使用してシミュレーション全体の速度を変更できるようにする
public class TimeController : MonoBehaviour {
  Slider timeSlider;
  public static int timeCount = 0;
  private float updateSpan = 0.1f;
  private float currentTime;

  private AntBridgeManager antBridgeManager;

  // 現在のθの番号
  public static int now_theta = 0;
  // シミュレーションを終わる時間
  public static int end_time = 2000;
  // 結果を出力するための変数
  private StreamWriter sw;
  // 上記の情報を出力するファイルを作る場所
  private string directory = "../result.txt";

  // Use this for initialization
  void Start() {
    timeSlider = GetComponent<Slider>();
    timeSlider.maxValue = 4.0f;

    // AntBridgeManagerの取得
    GameObject antBridgeManagerObject = GameObject.Find("Bridge");
    antBridgeManager = antBridgeManagerObject.GetComponent<AntBridgeManager>();

    timeSlider.value = Time.timeScale;
    sw = new StreamWriter(directory, true);
  }

  void FixedUpdate() {
    currentTime += Time.deltaTime;
    if (currentTime > updateSpan) {
      timeCount += 1;
      currentTime = 0.0f;

      // シミュレーションが終わった時の挙動
      if (timeCount == end_time) {
        timeCount = 0;
        Debug.Log(
            $"θ = {CreateBridge.theta}\n" +
            $"橋になったアリの数 : {AntBridgeManager.beenBridgeCnt}\n" +
            $"時間を考慮した数 : {AntBridgeManager.elapsedTimeAsBridge}\n" +
            $"探索中に橋を渡ったアリの数 : {AntBridgeManager.usedBridgeSearchingCnt}\n" +
            $"帰巣中に橋を通った数 : {AntBridgeManager.usedBridgeGoingHomeCnt}\n" +
            $"餌を持ち帰ったアリの数 : {AntBridgeManager.broughtHomeFoodCnt}\n");
        sw.WriteLine(
            $"θ = {CreateBridge.theta}\n" +
            $"橋になったアリの数 : {AntBridgeManager.beenBridgeCnt}\n" +
            $"時間を考慮した数 : {AntBridgeManager.elapsedTimeAsBridge}\n" +
            $"探索中に橋を渡ったアリの数 : {AntBridgeManager.usedBridgeSearchingCnt}\n" +
            $"帰巣中に橋を通った数 : {AntBridgeManager.usedBridgeGoingHomeCnt}\n" +
            $"餌を持ち帰ったアリの数 : {AntBridgeManager.broughtHomeFoodCnt}\n");
        sw.Flush();
        sw.Close();
        UnityEditor.EditorApplication.isPlaying = false;
      }
    }
  }

  public void OnValueChange() { Time.timeScale = timeSlider.value; }

  public void ResetTimeCount() { timeCount = 0; }
}
