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
public abstract class ObjectInfo : MonoBehaviour
{
    [SerializeField]
    private int id = -1;
    public int Id
    {
        get => id;
        set => id = value;
    }
}
