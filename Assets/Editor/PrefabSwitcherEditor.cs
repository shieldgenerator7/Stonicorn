using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PrefabSwitcher))]
public class PrefabSwitcherEditor : Editor
{
    PrefabSwitcher ps;

    void OnEnable()
    {
        ps = (PrefabSwitcher)target;
        Selection.selectionChanged += delegate ()
        {
            ps?.setOldObjs(new List<GameObject>(
                Selection.GetFiltered<GameObject>(SelectionMode.Editable)
                ));
        };
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (GUILayout.Button("Switch Prefabs"))
        {
            List<GameObject> oldObjs = ps.oldObjects;
            List<GameObject> newObjs = oldObjs.ConvertAll(go => ps.switchPrefab(go));
            Undo.RecordObjects(newObjs.ToArray(), "Create new objects with prefab: " + ps.newPrefab.name);
            Undo.RecordObjects(oldObjs.ToArray(), "Delete old objects");
            newObjs.ForEach(go => EditorUtility.SetDirty(go));
            oldObjs.ForEach(go => DestroyImmediate(go));
            ps.oldObjects.Clear();
            ps.oldObjects.AddRange(newObjs);
        }
    }
}
