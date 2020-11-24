using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

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
        GameObject newObject = (GameObject)PrefabUtility.InstantiatePrefab(newPrefab);
        newObject.transform.parent = oldObject.transform.parent;
        newObject.transform.position = oldObject.transform.position;
        newObject.transform.up = oldObject.transform.up;
        newObject.transform.localScale = oldObject.transform.localScale;
        newObject.name = oldObject.name;
        return newObject;
    }

    public void setOldObjs(List<GameObject> oldObjs)
    {
        Debug.Log("oldObjs count: " + oldObjs.Count);
        //If any of the selected objects has a prefab switcher
        if (oldObjs.Any(go => go.GetComponent<PrefabSwitcher>() != null))
        {
            //Don't do anything
            return;
        }
        this.oldObjects.Clear();
        oldObjs
            .FindAll(go => go is GameObject)
            .ForEach(
                go => oldObjects.Add((GameObject)go)
                );
    }
}

