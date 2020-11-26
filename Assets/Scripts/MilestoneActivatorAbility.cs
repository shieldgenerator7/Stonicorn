using UnityEngine;

public class MilestoneActivatorAbility : MilestoneActivator
{

    public string abilityTypeName;

    public override void activateEffect()
    {
        PlayerAbility pa = ((PlayerAbility)Managers.Player.GetComponent(abilityTypeName));
        if (!pa.Unlocked)
        {
            pa.Unlocked = true;
        }
        else
        {
            pa.UpgradeLevel++;
        }
        Fader fader = GetComponent<Fader>();
        if (fader)
        {
            fader.enabled = true;
        }
    }
}
