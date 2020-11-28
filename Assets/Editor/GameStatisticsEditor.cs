using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GameStatistics))]
public class GameStatisticsEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (GUILayout.Button("Print Stats"))
        {
            ((GameStatistics)target).printStats(false);
        }
        if (GUILayout.Button("Print Stats All"))
        {
            ((GameStatistics)target).printStats(true);
        }
    }
}
