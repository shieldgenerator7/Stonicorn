using UnityEngine;

public class MilestoneActivatorAbility : MilestoneActivator
{

    public string abilityTypeName;
    public bool canGrantAbility = true;
    public bool canGrantUpgrade = true;


    [AutoInitialize(AllowUnfound =true), SerializeField, HideInInspector]
    private Fader fader;

    [AutoInitialize(AllowUnfound =true,SearchChildren =true), SerializeField, HideInInspector]
    private SimpleScaling scaling;

    public override void activateEffect()
    {
        PlayerAbility pa = ((PlayerAbility)Managers.Player.GetComponent(abilityTypeName));
        if (pa)
        {
            if (canGrantAbility && !pa.Unlocked)
            {
                pa.Unlocked = true;
            }
            else if (canGrantUpgrade)
            {
                pa.UpgradeLevel++;
            }
        }
        else
        {
            Debug.LogError($"Can't find ability on player with name {abilityTypeName}!");
        }
        if (fader)
        {
            fader.enabled = true;
        }
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
