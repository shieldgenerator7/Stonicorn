using UnityEngine;

public class MilestoneActivatorAirport : MilestoneActivator
{//2016-03-29: copied from MilestoneActivatorForceTeleport

    public int incrementAmount = 1;

    public override void activateEffect()
    {
        GameManager.getPlayerObject().GetComponent<PlayerController>().maxAirPorts += incrementAmount;
    }
}
