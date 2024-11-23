using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// フェロモンの性質を決めるスクリプト。生成されてから一定時間経ったフェロモンは削除される。
// Prefab化して、アリから生成されるようにする。
public class PheromoneController : MonoBehaviour
{
    // フェロモンが減少する単位時間
    [Header("Settings"), SerializeField] private float updateSpan = 0.1f;
    private float UpdateSpan => updateSpan;

    // 元々のフェロモンの量
    [SerializeField] private float pheromone = 1.0f;
    public float Pheromone {
        get { return pheromone; }
        set {
            pheromone = value;
        }
    }

    // フェロモンの減少割合
    [SerializeField] private float alpha = 0.95f;
    public float Alpha { get { return alpha; } set { alpha = value; } }

    // フェロモンが削除される閾値
    [SerializeField] private float threshold = 0.03f;
    public float Threshold => threshold;

    private float CurrentTime { get; set; }

    // 生成されてから一定時間経ったフェロモンは削除
    void FixedUpdate() {
        CurrentTime += Time.deltaTime;
        if(CurrentTime > UpdateSpan) {
            Pheromone *= Alpha;
            if(Pheromone < Threshold) {//排出されてから時間の経過したフェロモンを削除する
                Destroy(gameObject, 0.0f);
            }
            CurrentTime = 0.0f;
        }
    }

    // 障害物と重なったフェロモンは削除
    void OnTriggerEnter(Collider collider) {
        if(collider.gameObject.layer == LayerMask.NameToLayer("Obstacle")) {//障害物とフェロモンが重なっているとき
            Debug.Log("collision!");
            Destroy(this.gameObject, 0.0f);
        }
    }
}
