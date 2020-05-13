using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SpriteShapeTool))]
public class SpriteShapeToolEditor : Editor
{
    SpriteShapeTool sst;

    void OnEnable()
    {
        sst = (SpriteShapeTool)target;
        Selection.selectionChanged += delegate() { sst?.setSSC(Selection.activeGameObject); };
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        GUI.enabled = !EditorApplication.isPlaying;
        if (GUILayout.Button("Level Terrain Points"))
        {
            sst.levelPoints();
        }
    }
    }
