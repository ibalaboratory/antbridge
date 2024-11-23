using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using System.IO;
using System;

//アリが利他行動を行い、橋を作成する
public class AntBridgeManager : MonoBehaviour
{
    private bool isChanged = false;

    // キー:antID, 値:antBridgePrefabの辞書
    private Dictionary<int, GameObject> antDict;
    //private Dictionary<int, GameObject> antDict_edge;//追加　変更済

    [SerializeField] private GameObject antBridgePrefab = null;
    private GameObject AntBridgePrefab => antBridgePrefab;

    [SerializeField] public float h = 4;
    [SerializeField] public float w = 8;

    private Mesh initMesh;

    // 記録変数
    // シミュレーションが終わるまでに橋になったことがあるアリの数
    public static int num_of_bridgeant = 0;
    //帰巣中にアリの橋を利用したアリの数
    public static int using_bridge_ant = 0;
    //探索 or フェロモン探索状態中にアリの橋を利用したアリの数
    public static int using_bridge_ant_search = 0;
    //橋になった時間の総量
    public static int bridgeant_cnt = 0;

    void Start()
    {
        antDict = new Dictionary<int, GameObject>();
        initMesh = Instantiate(gameObject.GetComponent<MeshFilter>().sharedMesh);
    }

    void FixedUpdate()
    {
        // 橋を作成するアリに変化があったとき(AddAnt, RemoveAntが呼び出されたとき)
        if(isChanged) {
            // meshの結合

            transform.GetComponent<MeshFilter>().mesh = Instantiate(initMesh);
            MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();
            CombineInstance[] combine = new CombineInstance[meshFilters.Length];

            int i = 0;
            while (i < meshFilters.Length)
            {
                combine[i].mesh = (meshFilters[i]).sharedMesh;
                combine[i].transform = meshFilters[i].transform.localToWorldMatrix;

                i++;
            }
            var combinedMesh = new Mesh();
            combinedMesh.CombineMeshes(combine);
            transform.GetComponent<MeshFilter>().mesh = combinedMesh;
            transform.gameObject.SetActive(true);
            transform.position = new Vector3(0, 0, 0);

            // NavMeshの作成

            GetComponent<NavMeshSurface>().BuildNavMesh();
        }
        isChanged = false;
    }

    // アリからboxを作成し、橋の一部にする
    // antID: 対象とするアリ/ pos: 橋の境界とアリの衝突位置/ angle:　作成するboxの角度/ direction: boxを作成する方向  
    public void AddAnt(int antID, Vector3 pos, float angle, Vector3 direction)//大幅に変更
    {
        if(antDict.ContainsKey(antID))
            return;
        Vector3 bridge_pos = pos + new Vector3(w * direction.x / 2.0f, -0.5f, h * direction.z / 2.0f);
        bridge_pos.y = -0.5f;
        GameObject bridge = Instantiate(antBridgePrefab, bridge_pos, Quaternion.identity);
        bridge.SetActive(true);
        float angle_euler = Util.ConvertToEuler(angle);
        bridge.transform.rotation = Quaternion.AngleAxis(-angle_euler - 90.0f, Vector3.up);
        bridge.transform.parent = gameObject.transform;
        bridge.transform.Find("Mesh").transform.localScale = new Vector3(h, 1.0f, w); 
        antDict[antID] = bridge;
        isChanged = true;
    }

    // 橋になっていたアリを元に戻す
    public void RemoveAnt(int antID)
    {
        if(!antDict.ContainsKey(antID))
            return;
        Destroy(antDict[antID]);
        antDict.Remove(antID);
        isChanged = true;
    }
}
