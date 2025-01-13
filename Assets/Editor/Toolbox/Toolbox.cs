using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using static Unity.Burst.Intrinsics.Arm;

public class Toolbox : EditorWindow
{
    bool enabled = true;

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

    private PlayerController pc;


    [MenuItem("SG7/Toolbox")]
    public static void OpenWindow()
    {
        GetWindow<Toolbox>();
    }
    //Asset Ref editor asset is null! id: 223774150

    private void findPlayerController()
    {
        pc = GameObject.FindObjectsByType<PlayerController>(FindObjectsSortMode.None).First(go => go.CompareTag("Player"));
    }

    public void OnEnable()
    {
        findPlayerController();
        Debug.Log("found player: " + pc.name);

        abilityNames.ForEach(abilityName => abilityLevelMap[abilityName] = getAbilityLevel(abilityName));
        abilityNames.ForEach(abilityName => abilityToggleMap[abilityName] = isAbilityOn(abilityName));

        EditorApplication.playModeStateChanged -= reactToPlayMode;
        EditorApplication.playModeStateChanged += reactToPlayMode;

        if (EditorApplication.isPlaying)
        {
            checkAllAbilities();
        }
    }

    void reactToPlayMode(PlayModeStateChange pmsc)
    {
        if (pmsc == PlayModeStateChange.EnteredPlayMode)
        {
            checkAllAbilities();
        }
    }

    private void OnGUI()
    {
        bool prevEnabled = enabled;
        enabled = EditorGUILayout.Toggle("Enable tool", enabled);
        if (enabled)
        {
            if (!prevEnabled || pc == null || ReferenceEquals(pc.gameObject, null))
            {
                findPlayerController();
            }
        }
        GUI.enabled = enabled;
        makeAbilityRow(
            "ALL",
            (int)abilityLevelMap.Values.Average(v => v),
            abilityToggleMap.Values.Any(v => v),
            (newVal, newOn) =>
                abilityNames.ForEach(abilityName =>
                    updateFunc(abilityName, newVal, newOn)
                )
        );
        abilityNames.ForEach(abilityName =>
            makeAbilityRow(
                abilityName,
                abilityLevelMap[abilityName],
                abilityToggleMap[abilityName],
                (newVal, newOn) => updateFunc(abilityName, newVal, newOn)
                )
        );

    }

    void updateFunc(string abilityName, int val, bool on)
    {
        abilityLevelMap[abilityName] = val;
        abilityToggleMap[abilityName] = on;
        if (enabled && EditorApplication.isPlaying)
        {
            checkAbility(on ? val : -1, abilityName);
        }
        EditorPrefs.SetInt($"{abilityName}_level", val);
        EditorPrefs.SetBool($"{abilityName}_on", on);
    }

    private void OnDisable()
    {
        EditorApplication.playModeStateChanged -= reactToPlayMode;
    }

    void makeAbilityRow(string abilityName, int oldVal, bool oldon, Action<int, bool> updateFunc)
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
            updateFunc(newVal, newon);
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
