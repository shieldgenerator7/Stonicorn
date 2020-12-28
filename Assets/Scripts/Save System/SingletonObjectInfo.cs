using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingletonObjectInfo : SavableObjectInfo
{
    public override string PrefabGUID => "Invalid Operation";

#if UNITY_EDITOR
    public override void autoset()
    {
        Debug.LogError("Cannot set the prefab GUID on a singleton.", gameObject);
    }
#endif
}
