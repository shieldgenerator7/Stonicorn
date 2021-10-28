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

    private void abilityUpgraded(StonicornAbility ability, int upgradeLevel)
    {
        updateEffect();
    }

    public override void updateEffect()
    {
        //Get list of all TeleportRangeSegments
        List<List<float>> segmentAngles = Managers.Player.GetComponents<StonicornAbility>().ToList()
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
                //And angle to list
                sa.ForEach(angle => angles.Add(angle));
            }
        });
        //Remove duplicates (merge touching zones)
        for (int i = angles.Count - 2; i >= 0; i--)
        {
            if (Mathf.Abs(angles[i] - angles[i + 1]) < 0.5f)
            {
                angles.RemoveAt(i + 1);
                angles.RemoveAt(i);
            }
        }
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
