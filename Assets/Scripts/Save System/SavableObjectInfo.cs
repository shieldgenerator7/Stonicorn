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
    public int spawnStateId = -1;//-1 is an invalid Id but it forces save on new objects
    public int destroyStateId = int.MaxValue;//the game state id in which this object was destroyed (max value for not destroyed)

    public SavableObjectInfoData Data
    {
        get => new SavableObjectInfoData(this);
        set
        {
            SavableObjectInfoData soid = value;
            this.Id = soid.id;
            this.spawnStateId = soid.spawnStateId;
            this.destroyStateId = soid.destroyStateId;
        }
    }

#if UNITY_EDITOR
    public virtual void autoset()
    {
        prefabAddress = new AssetReference(
            AssetDatabase.AssetPathToGUID(
            AssetDatabase.GetAssetPath(gameObject)
            )
            );
        spawnStateId = 0;
        destroyStateId = int.MaxValue;
    }
#endif
}
