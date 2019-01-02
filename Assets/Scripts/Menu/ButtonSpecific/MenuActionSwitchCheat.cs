using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuActionSwitchCheat : MenuActionSwitch
{
    public TesterShortcuts.Cheat cheatToActivate;

    public override void doAction(bool active)
    {
        TesterShortcuts.activateCheat(cheatToActivate, active);
    }

    public override bool getActiveState()
    {
        return TesterShortcuts.cheatActive(cheatToActivate);
    }
}
