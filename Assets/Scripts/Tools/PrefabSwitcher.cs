using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PrefabSwitcher : MonoBehaviour
{
    public GameObject newPrefab;
    public List<GameObject> oldObjects;

    public GameObject switchPrefab(GameObject oldObject, GameObject newPrefab = null)
    {
        if (newPrefab == null)
        {
            newPrefab = this.newPrefab;
        }
        GameObject newObject = Instantiate(newPrefab);
        newObject.transform.position = oldObject.transform.position;
        newObject.transform.up = oldObject.transform.up;
        newObject.transform.localScale = oldObject.transform.localScale;
        newObject.name = oldObject.name;
        return newObject;
    }
}
