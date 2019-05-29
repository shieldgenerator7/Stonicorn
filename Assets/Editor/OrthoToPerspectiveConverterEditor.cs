using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(OrthoToPerspectiveConverter))]
public class OrthoToPerspectiveConverterEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        OrthoToPerspectiveConverter otpc = ((OrthoToPerspectiveConverter)target);
        if (GUILayout.Button("Select All"))
        {
            otpc.selectAll();
        }
        if (GUILayout.Button("Convert"))
        {
            otpc.convert();
        }
        if (GUILayout.Button("Reset Settings to Default"))
        {
            otpc.settingsToDefault();
        }
    }
}
