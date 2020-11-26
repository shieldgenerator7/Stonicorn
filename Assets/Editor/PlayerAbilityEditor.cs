using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PlayerAbility), editorForChildClasses: true)]
public class PlayerAbilityEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GUILayout.Box("Fill out Upgrade Level 0 and Level 5,\nthen press the button below.");

        if (GUILayout.Button("Auto-Fillout Upgrade Levels"))
        {
            PlayerAbility pa = (PlayerAbility)target;
            if (pa.upgradeLevels.Count == 7)
            {
                //Auto-set feature levels
                if (!pa.upgradeLevels.Any(aul => aul.featureLevel > 0))
                {
                    AbilityUpgradeLevel aul3 = pa.upgradeLevels[3];
                    aul3.featureLevel = 1;
                    pa.upgradeLevels[3] = aul3;
                    AbilityUpgradeLevel aul6 = pa.upgradeLevels[6];
                    aul6.featureLevel = 2;
                    pa.upgradeLevels[6] = aul6;
                }

                //Fill out stats
                AbilityUpgradeLevel aul0 = pa.upgradeLevels[0];
                AbilityUpgradeLevel aul5 = pa.upgradeLevels[5];
                int ix = 0;
                int maxFeatureLevel = 0;
                for (int i = 0; i < pa.upgradeLevels.Count; i++)
                {
                    if (i % 3 != 0)
                    {
                        ix++;
                    }

                    AbilityUpgradeLevel aul = pa.upgradeLevels[i];

                    if (i != 0 && i != 5)
                    {
                        aul.stat1 = (aul5.stat1 - aul0.stat1)
                            * ix / 4 + aul0.stat1;
                        aul.stat2 = (aul5.stat2 - aul0.stat2)
                            * ix / 4 + aul0.stat2;
                        aul.stat3 = (aul5.stat3 - aul0.stat3)
                            * ix / 4 + aul0.stat3;
                        aul.stat4 = (aul5.stat4 - aul0.stat4)
                            * ix / 4 + aul0.stat4;
                    }

                    maxFeatureLevel = Mathf.Max(aul.featureLevel, maxFeatureLevel);
                    aul.featureLevel = maxFeatureLevel;

                    pa.upgradeLevels[i] = aul;
                }
            }
        }
    }
}
