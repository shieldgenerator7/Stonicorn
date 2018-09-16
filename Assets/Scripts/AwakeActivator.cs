using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This script is used to awake objects at program start time
/// so that they can be deactive during edit time without any problems
/// </summary>
public class AwakeActivator : MonoBehaviour
{
    public List<GameObject> objectsToActivate = new List<GameObject>();

    void Awake()
    {
        foreach (GameObject go in objectsToActivate)
        {
            go.SetActive(true);
        }
    }
}
