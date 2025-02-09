using UnityEngine;

[RequireComponent (typeof(PoweredActivator))]
public class PoweredMilestoneActivatorAbility : MilestoneActivatorAbility
{
    [AutoInitialize, SerializeField, HideInInspector]
    private PoweredActivator poweredActivator;

    public override void activateEffect()
    {
        base.activateEffect();
        poweredActivator.allowTurnOff = false;
    }
}
