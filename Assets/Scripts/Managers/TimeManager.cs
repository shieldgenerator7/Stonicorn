using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeManager : SavableMonoBehaviour
{
    [SerializeField]
    private float time = -1;//the time since the game started, accounting for rewind
    /// <summary>
    /// The amount of time since the game started, accounting for rewind and game pauses
    /// </summary>
    public float Time
    {
        get
        {
            float diff = UnityEngine.Time.time - lastCheckedTime;
            time += diff;
            lastCheckedTime = UnityEngine.Time.time;
            return time;
        }
        set
        {
            time = value;
            lastCheckedTime = UnityEngine.Time.time;
            endGameTimer.Active = true;
            endGameTimer.overrideStartTime(0);
        }
    }
    private float lastCheckedTime;//the Time.time point that it last checked for program time

    public Timer endGameTimer;

    public bool Paused
    {
        get => UnityEngine.Time.timeScale == 0;
        set
        {
            UnityEngine.Time.timeScale = (value) ? 0 : timeSpeed;
            onPauseChanged?.Invoke(value);
        }
    }
    public delegate void OnPauseChanged(bool paused);
    public OnPauseChanged onPauseChanged;

    [SerializeField]
    private float slowTimeSpeed = 0.2f;
    private float timeSpeed = 1;
    public bool SlowTime
    {
        get => timeSpeed < 1;
        set
        {
            if (value)
            {
                timeSpeed = slowTimeSpeed;
            }
            else
            {
                timeSpeed = 1;
            }
            //If not paused,
            if (!Paused)
            {
                //Update the simulation time
                UnityEngine.Time.timeScale = timeSpeed;
            }
        }
    }

    private void Start()
    {
        if (time <= 0)
        {
            Time = 0;
        }
        endGameTimer.onTimeFinished += cycleEnded;
    }

    void cycleEnded()
    {
        Managers.Game.RewindToStart();
    }

    /// <summary>
    /// Returns true if the repeated cycle duration has been hit (again)
    /// Used for syncing things up
    /// </summary>
    /// <param name="duration"></param>
    /// <returns></returns>
    public bool beat(float duration)
        //EX: deltaTime = 0.2; 5.01 % 5 = 0.01, but 4.99 % 5 == 4.99
        =>Time % duration < (Time - UnityEngine.Time.deltaTime) % duration;


    public override SavableObject getSavableObject()
    {
        return new SavableObject(this,
            "time", Time
            );
    }

    public override void acceptSavableObject(SavableObject savObj)
    {
        Time = (float)savObj.data["time"];
    }
}
