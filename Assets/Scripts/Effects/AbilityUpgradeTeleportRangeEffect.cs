using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AbilityUpgradeTeleportRangeEffect : TeleportRangeEffect
{
    public override void init(TeleportRangeUpdater updater)
    {
        base.init(updater);
        Managers.Player.onAbilityUpgraded += abilityUpgraded;
    }

    private void abilityUpgraded(PlayerAbility ability, int upgradeLevel)
    {
        updateEffect();
    }

    public override void updateEffect()
    {
        //Get list of all TeleportRangeSegments
        List<List<float>> segmentAngles = Managers.Player.GetComponents<PlayerAbility>().ToList()
            .FindAll(ability => ability.teleportRangeSegment && ability.UpgradeLevel > 0)
            .ConvertAll(ability =>
                ability.teleportRangeSegment.getAngles(ability.UpgradeLevel)
                );
        //Get list of angles
        List<float> angles = new List<float>();
        segmentAngles.ForEach(sa =>
        {
            //Don't add "max angle"
            sa.RemoveAt(sa.Count - 1);
            if (sa.Count > 1)
            {
                //Prevent duplicates in list of angles
                sa.FindAll(angle => !angles.Any(a => Mathf.Abs(a - angle) < 0.5f))
                //And angle to list
                    .ForEach(angle => angles.Add(angle));
            }
        });
        //Decrement the value 360 (if any)
        angles = angles.ConvertAll(angle => ((angle == 360) ? 359 : angle));
        //Get list of fragment groups
        List<List<GameObject>> fragmentGroups = updater.getFragmentGroups(angles);
        //Alternate switching on of fragment groups
        for (int i = 0; i < fragmentGroups.Count; i++)
        {
            bool on = i % 2 == 1;
            fragmentGroups[i].ForEach(fragment => fragment.SetActive(on));
        }
    }
}
