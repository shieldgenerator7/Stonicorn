using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityTeleportRangeEffect : TeleportRangeEffect
{
    public override void init(TeleportRangeUpdater updater)
    {
        base.init(updater);
        Managers.Player.onAbilityActivated += abilityActivated;
    }

    private void abilityActivated(StonicornAbility ability, bool active)
    {
        //Update effects
        updateEffect();
    }

    public override void updateEffect()
    {
        //Set the color to white
        foreach (GameObject fragment in updater.fragments)
        {
            fragment.GetComponent<SpriteRenderer>().color = Color.white;
        }
        //Segment consulting
        foreach (StonicornAbility ability in Managers.Player.GetComponents<StonicornAbility>())
        {
            if (ability.enabled)
            {
                ability.teleportRangeSegment?.processFragments(updater.fragments, transform.up);
            }
        }
    }
}
