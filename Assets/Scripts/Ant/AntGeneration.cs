using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// アリを生成する関数。オブジェクトColonyにアタッチ。
public class AntGeneration : MonoBehaviour
{
    [SerializeField]
    public float antMakeSpan = 3.0f; //アリを生成する間隔
    public int maxSensitiveAntNum = 55; //敏感アリの最大数
    public int maxInsensitiveAntNum = 5; //鈍感アリの最大数
    public float sensitivityHigh = 0.90f;
    public float sensitivityLow = 0.4f;

    private float currentTime = 3.0f;
    private int countSensitive = 0;
    private int countInsensitive = 0;

    private string Ant1tag = "Ant1";
    private string Ant2tag = "Ant2";

    [Header("References"), SerializeField] private AntController Ant = null;

    [SerializeField] private CreateBridge bridgeCreater = null;

    [Header("debug"),Multiline(7)] public string debugText = "No Data";

    private int antID = 0;

    // 一定時間ごとに、鈍感アリと敏感アリの数を参照して、数が少ない方のアリを作成する
    void FixedUpdate() {
        currentTime += Time.deltaTime;
        if(currentTime >= antMakeSpan) {
            // 敏感アリの数
            countSensitive = GameObject.FindGameObjectsWithTag(Ant1tag).Length;
            // 鈍感アリの数
            countInsensitive = GameObject.FindGameObjectsWithTag(Ant2tag).Length;
            if(countSensitive < maxSensitiveAntNum) { //敏感アリの数が最大値より下回っていたら、敏感アリを生成する
                GenerateAnt(sensitivityHigh, Ant1tag);
            }
            if(countInsensitive < maxInsensitiveAntNum) { //鈍感アリの数が最大値より下回っていたら、鈍感アリを生成する
                GenerateAnt(sensitivityLow, Ant2tag);
            }
            currentTime = 0.0f;
        }
        debugText = $"countSensitive = {countSensitive}\n"
                    +$"countInsensitive = {countInsensitive}\n"
                    +$"antMakeSpan = {antMakeSpan}\n"
                    +$"maxSensitiveAntNum = {maxSensitiveAntNum}\n"
                    +$"maxInsensitiveAntNum = {maxInsensitiveAntNum}\n";
    }

    // 指定したsensitivityとタグをもつアリを作成する
    private void GenerateAnt(float sensitivity, string tag) {
        var ant = Instantiate(Ant, transform.position, Quaternion.identity);
        ant.gameObject.SetActive(true);
        ant.Sensitivity = sensitivity;
        ant.tag = tag;
        ant.ID = antID;
        antID += 1;
        // 初めは右向き
        ant.transform.LookAt(new Vector3(1.0f, 0.0f, 0.0f));

        // この座標は利他行動を行う際に使用される
        ant.bridgePosition = new Vector3(bridgeCreater.centerPosition.x, bridgeCreater.centerPosition.y, bridgeCreater.centerPosition.z);
    }
}
