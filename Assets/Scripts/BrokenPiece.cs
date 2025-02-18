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
    [AutoInitialize(SearchChildren = true),SerializeField,HideInInspector]
    private List<SavableObjectInfo> savables;
    public List<SavableObjectInfo> Savables=>savables;

    public void unpack(GameObject original)
    {
        //Reparent child objects to this object
        foreach (SavableObjectInfo soi in Savables)
        {
            //(apparently something else is unparenting it before this)
            //TODO: find out how it gets unparent and figure out if it should be doing that
            soi.transform.SetParent(transform);
        }

        //Initialize this object
        Scene scene = original.scene;
        transform.position = original.transform.position;
        transform.rotation = original.transform.rotation;
        transform.localScale = original.transform.localScale;

        //Initialize child objects
        foreach (SavableObjectInfo soi in Savables)
        {
            //Unparent it
            soi.transform.SetParent(null);
            //Put it in the scene
            Managers.Scene.moveToScene(soi, scene);
        }

        //Delete this object
        Destroy(gameObject);
    }
}
