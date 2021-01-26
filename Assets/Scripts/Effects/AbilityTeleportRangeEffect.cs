using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityTeleportRangeEffect : TeleportRangeEffect
{
    public override void updateEffect(List<GameObject> fragments, float timeLeft, float duration)
    {
        //Set the color to white
        foreach (GameObject fragment in fragments)
        {
            fragment.GetComponent<SpriteRenderer>().color = Color.white;
        }
        //Segment consulting
        foreach (PlayerAbility ability in Managers.Player.GetComponents<PlayerAbility>())
        {
            if (ability.enabled)
            {
                ability.teleportRangeSegment?.processFragments(fragments, transform.up);
            }
        }
    }
}
