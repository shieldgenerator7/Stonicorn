using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.AddressableAssets;

/// <summary>
/// Contains information about a GameObject
/// to make it work better with the save system
/// </summary>
/// 
[DisallowMultipleComponent]
public class ObjectInfo : MonoBehaviour
{
    [SerializeField]
    private int id = -1;
    public int Id
    {
        get => id;
        set => id = value;
    }
    [SerializeField]
    private AssetReference prefabAddress;
    public string PrefabGUID => prefabAddress.AssetGUID;

#if UNITY_EDITOR
    public void autoset()
    {
        prefabAddress = new AssetReference(
            AssetDatabase.AssetPathToGUID(
            AssetDatabase.GetAssetPath(gameObject)
            )
            );
    }
#endif
}
