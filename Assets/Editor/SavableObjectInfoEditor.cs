using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets;
using UnityEngine;
using System.Linq;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEditor.Graphs;
using static UnityEngine.EventSystems.EventTrigger;

[CustomEditor(typeof(SavableObjectInfo))]
public class SavableObjectInfoEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        SavableObjectInfo info = (SavableObjectInfo)target;
        GUI.enabled = isPrefab(info.gameObject);
        if (GUILayout.Button("Autoset (Prefab Only)"))
        {
            if (!inAssetDatabase(info.gameObject))
            {
                addToAssetDatabase(info.gameObject);
            }
            info.autoset();
            EditorUtility.SetDirty(info);
        }

        //GUI.enabled = !isPrefab(info.gameObject);
        //GUI.enabled = true;
        //if (!inAssetDatabase(info.gameObject))
        //{
        //    if (GUILayout.Button("Add to database (Prefab Only)"))
        //    {
        //        addToAssetDatabase(info.gameObject);
        //        EditorUtility.SetDirty(info);
        //    }
        //}
    }

    bool isPrefab(GameObject go)
    {
        //return PrefabUtility.IsPartOfPrefabAsset(go);
        return go.scene == null || go.scene.name == go.name
            || go.scene.name == null || go.scene.name == "";
    }

    public virtual bool inAssetDatabase(GameObject go)
    {
        //2025-01-02: copied from https://discussions.unity.com/t/is-it-possible-to-see-if-an-asset-is-checked-as-addressable-in-editor/808603/2
        AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
        AddressableAssetEntry entry = settings.FindAssetEntry(AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(go)));
        List<AddressableAssetEntry> entries = new List<AddressableAssetEntry>();
        settings.GetAllAssets(entries, false);
        return entries.Any(entry =>
        {
            string[] split = entry.address.Split('/', '.');
            return go.name == split[split.Length - 2];
        }
        );
    }
    public virtual void addToAssetDatabase(GameObject go)
    {
        //2025-01-02: copied from https://discussions.unity.com/t/set-addressable-via-c/741902/14
        string groupName = "Default Local Group";
        AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
        var group = settings.FindGroup(groupName);
        if (!group)
        {
            Debug.LogError("cant find group! " + groupName);
            return;
            //group = settings.CreateGroup(groupName, false, false, true, null, typeof(ContentUpdateGroupSchema), typeof(BundledAssetGroupSchema));
        }

        List<string> guids = AssetDatabase.FindAssets(go.name).ToList();
        string guid = guids.Find(guid =>
        {
            string[] split = AssetDatabase.GUIDToAssetPath(guid).Split('/', '.');
            return go.name == split[split.Length - 2];
        });

        var e = settings.CreateOrMoveEntry(guid, group, false, false);
        if (e == null)
        {
            Debug.LogError($"Unable to add '{go?.name}' to addressables! guid {guid}");
            return;
        }

        var entriesAdded = new List<AddressableAssetEntry> { e };
        Debug.LogWarning("Added prefab to addressables! " + e?.ToString());

        group.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, e, false, true);
        settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, e, true, false);

    }
}
