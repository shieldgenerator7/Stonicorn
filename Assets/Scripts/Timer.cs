using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Used to countdown from a specified number to 0
/// </summary>
public class Timer : MonoBehaviour
{
    public float maxTime = 10;//in seconds, when it starts
    public const float minTime = 0;//in seconds, when it stops

    private float startTime = -1;

    public float Duration
    {
        get => maxTime - minTime;
        set
        {
            float duration = value;
            maxTime = minTime + duration;
        }
    }

    public bool Active
    {
        get => startTime >= 0;
        set
        {
            bool active = value;
            if (active)
            {
                startTime = Time.time;
                timeLeftChanged?.Invoke(TimeLeft, Duration);
            }
            else
            {
                startTime = -1;
            }
        }
    }

    public float TimeLeft
    {
        get => Mathf.Max(0, startTime + Duration - Time.time);
        set
        {
            float timeLeft = value;
            startTime = Time.time - Duration + timeLeft;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        setTimer(Duration);
    }

    // Update is called once per frame
    void Update()
    {
        if (Active)
        {
            float timeLeft = TimeLeft;
            timeLeftChanged?.Invoke(timeLeft, Duration);
            if (timeLeft <= 0)
            {
                timeFinished?.Invoke();
                Active = false;
            }
        }
    }

    public delegate void TimeLeftChanged(float timeLeft, float duration);
    public TimeLeftChanged timeLeftChanged;

    public delegate void TimeFinished();
    public TimeFinished timeFinished;

    public void setTimer(float seconds = 0)
    {
        seconds = (seconds > 0) ? seconds : maxTime;
        Duration = seconds;
        Active = true;
    }
}
