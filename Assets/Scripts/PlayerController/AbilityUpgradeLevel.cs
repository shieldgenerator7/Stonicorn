using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct AbilityUpgradeLevel
{
    public float stat1;
    public float stat2;
    public float stat3;
    public float stat4;

    [SerializeField]
    private int featureLevel;

    public int getFeatureLevel(List<AbilityUpgradeLevel> levels)
    {
        //if this level upgrades something,
        if (featureLevel > 0)
        {
            //return it
            return featureLevel;
        }
        else
        {
            int maxLevel = 0;
            foreach (AbilityUpgradeLevel aul in levels)
            {
                //Get the max feature level of previous levels
                if (aul.featureLevel > maxLevel)
                {
                    maxLevel = aul.featureLevel;
                }
                //Don't go past this aul
                if (aul.Equals(this))
                {
                    break;
                }
            }
            return maxLevel;
        }
    }

}
