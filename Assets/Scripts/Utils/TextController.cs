using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

//Textを表示する機能
public class TextController : MonoBehaviour
{
    private TextMeshProUGUI textTime;
    private TextMeshProUGUI textFood;
    private TextMeshProUGUI textIsEnd;

    private float time = 0.0f;
    private bool isEnd = false;
    public int foodNum {get; set;}
    [SerializeField] private int maxFoodNum = 1000;

    [SerializeField] private TimeController timeController;

    void Start()
    {
        textTime = transform.Find("Time").GetComponent<TextMeshProUGUI>();
        textFood = transform.Find("Food").GetComponent<TextMeshProUGUI>();
        textIsEnd = transform.Find("IsEnd").GetComponent<TextMeshProUGUI>();

        foodNum = 0;
    }

    void FixedUpdate()
    {
        if(!isEnd) {
            time = (float)timeController.timeCount;
            if (foodNum >= maxFoodNum) {
                isEnd = true;
            }
        }
        textTime.text = $"Time: {time}";
        textFood.text = $"EatenFood: {foodNum}";
        textIsEnd.text = $"Condition: {(isEnd ? "End" : "Processing")}";
    }
}
