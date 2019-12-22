using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeManager : SavableMonoBehaviour
{
    [SerializeField]
    private float time = 0;//the time since the game started, accounting for rewind
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
        }
    }
    private float lastCheckedTime;//the Time.time point that it last checked for program time

    private void Start()
    {
        Time = 0;
    }

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
