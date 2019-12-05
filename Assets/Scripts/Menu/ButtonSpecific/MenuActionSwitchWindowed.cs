using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuActionSwitchWindowed : MenuActionSwitch
{
    public override void doAction(bool active)
    {
        Managers.Video.Windowed = active;
    }
    public override bool getActiveState()
    {
        return Managers.Video.Windowed;
    }
}
