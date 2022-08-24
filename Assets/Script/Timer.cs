using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class Timer : MonoBehaviour
{
    //Timer
    public float StartTime;
    public int seconds;

    //Stop
    private bool gameOver;
    void Start()
    {
        StartTime = Time.time;
    }
    void Update()
    {
        gameOver = GameObject.Find("GameController").GetComponent<GameController>().gameOver;

        //Stop when game is over
        if (!gameOver)
        {
            //Minutes and seconds
            float TimerControl = Time.time - StartTime;
            int mins = ((int)TimerControl / 60);
            int segs = ((int)TimerControl % 60);

            //0:60 -> 1:00
            if (segs == 60)
            {
                segs = 0;
                mins += 1;
            }
            //Global time
            seconds = mins * 60 + segs;

            //Format to text
            string TimerString = string.Format("{00}:{01}", mins.ToString("00"), segs.ToString("00"));
            GetComponent<TextMeshPro>().text = TimerString.ToString();
        }
    }
}