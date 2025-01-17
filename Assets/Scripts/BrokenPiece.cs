using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Breaks apart a spawned prefab into its child pieces
/// </summary>
[DisallowMultipleComponent]
public class BrokenPiece : MonoBehaviour, ISavableContainer
{
    private List<GameObject> savables;
    public List<GameObject> Savables
    {
        get
        {
            if (savables == null)
            {
                savables = new List<GameObject>();
                foreach (Transform t in transform)
                {
                    savables.Add(t.gameObject);
                }
            }
            return savables;
        }
    }

    public void unpack(GameObject original)
    {
        //Reparent child objects to this object
        foreach (GameObject go in Savables)
        {
            //(apparently something else is unparenting it before this)
            //TODO: find out how it gets unparent and figure out if it should be doing that
            go.transform.SetParent(transform);
        }

        //Initialize this object
        Scene scene = original.scene;
        transform.position = original.transform.position;
        transform.rotation = original.transform.rotation;
        Managers.Scene.moveToScene(gameObject, scene);

        //Initialize child objects
        foreach (GameObject go in Savables)
        {
            //Unparent it
            go.transform.SetParent(null);
            //Put it in the scene
            Managers.Scene.moveToScene(go, scene);
        }
        //Delete this object
        Destroy(gameObject);
    }
}
