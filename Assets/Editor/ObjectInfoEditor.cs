using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ObjectInfo))]
public class ObjectInfoEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        ObjectInfo info = (ObjectInfo)target;
        GUI.enabled = isPrefab(info.gameObject);
        if (GUILayout.Button("Autoset (Prefab Only)"))
        {
            info.autoset();
            EditorUtility.SetDirty(info);
        }
    }

    bool isPrefab(GameObject go)
    {
        //return PrefabUtility.IsPartOfPrefabAsset(go);
        return go.scene == null || go.scene.name == go.name
            || go.scene.name == null || go.scene.name == "";
    }
}
