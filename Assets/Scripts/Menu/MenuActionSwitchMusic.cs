using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuActionSwitchMusic : MenuActionSwitch {

    public override void doAction(bool active)
    {
        FindObjectOfType<MusicManager>().muteMusic(!active);
    }
}
