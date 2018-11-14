using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    public Text timerBox;
    private float timer;

    // Use this for initialization
    void Start()
    {
        timer = 0;
    }

    // Update is called once per frame
    void Update()
    {
        int seconds;
        int minutes;
        int hours;
        timer += Time.deltaTime;
        hours = (int)timer / 60;
        minutes = (int)(timer - hours) / 60;
        seconds = (int)(timer - hours) % 60;
        timerBox.text = hours.ToString("00") + ":" + minutes.ToString("00") + ":" + seconds.ToString("00");
    }

    public float GetTime()
    {
        return timer;
    }
}
