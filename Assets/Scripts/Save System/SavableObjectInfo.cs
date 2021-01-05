using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class SavableObjectInfo : ObjectInfo
{
    [SerializeField]
    private AssetReference prefabAddress;
    public AssetReference PrefabAddress => prefabAddress;
    public virtual string PrefabGUID => prefabAddress.AssetGUID;

    public SavableObjectInfoData Data
        => new SavableObjectInfoData(this);

#if UNITY_EDITOR
    public virtual void autoset()
    {
        prefabAddress = new AssetReference(
            AssetDatabase.AssetPathToGUID(
            AssetDatabase.GetAssetPath(gameObject)
            )
            );
    }
#endif
}
