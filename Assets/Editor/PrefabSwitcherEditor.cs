using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PrefabSwitcher))]
public class PrefabSwitcherEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (GUILayout.Button("Switch Prefabs"))
        {
            PrefabSwitcher ps = (PrefabSwitcher)target;
            List<GameObject> oldObjs = ps.oldObjects;
            List<GameObject> newObjs = oldObjs.ConvertAll(go => ps.switchPrefab(go));
            Undo.RecordObjects(newObjs.ToArray(), "Create new objects with prefab: " + ps.newPrefab.name);
            Undo.RecordObjects(oldObjs.ToArray(), "Delete old objects");
            oldObjs.ForEach(go => DestroyImmediate(go));
        }
    }
}
