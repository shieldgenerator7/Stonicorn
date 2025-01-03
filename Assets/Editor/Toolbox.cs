using System.Reflection;
using UnityEditor;
using UnityEngine;

public class Toolbox : EditorWindow
{
    bool enabled = true;

    [Range(-1, 6)]
    public int ForceLaunchAbility = -1;
    [Range(-1, 6)]
    public int SwapAbility = -1;
    [Range(-1, 6)]
    public int WallClimbAbility = -1;
    [Range(-1, 6)]
    public int AirSliceAbility = -1;
    [Range(-1, 6)]
    public int ElectricBeamAbility = -1;
    [Range(-1, 6)]
    public int LongTeleportAbility = -1;

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
            ForceLaunchAbility = makeAbilityRow("ForceLaunchAbility", ForceLaunchAbility);
            SwapAbility = makeAbilityRow("SwapAbility", SwapAbility);
            ElectricBeamAbility = makeAbilityRow("ElectricBeamAbility", ElectricBeamAbility);
            WallClimbAbility = makeAbilityRow("WallClimbAbility", WallClimbAbility);
            AirSliceAbility = makeAbilityRow("AirSliceAbility", AirSliceAbility);
            LongTeleportAbility = makeAbilityRow("LongTeleportAbility", LongTeleportAbility);

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
