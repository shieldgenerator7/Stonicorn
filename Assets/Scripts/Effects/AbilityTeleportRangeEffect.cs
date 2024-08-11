using System;
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

    private void abilityActivated(PlayerAbility ability, bool active)
    {
        //Update effects
        updateEffect();
    }

    public override void updateEffect()
    {
        //Set the color to white
        foreach (TeleportRangeFragment fragment in updater.fragments)
        {
            try
            {
            fragment.SpriteRenderer.color = Color.white;
            }
            catch(NullReferenceException nre)
            {
                Debug.LogException(nre);
            }
        }
        //Segment consulting
        foreach (PlayerAbility ability in Managers.Player.GetComponents<PlayerAbility>())
        {
            if (ability.enabled)
            {
                ability.teleportRangeSegment?.processFragments(updater.fragments, transform.up);
            }
        }
    }
}
