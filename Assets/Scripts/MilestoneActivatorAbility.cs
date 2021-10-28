using UnityEngine;

public class MilestoneActivatorAbility : MilestoneActivator
{

    public string abilityTypeName;
    public bool canGrantAbility = true;
    public bool canGrantUpgrade = true;

    public override void activateEffect()
    {
        StonicornAbility pa = ((StonicornAbility)Managers.Player.GetComponent(abilityTypeName));
        if (canGrantAbility && !pa.Unlocked)
        {
            pa.Unlocked = true;
        }
        else if (canGrantUpgrade)
        {
            pa.UpgradeLevel++;
        }
        Fader fader = GetComponent<Fader>();
        if (fader)
        {
            fader.enabled = true;
        }
        SimpleScaling scaling = GetComponent<SimpleScaling>();
        if (scaling)
        {
            scaling.enabled = true;
        }
    }

    protected override void previouslyDiscovered()
    {
        Destroy(gameObject);
    }
}
