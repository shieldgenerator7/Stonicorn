using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuActionSwitchSound : MenuActionSwitch {

    public override void doAction(bool active)
    {
        FindObjectOfType<SoundManager>().Mute = !active;
    }
}
