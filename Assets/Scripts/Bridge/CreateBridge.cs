using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

// アリが探索を行う橋を作成する。
public class CreateBridge : MonoBehaviour
{
    // 橋の高さ
    [SerializeField]
    public float h = 10;
    // 橋全体の幅
    [SerializeField]
    public float wa = 5;
    // place1, place4の長さ
    [SerializeField]
    public float wb = 10;
    // place2, place3の長さ
    [SerializeField]
    public float lo = 10;
    //place2とplace3の間の角度(radianではなくdegree)。
    [SerializeField]
    public float theta = 30;
    // アリの巣。Prefab化したColonyオブジェクトをアタッチする
    [SerializeField] private GameObject colony;
    public GameObject Colony => colony;

    // マテリアル。橋の見た目を決定する
    [SerializeField]
    public Material targetMaterial;

    // コロニーの位置
    private Vector3 colonyPosition { get; set; }
    // 餌の位置
    public Vector3 feedPosition { get; set; }
    // カメラの位置
    public Vector3 cameraPosition;

    public int BridgeLayer { get; private set; }



    public Vector3 centerPosition {get; set;}

    void Awake()
    {
        // 4つのPlaceから成る橋をxz平面上に作成する
        // CreatePlace関数はオブジェクトを作成すると同時に、次のPlaceを作成するスタート位置を返す。

        BridgeLayer = LayerMask.NameToLayer("Bridge");
        colonyPosition = new Vector3(colony.transform.position.x, colony.transform.position.y, colony.transform.position.z);
        Vector3 pos = new Vector3(colonyPosition.x, 0.0f - h / 2.0f, colonyPosition.z);
        centerPosition = new Vector3(pos.x, pos.y, pos.z);
        float angle = 0.0f;
        pos = CreatePlace(pos, angle, wb, "Place1", true);
        angle = 90.0f - theta / 2.0f;
        cameraPosition.z = pos.z;
        pos = CreatePlace(pos, angle, lo, "Place2");
        cameraPosition.x = pos.x;
        cameraPosition.z = (cameraPosition.z + pos.z) / 2.0f;
        angle = -(90.0f - theta / 2.0f);
        pos = CreatePlace(pos, angle, lo, "Place3");
        angle = 0.0f;
        pos = CreatePlace(pos, angle, wb, "Place4");

        centerPosition = new Vector3((centerPosition.x + pos.x) / 2.0f, 0.0f, pos.z + (lo * (float)Math.Cos(Util.ConvertToRad(theta / 2)) - wa / (2 * (float)Math.Tan(Util.ConvertToRad(theta / 2)))));
        feedPosition = new Vector3(pos.x, pos.y, pos.z);

        // 各Placeのmeshを結合し、全体で一つのmeshにする。作成したmeshは親(このスクリプトをアタッチしている、empty Object"Bridge")のmeshに保存する。
        MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>().Where(i => i != gameObject.GetComponent<MeshFilter>()).ToArray();
        CombineInstance[] combine = new CombineInstance[meshFilters.Length];

        int i = 0;
        while (i < meshFilters.Length)
        {
            combine[i].mesh = (meshFilters[i]).sharedMesh;
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
            meshFilters[i].gameObject.SetActive(false);

            i++;
        }
        var combinedMesh = new Mesh();
        combinedMesh.CombineMeshes(combine);
        transform.GetComponent<MeshFilter>().mesh = combinedMesh;
        transform.gameObject.SetActive(true);
        transform.gameObject.GetComponent<Renderer>().material = targetMaterial;
        transform.position = new Vector3(0, 0, 0);

        // NavMeshの作成。アリが行動するために必要。
        GetComponent<NavMeshSurface>().BuildNavMesh();

        // カメラ位置の設定
        GameObject camera = GameObject.Find("Main Camera");
        camera.transform.position = new Vector3(cameraPosition.x, camera.transform.position.y, cameraPosition.z);
    }

    void Start()
    {
    }

    void FixedUpdate()
    {
    }

    // Placeを作成。各Placeはそれぞれ円柱2つと直方体1つを組み合わせて作成される。
    // 初めてCreatePraceを使用するときには、円柱・直方体・円柱の3つを作成
    // 2回目以降に作成する場合には、円柱が重なってしまうため、直方体・円柱の2つを作成
    // pos:一番端の円柱の位置/angle:x軸からの角度/w:Placeの直方体部分の長さ/name:Placeの名前/isCreateFirst:初めに作成されたPlaceの時true
    // オブジェクトを作成後、最後に作成したオブジェクトの位置を返す。次のPlaceを作成するときには、その位置からスタートする。
    private Vector3 CreatePlace(Vector3 pos, float angle, float w, string name, bool isCreateFirst = false)
    {
        float wd2 = w / 2;
        float angle_rad = Util.ConvertToRad(angle);
        Vector3 center = new Vector3(pos.x, pos.y, pos.z);
        Vector3 rot = new Vector3((float)Math.Cos(angle_rad), 0, (float)Math.Sin(angle_rad));
        GameObject place = new GameObject(name);
        place.layer = BridgeLayer;
        place.transform.parent = gameObject.transform;
        place.transform.position = center;
        if(isCreateFirst) {
            CreateCylinder(place, center, angle, wa / 2, h / 2, wa / 2);
        }
        center = new Vector3(center.x + wd2 * rot.x, center.y + wd2 * rot.y, center.z + wd2 * rot.z);
        CreateCube(place, center, angle, w, h, wa / 2);
        center = new Vector3(center.x + wd2 * rot.x, center.y + wd2 * rot.y, center.z + wd2 * rot.z);
        CreateCylinder(place, center, angle, wa / 2, h / 2, wa / 2);
        return new Vector3(center.x, center.y, center.z);
    }

    // Placeの円柱部分を作成。
    private void CreateCylinder(GameObject parent, Vector3 pos, float angle, float xscale, float yscale, float zscale)
    {
        GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        cylinder.layer = BridgeLayer;
        cylinder.transform.parent = parent.transform;
        cylinder.transform.rotation = Quaternion.AngleAxis(-angle, Vector3.up);
        cylinder.transform.position = new Vector3(pos.x, pos.y, pos.z);
        cylinder.transform.localScale = new Vector3(xscale, yscale, zscale);
    }

    // Placeの直方体部分を作成。
    private void CreateCube(GameObject parent, Vector3 pos, float angle, float xscale, float yscale, float zscale)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.layer = BridgeLayer;
        cube.transform.parent = parent.transform;
        cube.transform.rotation = Quaternion.AngleAxis(-angle, Vector3.up);
        cube.transform.position = new Vector3(pos.x, pos.y, pos.z);
        cube.transform.localScale = new Vector3(xscale, yscale, zscale);
    }
}
