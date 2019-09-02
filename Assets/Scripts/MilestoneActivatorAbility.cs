using UnityEngine;

public class MilestoneActivatorAbility : MilestoneActivator {

    public string abilityTypeName;

    public override void activateEffect()
    {
        ((PlayerAbility)Managers.Player.GetComponent(abilityTypeName)).enabled = true;
    }
}
