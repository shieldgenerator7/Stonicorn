using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class SavableObjectInfo : ObjectInfo
{
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
