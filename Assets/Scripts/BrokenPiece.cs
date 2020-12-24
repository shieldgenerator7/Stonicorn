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
        //Initialize this object
        Scene scene = original.scene;
        transform.position = original.transform.position;
        transform.rotation = original.transform.rotation;
        SceneLoader.moveToScene(gameObject, scene);

        //Initialize child objects
        Vector3 origScale = original.transform.localScale;
        SpriteRenderer origSR = original.GetComponent<SpriteRenderer>();
        foreach (GameObject go in Savables)
        //foreach (Transform t in transform)
        {
            Transform t = go.transform;
            //GameObject go = t.gameObject;
            //Scale and Position
            t.localScale = new Vector3(
                t.localScale.x * origScale.x,
                t.localScale.y * origScale.y,
                t.localScale.z * origScale.z
            );
            t.localPosition = new Vector2(
                t.localPosition.x * t.localScale.x,
                t.localPosition.y * t.localScale.y
                );
            //SpriteRenderer
            SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
            sr.color = origSR.color;
            sr.sortingLayerID = origSR.sortingLayerID;
            sr.sortingOrder = origSR.sortingOrder;
            //Copyables
            foreach (ICopyable copy in original.GetComponents<ICopyable>())
            {
                ICopyable mb = (ICopyable)go.GetComponent(copy.CopyableType);
                if (mb != null)
                {
                    mb.copyFrom(original);
                }
            }
            //Unparent it
            t.SetParent(null);
            //Put it in the scene
            SceneLoader.moveToScene(go, scene);
        }
        //Delete this object
        Destroy(gameObject);
    }
}
