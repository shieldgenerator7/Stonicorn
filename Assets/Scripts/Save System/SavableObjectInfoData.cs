using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SavableObjectInfoData
{
    public int id;
    public string prefabGUID;

    public SavableObjectInfoData(SavableObjectInfo soi)
    {
        this.id = soi.Id;
        this.prefabGUID = soi.PrefabGUID;
    }
}
