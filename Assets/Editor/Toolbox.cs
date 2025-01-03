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
    private Dictionary<string, bool> abilityToggleMap = new Dictionary<string, bool>();

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

        abilityNames.ForEach(abilityName => abilityLevelMap[abilityName] = getAbilityLevel(abilityName));
        abilityNames.ForEach(abilityName => abilityToggleMap[abilityName] = true || isAbilityOn(abilityName));

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
            makeAbilityRow(abilityName)
        );

    }

    private void OnDisable()
    {
        //SceneView.duringSceneGui -= RotateCamera;
    }

    void makeAbilityRow(string abilityName)
    {
        int oldVal = abilityLevelMap[abilityName];
        bool oldon = abilityToggleMap[abilityName];

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
            abilityLevelMap[abilityName] = newVal;
            abilityToggleMap[abilityName] = newon;
            if (enabled && EditorApplication.isPlaying)
            {
                checkAbility(newon ? newVal : -1, abilityName);
            }
        }

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
    bool isAbilityOn(string abilityName)
    {
        PlayerAbility ability = (PlayerAbility)pc.GetComponent(abilityName);
        return ability.enabled;
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
