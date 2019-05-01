using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuActionSwitchSound : MenuActionSwitch {

    public override void doAction(bool active)
    {
        Managers.Sound.Mute = !active;
    }

    public override bool getActiveState()
    {
        bool mute = Managers.Sound.Mute;
        return !mute;
    }
}
