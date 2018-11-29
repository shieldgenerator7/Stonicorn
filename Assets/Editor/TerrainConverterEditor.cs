using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TerrainConverter))]
public class TerrainConverterEditor : Editor {

    TerrainConverter tc;

    private void OnEnable()
    {
        tc = (TerrainConverter)target;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GUI.enabled = !EditorApplication.isPlaying;
        if (GUILayout.Button("Select All Terrains"))
        {
            tc.addAllTerrains();
        }
        if (GUILayout.Button("Convert the Terrains"))
        {
            tc.convertTerrains();
        }
    }
}
