using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// アリと餌、巣穴の衝突を検知
public class CollisionDetection : MonoBehaviour {
  private AntController Ant { get; set; }

  // 以下追加の変数
  private void Awake() { Ant = transform.parent.GetComponent<AntController>(); }

  void OnTriggerEnter(Collider collider) {
    if (Ant.State != AntState.BackHome) {
      if (collider.gameObject.layer == Ant.FeedLayer) { // 餌にぶつかった時
        var feed = collider.gameObject.GetComponent<FeedController>();
        Ant.FindFeed(feed.Amount);
        feed.SetAmount(feed.Amount - 0.1f);
        feed.AddEatCount(); // 餌の食べられた回数を増やす
        feed.OutputToLog();
      }
    } else {
      if (collider.gameObject.layer == Ant.ColonyLayer) { // 巣穴にぶつかった時
        Debug.Log("BackHome!");
        Ant.ResetAnt();

        // 餌を持ち帰ったアリの数の更新
        if (Ant.usedBridgeGoingHome == 2) {
          AntBridgeManager.usedBridgeGoingHomeCnt++;
        }
        AntBridgeManager.broughtHomeFoodCnt++;
      }
    }
  }
}
