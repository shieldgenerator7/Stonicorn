using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets;
using UnityEngine;
using System.Linq;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEditor.Graphs;

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
            info.autoset();
            EditorUtility.SetDirty(info);
        }

        //GUI.enabled = !isPrefab(info.gameObject);
        GUI.enabled = true;
        if (!inAssetDatabase(info.gameObject))
        {
            if (GUILayout.Button("Add to database (Prefab Only)"))
            {
                addToAssetDatabase(info.gameObject);
                EditorUtility.SetDirty(info);
            }
        }
    }

    bool isPrefab(GameObject go)
    {
        //return PrefabUtility.IsPartOfPrefabAsset(go);
        return go.scene == null || go.scene.name == go.name
            || go.scene.name == null || go.scene.name == "";
    }

    public virtual bool inAssetDatabase(GameObject go)
    {
        //2025-01-02: copied from https://stackoverflow.com/a/63814229/2336212
        GameObject prefab = PrefabUtility.GetCorrespondingObjectFromOriginalSource(go);
        //2025-01-02: copied from https://discussions.unity.com/t/is-it-possible-to-see-if-an-asset-is-checked-as-addressable-in-editor/808603/2
        AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
        AddressableAssetEntry entry = settings.FindAssetEntry(AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(go)));
        if (entry == null)
        {
            entry = settings.FindAssetEntry(AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(prefab)));
        }
        List<AddressableAssetEntry> entries = new List<AddressableAssetEntry>();
        settings.GetAllAssets(entries, false);
        //entries.ForEach(entry => Debug.Log("entry: " + entry.address + ", " + entry.AssetPath+", "+ entry.address.Split('/', '.')[1]));
        //List<string> names = new List<string>() { go.name,prefab.name};
        string name = (isPrefab(go)) ? go.name : prefab.name;
        return entries.Any(entry =>
        {
            string[] split = entry.address.Split('/', '.');
            //return names.Contains(split[split.Length - 2]);
            //Debug.Log("entry: " + entry.address + ", " + split[split.Length - 2] + ", " + entry.parentGroup.name);
            return name == split[split.Length - 2];
        }
        //entry.TargetAsset == go || entry.TargetAsset == prefab
        );
        //return AssetDatabase.Contains(go) || AssetDatabase.Contains(prefab);
        //Debug.Log("assets count in addressables: " + entries.Count);
        //return entry != null;
    }
    public virtual void addToAssetDatabase(GameObject go)
    {
        Debug.Log("is prefab? "+isPrefab(go));
            string groupName = "Default Local Group";
        GameObject prefab = PrefabUtility.GetCorrespondingObjectFromOriginalSource(go);
        if (!isPrefab(go))
        {
            go = prefab;
        }
        AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;

        //2025-01-02: copied from https://discussions.unity.com/t/set-addressable-via-c/741902/14
        var group = settings.FindGroup(groupName);
        if (!group)
        {
            Debug.LogError("cant find group! " + groupName);
            //group = settings.CreateGroup(groupName, false, false, true, null, typeof(ContentUpdateGroupSchema), typeof(BundledAssetGroupSchema));
        }

        var assetpath = AssetDatabase.GetAssetPath(go);
        var guid = AssetDatabase.AssetPathToGUID(assetpath);
        Debug.Log($"assetpath {assetpath}, guid {guid}");

        var e = settings.CreateOrMoveEntry(guid, group, false, false);
        var entriesAdded = new List<AddressableAssetEntry> { e };

        group.SetDirty(AddressableAssetSettings.ModificationEvent.EntryCreated, entriesAdded, false, true);
        settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryCreated, entriesAdded, true, false);

    }
}
