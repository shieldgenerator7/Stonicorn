using UnityEngine;
using System.Collections;

public class MilestoneActivatorRange : MilestoneActivator
{//2016-03-17: copied from MilestoneActivator

    public int incrementAmount = 1;

    public override void activateEffect()
    {
        Managers.Player.baseRange += incrementAmount;
    }
}
