using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ScenesManager))]
public class ScenesManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (GUILayout.Button("Print Object Scene Ids"))
        {
            ((ScenesManager)target).printObjectSceneList();
        }
    }
}
