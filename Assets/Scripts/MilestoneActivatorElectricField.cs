using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MilestoneActivatorElectricField : MilestoneActivator
{//2017-11-17: copied from MilestoneActivatorShieldBubble

    public override void activateEffect()
    {
        GameManager.Player.GetComponent<ElectricFieldAbility>().enabled = true;
    }
}
