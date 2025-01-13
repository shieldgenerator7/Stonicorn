using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Toolbars;
using UnityEngine;
using static Unity.Burst.Intrinsics.Arm;

public class Toolbox : EditorWindow
{
    List<ToolboxTool> tools = new List<ToolboxTool>()
    {
        new AbilitySettingsTool(),
        new EditorCameraRotatorTool(),
    };

    bool enabled = true;
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

        EditorApplication.playModeStateChanged -= reactToPlayMode;
        EditorApplication.playModeStateChanged += reactToPlayMode;

        if (EditorApplication.isPlaying)
        {
            tools.ForEach(tool => tool.initPlayMode(pc));
        }
        else
        {
            tools.ForEach(tool => tool.init(pc));
        }
    }

    void reactToPlayMode(PlayModeStateChange pmsc)
    {
        if (pmsc == PlayModeStateChange.EnteredPlayMode)
        {
            tools.ForEach(tool => tool.initPlayMode(pc));
        }
        else if (pmsc == PlayModeStateChange.EnteredEditMode)
        {
            tools.ForEach(tool => tool.init(pc));
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

        tools.ForEach(tool => tool.display());
    }

    private void OnDisable()
    {
        EditorApplication.playModeStateChanged -= reactToPlayMode;
        tools.ForEach(tool => tool.dispose());
    }    
}
