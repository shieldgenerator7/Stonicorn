using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuActionSwitchMusic : MenuActionSwitch {

    public override void doAction(bool active)
    {
        Managers.Music.Mute = !active;
    }

    public override bool getActiveState()
    {
        bool mute = Managers.Music.Mute;
        return !mute;
    }
}
