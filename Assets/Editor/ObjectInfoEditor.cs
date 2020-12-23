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
        Debug.Log("" + go.name + ".scene: " + go.scene.name);
        //2020-12-22: copied from http://answers.unity.com/comments/220033/view.html
        return go.scene == null || go.scene.name == go.name
            || go.scene.name == null || go.scene.name == "";
    }
}
