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
            ForceLaunchAbility = (int)EditorGUILayout.Slider("Force Launch", ForceLaunchAbility, -1, 6);
            SwapAbility = (int)EditorGUILayout.Slider("Swap", SwapAbility, -1, 6);
            ElectricBeamAbility = (int)EditorGUILayout.Slider("Electric Beam", ElectricBeamAbility, -1, 6);
            WallClimbAbility = (int)EditorGUILayout.Slider("Wall Climb", WallClimbAbility, -1, 6);
            AirSliceAbility = (int)EditorGUILayout.Slider("Air Slice", AirSliceAbility, -1, 6);
            LongTeleportAbility = (int)EditorGUILayout.Slider("Long Teleport", LongTeleportAbility, -1, 6);

            //Activate abilities
            checkAbility(ForceLaunchAbility, pc.GetComponent<ForceLaunchAbility>());
            checkAbility(SwapAbility, pc.GetComponent<SwapAbility>());
            checkAbility(WallClimbAbility, pc.GetComponent<WallClimbAbility>());
            checkAbility(AirSliceAbility, pc.GetComponent<AirSliceAbility>());
            checkAbility(ElectricBeamAbility, pc.GetComponent<ElectricBeamAbility>());
            checkAbility(LongTeleportAbility, pc.GetComponent<LongTeleportAbility>());
        }
    }

    private void OnDisable()
    {
        //SceneView.duringSceneGui -= RotateCamera;
    }

    

    void checkAbility(int level, PlayerAbility ability)
    {
        if (level >= 0)
        {
            ability.enabled = true;
            ability.UpgradeLevel = level;
        }
        else
        {
            ability.enabled = false;
            ability.UpgradeLevel = 0;
        }
    }
}
