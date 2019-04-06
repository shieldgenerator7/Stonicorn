using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MilestoneActivatorWallJump : MilestoneActivator
{//2017-02-25: copied from MilestoneActivatorAirport

    public override void activateEffect()
    {
        Managers.Player.GetComponent<WallClimbAbility>().enabled = true;
    }
}
