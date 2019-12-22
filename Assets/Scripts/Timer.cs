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
    public bool destroyOnFinish = false;
    public bool useUnscaledTime = false;

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
                startTime = CurrentTime;
                onTimeLeftChanged?.Invoke(TimeLeft, Duration);
            }
            else
            {
                startTime = -1;
                if (destroyOnFinish)
                {
                    Destroy(this);
                }
            }
        }
    }

    public float TimeLeft
    {
        get => Mathf.Max(0, startTime + Duration - CurrentTime);
        set
        {
            float timeLeft = value;
            startTime = CurrentTime - Duration + timeLeft;
        }
    }

    public float CurrentTime
    {
        get => (useUnscaledTime)
            ? Time.unscaledTime
            : Managers.Time.Time;
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
            onTimeLeftChanged?.Invoke(timeLeft, Duration);
            if (timeLeft <= 0)
            {
                onTimeFinished?.Invoke();
                Active = false;
            }
        }
    }

    public delegate void OnTimeLeftChanged(float timeLeft, float duration);
    public OnTimeLeftChanged onTimeLeftChanged;

    public delegate void OnTimeFinished();
    public OnTimeFinished onTimeFinished;

    public void setTimer(float seconds = 0)
    {
        seconds = (seconds > 0) ? seconds : maxTime;
        Duration = seconds;
        Active = true;
    }

    public void overrideStartTime(float startTime)
    {
        this.startTime = startTime;
    }

    public static Timer startTimer(float seconds = 1, OnTimeFinished timeFinished = null)
    {
        GameObject go = FindObjectOfType<GameManager>().gameObject;
        Timer timer = go.AddComponent<Timer>();
        timer.destroyOnFinish = true;
        timer.useUnscaledTime = true;
        timer.onTimeFinished += timeFinished;
        timer.setTimer(seconds);
        return timer;
    }
}
