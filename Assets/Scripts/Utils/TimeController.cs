using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//Sliderを使用してシミュレーション全体の速度を変更できるようにする
public class TimeController : MonoBehaviour
{
    Slider timeSlider;

    public int timeCount=0;
    private float updateSpan = 0.1f;
    private float currentTime;

    // Use this for initialization
    void Start()
    {
        timeSlider = GetComponent<Slider>();
        timeSlider.maxValue = 4.0f;
        timeSlider.value = 2.0f;
        Time.timeScale = timeSlider.value;
    }

    void FixedUpdate()
    {
        currentTime += Time.deltaTime;
        if(currentTime > updateSpan) {
            timeCount += 1;
            currentTime = 0.0f;
        }
    }

    public void OnValueChange()
    {
        Time.timeScale = timeSlider.value;
    }
}
