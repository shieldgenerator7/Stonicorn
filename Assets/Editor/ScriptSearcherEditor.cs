using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ScriptSearcher))]
public class ScriptSearcherEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (GUILayout.Button("Search"))
        {
            ((ScriptSearcher)target).findAllObjectsWithScript();
        }
    }
}
