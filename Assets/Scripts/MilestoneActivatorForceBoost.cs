

public class MilestoneActivatorForceBoost : MilestoneActivator
{//2019-04-06: copied from MilestoneActivatorForceTeleport
    public override void activateEffect()
    {
        Managers.Player.GetComponent<ForceBoostAbility>().Unlocked = true;
    }
}
