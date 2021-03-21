using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SpriteRendererColorer))]

public class SpriteRendererColorerEditor : Editor
{
    SpriteRendererColorer src;

    void OnEnable()
    {
        src = (SpriteRendererColorer)target;
        Selection.selectionChanged += delegate ()
        {
            src?.setSelectedObjs(new List<GameObject>(
                Selection.GetFiltered<GameObject>(SelectionMode.Editable)
                ));
        };
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (GUILayout.Button("Color Renderers"))
        {
            src.colorRenderers();
            src.selectedObjects.ForEach(go => EditorUtility.SetDirty(go));
        }
    }
}
