using UnityEngine;

public class MilestoneActivatorForceTeleport : MilestoneActivator
{//2016-03-29: copied from MilestoneActivatorRange

    public override void activateEffect()
    {
        Managers.Player.GetComponent<ForceTeleportAbility>().enabled = true;
    }
}
