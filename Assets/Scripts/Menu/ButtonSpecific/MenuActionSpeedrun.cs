using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuActionSpeedrun : MenuActionSwitch
{
    public override void doAction(bool active)
    {
        FindObjectOfType<SpeedRunTimer>().activate();
    }

    public override bool getActiveState()
    {
        return FindObjectOfType<SpeedRunTimer>().Active;
    }
}
