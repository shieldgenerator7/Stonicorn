using UnityEngine;

[RequireComponent (typeof(PoweredActivator))]
public class PoweredMilestoneActivatorAbility : MilestoneActivatorAbility
{
    public override void activateEffect()
    {
        base.activateEffect();
        GetComponent<PoweredActivator>().allowTurnOff = false;
    }
}
