using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Contains information about a GameObject
/// to make it work better with the save system
/// </summary>
public class ObjectInfo : MonoBehaviour
{
    [SerializeField]
    private string prefabName;
    public string PrefabName => prefabName;
}
