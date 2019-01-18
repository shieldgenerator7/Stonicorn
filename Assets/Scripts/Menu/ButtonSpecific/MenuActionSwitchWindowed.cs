using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuActionSwitchWindowed : MenuActionSwitch
{
    public float timeToWait = 1;//how many seconds to wait for it to switch fullscreen mode
    private float lastChangeTime = 0;

    public override void doAction(bool active)
    {
        Screen.fullScreen = !active;
        lastChangeTime = Time.time;
    }
    public override bool getActiveState()
    {
        bool answer = !Screen.fullScreen;
        if (lastChangeTime != 0 && Time.time < lastChangeTime + timeToWait)
        {
            return !answer;
        }
        return answer;
    }
}
