using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Used to countdown from a specified number to 0
/// </summary>
public class Timer : MonoBehaviour
{
    public bool destroyOnFinish = false;
    [SerializeField]
    private float maxTime = 10;//in seconds, when it starts
    [SerializeField]
    private const float minTime = 0;//in seconds, when it stops
    [SerializeField]
    private bool useUnscaledTime = false;
    [SerializeField]
    private bool startFromZeroOnStart = false;

    private float startTime = -1;

    public virtual float Duration
    {
        get => maxTime - minTime;
        set
        {
            float duration = value;
            maxTime = minTime + duration;
        }
    }

    public virtual bool Active
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

    public virtual float TimeLeft
    {
        get => Mathf.Max(0, startTime + Duration - CurrentTime);
        set
        {
            float timeLeft = value;
            startTime = CurrentTime - Duration + timeLeft;
        }
    }

    public virtual float CurrentTime
    {
        get => (useUnscaledTime)
            ? Time.unscaledTime
            : Managers.Time.Time;
    }

    // Start is called before the first frame update
    protected virtual void Start()
    {
        setTimer(Duration);
        if (startFromZeroOnStart)
        {
            overrideStartTime(0);
        }
    }

    // Update is called once per frame
    protected virtual void Update()
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
    public event OnTimeLeftChanged onTimeLeftChanged;

    public delegate void OnTimeFinished();
    public event OnTimeFinished onTimeFinished;

    protected void callOnTimeLeftChanged(float timeLeft, float duration)
    {
        onTimeLeftChanged?.Invoke(timeLeft, duration);
    }
    protected void callOnTimeFinished()
    {
        onTimeFinished?.Invoke();
    }

    public virtual void setTimer(float seconds = 0)
    {
        seconds = (seconds > 0) ? seconds : maxTime;
        Duration = seconds;
        Active = true;
    }

    public virtual void overrideStartTime(float startTime)
    {
        this.startTime = startTime;
    }

    public static Timer startTimer(float seconds = 1, OnTimeFinished timeFinished = null)
    {
        GameObject go = Managers.Game.gameObject;
        Timer timer = go.AddComponent<Timer>();
        timer.destroyOnFinish = true;
        timer.useUnscaledTime = true;
        timer.onTimeFinished += timeFinished;
        timer.setTimer(seconds);
        return timer;
    }

    public static Timer startTimerRecyclable(float seconds = 1, OnTimeFinished timeFinished = null, GameObject go = null)
    {
        if (go == null)
        {
            go = Managers.Game.gameObject;
        }
        Timer timer = go.GetComponent<Timer>();
        if (timer == null)
        {
            timer = go.AddComponent<Timer>();
        }
        timer.destroyOnFinish = false;
        timer.useUnscaledTime = true;
        timer.onTimeFinished -= timeFinished;
        timer.onTimeFinished += timeFinished;
        timer.setTimer(seconds);
        return timer;
    }
}
