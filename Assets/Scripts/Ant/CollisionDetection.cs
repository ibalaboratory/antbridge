using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//アリと餌、巣穴の衝突を検知
public class CollisionDetection : MonoBehaviour
{

    private AntController Ant { get; set; }

    // 巣に餌を持ち帰ったアリの数
    public static int num_of_backhome_ants = 0;

    private void Awake() {
        Ant = transform.parent.GetComponent<AntController>();
    }

    void OnTriggerEnter(Collider collider) {
        if(Ant.State != AntState.BackHome) {
            //餌にぶつかった時
            if(collider.gameObject.layer == Ant.FeedLayer) { 
                var feed = collider.gameObject.GetComponent<FeedController>();
                Ant.FindFeed(feed.Amount);
                feed.SetAmount(feed.Amount - 0.1f);
                feed.AddEatCount();
                feed.OutputToLog();
            }
        }
        else {
            //巣穴にぶつかった時
            if(collider.gameObject.layer == Ant.ColonyLayer) {
                Debug.Log("BackHome!");
                Ant.ResetAnt();

                // 餌を持ち帰ったアリの数の更新
                if(Ant.check_if_use_antbridge == 2){
                    AntBridgeManager.using_bridge_ant++;
                }
                num_of_backhome_ants++;
            }
        }
    }
}
