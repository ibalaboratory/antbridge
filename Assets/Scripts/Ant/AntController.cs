using UnityEngine;
using UnityEngine.AI;

// アリの状態を表す変数
// Random: ランダム行動/ PheromoneSearch: フェロモン探索行動/ BackHome: 帰巣行動/ Bridge: 利他行動
public enum AntState
{
    Random = 0,
    PheromoneSearch,
    BackHome,
    Bridge
}

// アリの行動を決めるスクリプト
public class AntController : MonoBehaviour
{
    [Header("Ant")]
    // ID
        public int ID;
    // アリのイラストの描画
    [SerializeField] private SpriteRenderer antRenderer = null;
    public SpriteRenderer AntRenderer { get { return antRenderer; } }
    // アリの状態
    public AntState State { get; set; }
    // 行動範囲を決定するagent
    public NavMeshAgent Agent { get; set; }
    // アリの行動をupdateするスパン
    private float updateSpan = 0.1f;
    // 移動スピード
    private float Speed { get; set; }
    // アリの状態のupdateに使う変数
    private float CurrentTime { get; set; }
    // 内部活性
    public float E { get; set; } = 1.0f;
    // 内部活性の減少割合
    [SerializeField] private float alpha = 0.99f;
    private float Alpha => alpha;
    // アリを削除する閾値
    [SerializeField]
    private float threshold = 0.05f;
    public float Threshold => threshold;
    // 進行方向決定時のランダムネス
    public float Sensitivity { get; set; }
    // ガウス分布
    private float Z { get; set; }

    // 餌・巣の発見とその方向
    private bool DetectFeed { get; set; }
    private Vector3 FeedDirection { get; set; }
    private bool DetectHome { get; set; }
    private Vector3 HomeDirection { get; set; }
    //周囲のアリの数
    private int antCount = 0;



    [Header("Colony")]
    // Colony
    [SerializeField] private GameObject colony = null;
    private GameObject Colony => colony;
    // コロニーの位置
    private Vector3 ColonyPosition { get; set; }

    [Header("Pheromone")]
    // ColonyPheromonのPrefab
    [SerializeField] private PheromoneController pheromonePrefab1 = null;
    private PheromoneController PheromonePrefab1 => pheromonePrefab1;
    // FeedPheromonのPrefab
    [SerializeField] private PheromoneController pheromonePrefab2 = null;
    private PheromoneController PheromonePrefab2 => pheromonePrefab2;
    // フェロモンを生成するステップと現在の値
    [SerializeField] private int pheromoneStep = 2;
    private int PheromoneStep => pheromoneStep;
    private int CurrentPheromoneStep { get; set; }
    // フェロモンの個数が閾値を超えるとtrueを返す変数
    private PheromoneLimitation pheromoneLimitation;
    // 周囲のフェロモンの状態を保持する変数
    [SerializeField] private PheromoneInfo colonyPheromoneInfo = null;
    private PheromoneInfo ColonyPheromoneInfo => colonyPheromoneInfo;
    [SerializeField] private PheromoneInfo feedPheromoneInfo = null;
    private PheromoneInfo FeedPheromoneInfo => feedPheromoneInfo;


    [Header("altruistic")]
    // 利他行動を行っているアリを管理するオブジェクト
        private AntBridgeManager antBridgeManager;
    //  仮説1で利他行動を行うときの、周囲のアリの数の閾値(この値以上のとき利他行動を行う)
    [SerializeField] private int antThreshold = 5;
    public int AntThreshold => antThreshold;
    // 仮説2で利他行動を行うときの、周囲のフェロモンの数の閾値(この値以上のとき利他行動を行う)
    [SerializeField] private int pheromoneThreshold = 17;
    public int PheromoneThreshold => pheromoneThreshold;
    // アリが利他行動をしているときtrue, していないときfalse
    private bool isAltruistic = false;
    // 仮説の番号(0: 利他行動をしない/1: 周囲のアリの数が閾値以上 /2: 周囲のフェロモン数が閾値以上)
    [SerializeField] private int hypothesis = 1;
    // 利他行動を行うときの橋のサイズと位置
    [SerializeField]
    private float bridgeRadius = 20.0f;
    public Vector3 bridgePosition { get; set; }
    // 利他行動を行うときのランダムネス
    [SerializeField]
    private float randomThreshold = 0.2f;
    // 利他行動をやめる閾値。timeCntがこの値になると利他行動をやめる
    [SerializeField]
    private int timeThreshold = 30;
    // アリが利他行動を継続している時間
    private int timeCnt = 0;

    // レイヤー
    public int AntLayer { get; private set; }
    public int ColonyLayer { get; private set; }
    public int FeedLayer { get; private set; }
    public int ColonyPheromoneLayer { get; private set; }
    public int FeedPheromoneLayer { get; private set; }

    //デバッグ用
    [Header("debug"), Multiline(7)] public string debugText = "No Data";

    // 記録用変数
    // アリが帰巣途中に橋を渡ったかを示す変数。
    // 0:渡ってない 1:渡っている途中 2:渡った　更新済
    public int check_if_use_antbridge = 0;

    //アリが探索状態で橋を渡ったかを示す変数。0:渡ってない 1:渡っている途中 3:渡った　更新済
    public int check_if_use_antbridge_search = 0;

    //アリが橋を作る場所
    private Vector3 bridge_spot;

    //アリが橋になったことがあるか
    private bool experience = false;

    void Start() {
        Agent = GetComponent<NavMeshAgent>();
        AntRenderer.color = Color.black;
        // 作成したアリをランダム方向に回転
        transform.Rotate(new Vector3(0, Random.value * 360, 0));

        ColonyPosition = transform.position;
        Speed = 35.0f;

        AntLayer = LayerMask.NameToLayer("Ant");
        ColonyLayer = LayerMask.NameToLayer("Colony");
        FeedLayer = LayerMask.NameToLayer("Feed");
        ColonyPheromoneLayer = LayerMask.NameToLayer("ColonyPheromone");
        FeedPheromoneLayer = LayerMask.NameToLayer("FeedPheromone");

        //AntBridgeManagerの取得
        GameObject antBridgeManagerObject = GameObject.Find("Bridge");
        antBridgeManager = antBridgeManagerObject.GetComponent<AntBridgeManager>();

        // PheromoneLimitationの取得
        GameObject pheromoneLimitationObject = GameObject.Find("PheromoneLimitation");
        pheromoneLimitation = pheromoneLimitationObject.GetComponent<PheromoneLimitation>();

        // 記録用変数をリセット
        check_if_use_antbridge = 0;
        check_if_use_antbridge_search = 0;
    }

    void FixedUpdate() {
        CurrentTime += Time.deltaTime;
        if(CurrentTime > updateSpan) {
            if(State == AntState.Bridge)
                timeCnt++;
            // フェロモンの方向ベクトルの正規化
            ColonyPheromoneInfo.NormalizeDirection();
            FeedPheromoneInfo.NormalizeDirection();

            //状態の遷移
            if(State == AntState.Random && FeedPheromoneInfo.Count >= FeedPheromoneInfo.Threshold){//ランダム探索状態のときに餌フェロモンの数が一定値を上回ったとき
                State = AntState.PheromoneSearch;//フェロモン探索状態に移行
            }
            else if(State == AntState.PheromoneSearch && FeedPheromoneInfo.Count < FeedPheromoneInfo.Threshold){//フェロモン探索状態のときに餌フェロモンの数が一定値をした下回ったとき
                State = AntState.Random;//ランダム探索状態に移行
            }

            // 進行方向の決定
            var pos = transform.position;
            if(State == AntState.BackHome) { DetectFeed = false; }
            if(State != AntState.BackHome) { DetectHome = false; }

            if(DetectFeed) { // 餌の方向を向く
                transform.LookAt(pos + FeedDirection);
                DetectFeed = false;
            }
            else if(DetectHome) { // 巣の方向を向く
                transform.LookAt(pos + HomeDirection);
            }
            else if(State == AntState.Random) { // ランダム探索状態
                transform.Rotate(new Vector3(0, (Random.value * 2 - 1) * 60, 0));
                //アリがantbridgeを渡ったかどうか
                if(check_if_use_antbridge_search == 0){
                    if(transform.position.z < CreateBridge.a_1*transform.position.x + CreateBridge.b_1 
                            && transform.position.z < CreateBridge.a_2*transform.position.x + CreateBridge.b_2){
                        check_if_use_antbridge_search = 1;
                    }
                }
                else if(check_if_use_antbridge_search == 1){
                    if(transform.position.z > CreateBridge.a_2*transform.position.x + CreateBridge.b_2){
                        check_if_use_antbridge_search = 2;
                        AntBridgeManager.using_bridge_ant_search++;
                    }
                    else if(transform.position.z > CreateBridge.a_1*transform.position.x + CreateBridge.b_1){
                        check_if_use_antbridge_search = 0;
                    }
                }
            }
            else if(State == AntState.PheromoneSearch) { // フェロモン探索状態
                transform.LookAt(pos + FeedPheromoneInfo.Direction);
                // 分散1，平均0の正規分布に従う乱数
                Z = Mathf.Sqrt(-2.0f * Mathf.Log(Random.value)) * Mathf.Cos(2.0f * Mathf.PI * Random.value); 
                transform.Rotate(new Vector3(0, 20f * Z * (1.0f - Sensitivity), 0));
                // アリがantbridgeを渡ったかどうか
                if(check_if_use_antbridge_search == 0){
                    if(transform.position.z < CreateBridge.a_1*transform.position.x + CreateBridge.b_1 
                       && transform.position.z < CreateBridge.a_2*transform.position.x + CreateBridge.b_2){
                        check_if_use_antbridge_search = 1;
                    }
                }
                else if(check_if_use_antbridge_search == 1){
                    if(transform.position.z > CreateBridge.a_2*transform.position.x + CreateBridge.b_2){
                        check_if_use_antbridge_search = 2;
                        ++AntBridgeManager.using_bridge_ant_search;
                    }
                    else if(transform.position.z > CreateBridge.a_1*transform.position.x + CreateBridge.b_1){
                        check_if_use_antbridge_search = 0;
                    }
                }
            }
            else if(State == AntState.BackHome) { // 帰巣状態
                if(ColonyPheromoneInfo.Count >= ColonyPheromoneInfo.Threshold){//フェロモンの数が一定値を上回ったら、帰巣フェロモンの方向を向く
                    transform.LookAt(pos + ColonyPheromoneInfo.Direction);
                    Z = Mathf.Sqrt(-2.0f * Mathf.Log(Random.value)) * Mathf.Cos(2.0f * Mathf.PI * Random.value);
                    transform.Rotate(new Vector3(0, 20f * Z * (1.0f - Sensitivity), 0));
                }else{
                    transform.Rotate(new Vector3(0, (Random.value * 2 - 1) * 60, 0));
                }
                // アリがantbridgeを渡ったかどうか 変更済
                if(check_if_use_antbridge == 0){
                    if(transform.position.z < CreateBridge.a_1*transform.position.x + CreateBridge.b_1 
                       && transform.position.z < CreateBridge.a_2*transform.position.x + CreateBridge.b_2){
                        check_if_use_antbridge = 1;
                    }
                }
                else if(check_if_use_antbridge == 1){
                    if(transform.position.z > CreateBridge.a_2*transform.position.x + CreateBridge.b_2){
                        check_if_use_antbridge = 0;
                    }
                    else if(transform.position.z > CreateBridge.a_1*transform.position.x + CreateBridge.b_1){
                        check_if_use_antbridge = 2;
                    }
                }
                else if(check_if_use_antbridge == 2){
                    if(transform.position.z > CreateBridge.a_2*transform.position.x + CreateBridge.b_2){
                        check_if_use_antbridge = 0;
                    }
                    else if(transform.position.z < CreateBridge.a_1*transform.position.x + CreateBridge.b_1 
                            && transform.position.z < CreateBridge.a_2*transform.position.x + CreateBridge.b_2){
                        check_if_use_antbridge = 1;
                    }
                }
            }

            if(State == AntState.BackHome || !CheckIfIsAltruistic()) {
                if(State == AntState.Bridge) {
                    State = AntState.Random;
                    transform.Find("Sprite").gameObject.SetActive(true);
                    transform.Find("CollisionDetector").gameObject.SetActive(true);
                    gameObject.GetComponent<SphereCollider>().enabled = true;
                }
                Agent.enabled = true;
                // 目的地の設定
                pos += transform.forward * Speed * 0.1f;
                Agent.SetDestination(pos);

                // フェロモンの放出
                CurrentPheromoneStep += 1;
                if(CurrentPheromoneStep > PheromoneStep && pheromoneLimitation.flg) {
                    CurrentPheromoneStep = 0;
                    if(State == AntState.Random || State == AntState.PheromoneSearch) {//ランダム探索状態またはフェロモン探索状態の時
                        GeneratePheromone(PheromonePrefab1, E * ColonyPheromoneInfo.R, ColonyPheromoneInfo.Alpha);//帰巣フェロモンを放出する
                    }
                    else if(State == AntState.BackHome) {//帰巣状態の時
                        GeneratePheromone(PheromonePrefab2, E * FeedPheromoneInfo.R, FeedPheromoneInfo.Alpha);//餌フェロモンを放出する
                    }
                }

                // 内部活性の減衰
                E *= Alpha;
            } else {
                State = AntState.Bridge;
                transform.Find("Sprite").gameObject.SetActive(false);
                transform.Find("CollisionDetector").gameObject.SetActive(false);
                gameObject.GetComponent<SphereCollider>().enabled = false;

                // 以下諸変数の更新
                AntBridgeManager.bridgeant_cnt++;
                if(!experience){
                    AntBridgeManager.num_of_bridgeant++;
                    experience = true;
                }

            }

            // 探索の終了判定(下回ったアリを活動停止させる)
            if(E < Threshold) {
                Debug.Log("death");
                ResetAnt();
            }

            CurrentTime = 0.0f;
        }

        debugText = $"timeThreshold = {timeThreshold}\n"
            +$"threshold = {threshold}\n"
            +$"antThreshold = {antThreshold}\n"
            +$"pheromoneThreshold = {pheromoneThreshold}";

        // Reset counting
        ColonyPheromoneInfo.Reset();
        FeedPheromoneInfo.Reset();
        antCount = 0;
    }

    public void ResetAnt() {
        Destroy(gameObject, 0.0f);
    }
    // setting a destination
    public void FindFeed(float amount) {
        State = AntState.BackHome;
        Agent.ResetPath();
        // 次に進行方向を決定するまでNavMeshAgentを停止させる
        Agent.enabled = false;
        transform.Rotate(new Vector3(0, 180, 0));
        AntRenderer.color = Color.red;
        // 内部活性の回復
        E = Mathf.Clamp(amount, 0, 1);
        Debug.Log("Find Feed");
    }

    // フェロモンを生成する関数
    private void GeneratePheromone(PheromoneController prefab, float pheromone, float alpha) {
        var controller = Instantiate(prefab, transform.position + new Vector3((Random.value - 0.5f) * 2.0f, 0.0f, (Random.value - 0.5f) * 2.0f), Quaternion.identity);
        controller.Pheromone = pheromone;
        controller.Alpha = alpha;
    }
    // 更新時、利他行動を継続もしくはするようになるときtrue。
    private bool CheckIfIsAltruistic() {
        bool hypoCheck = false;
        if(hypothesis == 1)
            hypoCheck = (antCount >= antThreshold);
        else if(hypothesis == 2)
            hypoCheck = (ColonyPheromoneInfo.Count + FeedPheromoneInfo.Count >= pheromoneThreshold);

        // 現在利他行動をしていないとき
        if(!isAltruistic) {
            // エッジを検出
            NavMeshHit hit;
            var purpose_pos = new Vector3(transform.position.x, 0.0f, transform.position.z);

            if(hypoCheck && NavMesh.FindClosestEdge(purpose_pos, out hit, NavMesh.AllAreas)) {
                isAltruistic = true;
                float eps = 1e-3f;
                if(System.Math.Abs(hit.position.x - transform.position.x) < eps || System.Math.Abs(hit.position.z - transform.position.z) < eps
                        || (hit.position.x - bridgePosition.x) * (hit.position.x - bridgePosition.x) + (hit.position.z - bridgePosition.z) * (hit.position.z - bridgePosition.z) > bridgeRadius * bridgeRadius
                        || Random.value >= randomThreshold || transform.position.z < -6) 
                {
                    isAltruistic = false;
                    return isAltruistic;
                }
                Vector3 direction = hit.position - transform.position;
                direction.Normalize();
                float angle = (float)System.Math.Atan((hit.position.z - transform.position.z) / (hit.position.x - transform.position.x));
                // 利他行動を管理する辞書を更新
                antBridgeManager.AddAnt(ID, hit.position, angle, direction);
                bridge_spot = hit.position + new Vector3(antBridgeManager.w * direction.x / 2.0f, 0f, antBridgeManager.h * direction.z / 2.0f);
            }
            return isAltruistic;  
        }
        // 現在利他行動をしているとき
        else {
            if(antCount > Threshold){
                timeCnt = 0;
            }
            isAltruistic = !CheckIfStopAltruistic();
            if(!isAltruistic) {
                antBridgeManager.RemoveAnt(ID);
            }
            return isAltruistic;
        }
    }

    // 利他行動しているとき、やめるかどうか。やめるときtrue, やめないときfalse
    private bool CheckIfStopAltruistic() 
    {
        if(timeCnt >= timeThreshold) {// 利他行動を行う時間が一定以上になると利他行動をやめる
            timeCnt = 0;
            return true;
        }
        return false;
    }

    void OnTriggerStay(Collider collider) {
        // アリがフェロモン、餌、巣の検出をしたときの動作
        if(collider.gameObject.layer == ColonyPheromoneLayer) {// 帰巣フェロモンを検出したとき
            var v = (collider.gameObject.transform.position - transform.position);
            // 内積からフェロモンの方向を計算する
            if(v.magnitude >= 0.01f && Vector3.Dot(v.normalized, transform.forward) > 0.5) {
                // フェロモンの強さ、距離に応じてベクトルを加算
                ColonyPheromoneInfo.Direction += collider.gameObject.GetComponent<PheromoneController>().Pheromone * v / Mathf.Pow(v.magnitude + 0.1f, 1.2f);
                ColonyPheromoneInfo.Count += 1;
            }
        }
        else if(collider.gameObject.layer == FeedPheromoneLayer) {// 餌フェロモンを検出したとき
            var v = (collider.gameObject.transform.position - transform.position);
            if(v.magnitude >= 0.01f && Vector3.Dot(v.normalized, transform.forward) > 0.5) {
                FeedPheromoneInfo.Direction += collider.gameObject.GetComponent<PheromoneController>().Pheromone * v / Mathf.Pow(v.magnitude + 0.1f, 1.2f);
                FeedPheromoneInfo.Count += 1;
            }
        }
        // 餌と近いかどうか
        else if(collider.gameObject.layer == FeedLayer && State != AntState.BackHome) {
            DetectFeed = true;
            FeedDirection = (collider.gameObject.transform.position - transform.position).normalized;
        }
        // 巣と近いかどうか
        else if(collider.gameObject.layer == ColonyLayer && State == AntState.BackHome) {
            DetectHome = true;
            HomeDirection = (collider.gameObject.transform.position - transform.position).normalized;
        }
        // 他の蟻が近いかどうか
        else if(collider.gameObject.layer == AntLayer) {
            antCount += 1;
        }
    }
}
