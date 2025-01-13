using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class AbilitySettingsTool : ToolboxTool
{

    List<string> abilityNames = new List<string> {
        "ForceLaunchAbility",
        "SwapAbility",
        "ElectricBeamAbility",
        "WallClimbAbility",
        "AirSliceAbility",
        "LongTeleportAbility",
    };

    private Dictionary<string, int> abilityLevelMap = new Dictionary<string, int>();
    private Dictionary<string, bool> abilityToggleMap = new Dictionary<string, bool>();

    public override string Name => "Ability Settings";
    protected override void init()
    {
        abilityNames.ForEach(abilityName => abilityLevelMap[abilityName] = getAbilityLevel(abilityName));
        abilityNames.ForEach(abilityName => abilityToggleMap[abilityName] = isAbilityOn(abilityName));
    }
    protected override void initPlayMode()
    {
        init();

        checkAllAbilities();
    }

    public override void dispose()
    {
    }

    public override void display()
    {
        makeAbilityRow(
            "ALL",
            (int)abilityLevelMap.Values.Average(v => v),
            abilityToggleMap.Values.Any(v => v),
            (newVal, newOn, changeLevel) =>
                abilityNames.ForEach(abilityName =>
                    updateFunc(abilityName, newVal, newOn, changeLevel)
                )
        );
        abilityNames.ForEach(abilityName =>
            makeAbilityRow(
                abilityName,
                abilityLevelMap[abilityName],
                abilityToggleMap[abilityName],
                (newVal, newOn, changeLevel) => updateFunc(abilityName, newVal, newOn, true)
                )
        );
    }


    void updateFunc(string abilityName, int val, bool on, bool changeLevel)
    {
        if (changeLevel)
        {
            abilityLevelMap[abilityName] = val;
        }
        else
        {
            val = abilityLevelMap[abilityName];
        }
        abilityToggleMap[abilityName] = on;
        if (EditorApplication.isPlaying)
        {
            checkAbility(on ? val : -1, abilityName);
        }
        EditorPrefs.SetInt($"{abilityName}_level", val);
        EditorPrefs.SetBool($"{abilityName}_on", on);
    }

    void makeAbilityRow(string abilityName, int oldVal, bool oldon, Action<int, bool, bool> updateFunc)
    {

        EditorGUILayout.BeginHorizontal();
        bool newon = EditorGUILayout.Toggle(oldon, GUILayout.Width(10));
        int newVal = (int)EditorGUILayout.Slider(abilityName, oldVal, -1, 6);
        EditorGUILayout.EndHorizontal();

        bool needsUpdate = false;
        if (oldon != newon)
        {
            needsUpdate = true;
        }
        if (oldVal != newVal)
        {
            newon = newVal >= 0;
            needsUpdate = true;
        }
        if (needsUpdate)
        {
            updateFunc(newVal, newon, oldVal != newVal);
        }
    }

    int getAbilityLevel(string abilityName)
    {
        if (EditorPrefs.HasKey($"{abilityName}_level"))
        {
            return EditorPrefs.GetInt($"{abilityName}_level");
        }
        PlayerAbility ability = (PlayerAbility)pc.GetComponent(abilityName);
        if (!ability.Unlocked || !ability.enabled)
        {
            return -1;
        }
        return ability.UpgradeLevel;
    }
    bool isAbilityOn(string abilityName)
    {
        if (EditorPrefs.HasKey($"{abilityName}_on"))
        {
            return EditorPrefs.GetBool($"{abilityName}_on");
        }
        PlayerAbility ability = (PlayerAbility)pc.GetComponent(abilityName);
        return ability.enabled;
    }

    void checkAllAbilities()
    {
        abilityNames.ForEach(abilityName =>
        {
            PlayerAbility ability = (PlayerAbility)pc.GetComponent(abilityName);
            ability.enabled = abilityToggleMap[abilityName];
            ability.setUpgradeLevel(abilityLevelMap[abilityName]);
        });
    }

    void checkAbility(int level, string abilityName)
    {
        checkAbility(level, (PlayerAbility)pc.GetComponent(abilityName));
    }

    void checkAbility(int level, PlayerAbility ability)
    {
        if (level >= 0)
        {
            ability.enabled = true;
            //ability.UpgradeLevel = level;
            ability.setUpgradeLevel(level);
        }
        else
        {
            ability.enabled = false;
            //ability.UpgradeLevel = 0;
            ability.setUpgradeLevel(0);
        }
    }
}
