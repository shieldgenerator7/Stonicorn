using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

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

    private PlayerController pc;


    [MenuItem("SG7/Toolbox")]
    public static void OpenWindow()
    {
        GetWindow<Toolbox>();
    }

    public void OnEnable()
    {
        pc = GameObject.FindFirstObjectByType<PlayerController>();
        Debug.Log("found player: " + pc.name);

        abilityNames.ForEach(abilityName=>abilityLevelMap[abilityName] = getAbilityLevel(abilityName));

        //SceneView.duringSceneGui -= RotateCamera;
        //SceneView.duringSceneGui += RotateCamera;
    }

    private void OnGUI()
    {
        enabled = EditorGUILayout.Toggle("Enable tool", enabled);
        if (enabled)
        {
            if (pc == null || ReferenceEquals(pc.gameObject, null))
            {
                pc = GameObject.FindFirstObjectByType<PlayerController>();
            }
        }
        GUI.enabled = enabled;
        abilityNames.ForEach(abilityName =>
            abilityLevelMap[abilityName] = makeAbilityRow(abilityName, abilityLevelMap[abilityName])
        );

    }

    private void OnDisable()
    {
        //SceneView.duringSceneGui -= RotateCamera;
    }

    int makeAbilityRow(string abilityName, int value)
    {
        int oldVal = value;
        int newVal = (int)EditorGUILayout.Slider(abilityName, value, -1, 6);
        if (oldVal != newVal && enabled && EditorApplication.isPlaying)
        {
            checkAbility(newVal, abilityName);
        }
        return newVal;
    }

    int getAbilityLevel(string abilityName)
    {
        PlayerAbility ability = (PlayerAbility)pc.GetComponent(abilityName);
        if (!ability.Unlocked || !ability.enabled)
        {
            return -1;
        }
        return ability.UpgradeLevel;
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
