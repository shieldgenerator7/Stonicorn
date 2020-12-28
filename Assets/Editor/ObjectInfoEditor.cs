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
        if (GUILayout.Button("Convert to SavableObjectInfo (Prefab Only)"))
        {
            GameObject go = info.gameObject;
            DestroyImmediate(info, true);
            SavableObjectInfo soi = go.AddComponent<SavableObjectInfo>();
            soi.autoset();
            EditorUtility.SetDirty(soi);
        }
    }

    bool isPrefab(GameObject go)
    {
        //return PrefabUtility.IsPartOfPrefabAsset(go);
        return go.scene == null || go.scene.name == go.name
            || go.scene.name == null || go.scene.name == "";
    }
}
