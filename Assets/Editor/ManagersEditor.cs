using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Managers))]
public class ManagersEditor : Editor
{
    static bool fold = false;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GUI.enabled = EditorApplication.isPlaying;

        fold = EditorGUILayout.Foldout(fold, "Progress Variables (Play Mode)")
            && GUI.enabled;
        if (fold)
        {
            Managers.Progress.getVariableValues()
                .ForEach(kvpStr => GUILayout.Label(kvpStr));
        }
    }
}
