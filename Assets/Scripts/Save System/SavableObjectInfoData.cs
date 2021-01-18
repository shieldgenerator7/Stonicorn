using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

[System.Serializable]
public struct SavableObjectInfoData
{
    public int id;
    public string prefabGUID;
    [SerializeField]
    [ES3NonSerializable]
    private AssetReference prefabAddress;

    public SavableObjectInfoData(SavableObjectInfo soi)
    {
        this.id = soi.Id;
        this.prefabGUID = soi.PrefabGUID;
        this.prefabAddress = soi.PrefabAddress;
    }

    public override bool Equals(object obj)
        => this == (SavableObjectInfoData)obj;

    public override int GetHashCode()
        => id;

    public static bool operator ==(SavableObjectInfoData soid1, SavableObjectInfoData soid2)
        => soid1.id == soid2.id;
    public static bool operator !=(SavableObjectInfoData soid1, SavableObjectInfoData soid2)
        => soid1.id != soid2.id;
}
