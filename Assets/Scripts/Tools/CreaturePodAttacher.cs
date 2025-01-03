using NUnit.Framework;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CreaturePodAttacher:MonoBehaviour
{

    public GameObject prefab;

    public GameObject vinePrefab;

    public List<GameObject> objectsToConvert;

    [SerializeField]
    private List<GameObject> createdObjects;

    public void convertAll()
    {
        createdObjects.Clear();
        objectsToConvert.ForEach(obj => convert(obj));
        objectsToConvert.Clear();
        objectsToConvert.AddRange(createdObjects);
    }

    private void convert(GameObject go)
    {
        GameObject newgo = (GameObject)PrefabUtility.InstantiatePrefab(prefab, go.transform.parent);
        newgo.transform.position = go.transform.position;
        newgo.transform.localScale = go.transform.localScale;
        newgo.transform.rotation = go.transform.rotation;
        newgo.name = go.name;
        createdObjects.Add(newgo);

        GameObject.DestroyImmediate(go);


    }
}
