using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[CustomEditor(typeof(TugBoat))]
public class TugBoatEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        GUI.enabled = !EditorApplication.isPlaying;
        if (GUILayout.Button("Tug towards Position"))
        {
            if (EditorApplication.isPlaying)
            {
                throw new UnityException("You must be in Edit Mode to use this function!");
            }
            TugBoat tb = (TugBoat)target;
            tb.move();
            EditorSceneManager.MarkSceneDirty(tb.gameObject.scene);
        }
    }
}
