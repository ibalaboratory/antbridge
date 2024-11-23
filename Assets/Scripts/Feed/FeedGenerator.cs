using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 餌を生成する
public class FeedGenerator : MonoBehaviour
{
    [Header("Feed Settings"), SerializeField] private List<FeedItem> feeds = new List<FeedItem>();
    private List<FeedItem> Feeds => feeds;

    [SerializeField] public FeedController FeedPrefab;
    private int id = 0;

    [SerializeField] private CreateBridge bridge;
    [SerializeField] private TextController textController;

    public int FeedLayer { get; private set; }

    void Awake() 
    {
        FeedLayer = LayerMask.NameToLayer("Feed");
        Vector3 pos = bridge.feedPosition;
        Feeds.ForEach(item => {
            var feed = Instantiate(item.prefab, new Vector3(pos.x, pos.y + bridge.h / 2.0f, pos.z), Quaternion.identity);
            feed.textController = textController;
            feed.SetAmount(item.amount);
            feed.SetId(id);
            id++;
        });
    }

    void Start() 
    {

    }

    void Update(){
        if(Input.GetMouseButtonDown(1)){
            Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            pos.y = 2.36f;
            var feed = Instantiate(FeedPrefab, pos, Quaternion.identity);
            feed.SetAmount(10.0f);
            feed.SetId(id);
            id++;
        }
    }

    [Serializable]
    private class FeedItem
    {
        public FeedController prefab = null;
        public float amount = 10.0f; //餌の分量。数字が大きいほど餌が多い。
    }
}
