using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuActionSwitchSound : MenuActionSwitch {

    public override void doAction(bool active)
    {
        FindObjectOfType<SoundManager>().Mute = !active;
    }

    public override bool getActiveState()
    {
        bool mute = FindObjectOfType<SoundManager>().Mute;
        return !mute;
    }
}
