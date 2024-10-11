using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

public class ObjectState
{
    //Transform
    public Vector3 position;//2017-10-10: actually stores the localPosition
    public Vector3 localScale;
    public Quaternion rotation;//2017-10-10: actually stores the localRotation
    //RigidBody2D
    public Vector2 velocity;
    public float angularVelocity;
    //Saveable Object
    public List<SavableObject> soList;
    //Name
    public int objectId = -1;
    public int sceneId = -1;

    public ObjectState() { }
    public ObjectState(GameObject go)
    {
        SavableObjectInfo info = go.GetComponent<SavableObjectInfo>();
        objectId = info.Id;
        sceneId = go.scene.buildIndex;
        saveState(go);
    }

    private void saveState(GameObject go)
    {
        //Transform
        position = go.transform.position;
        localScale = go.transform.localScale;
        rotation = go.transform.rotation;
        //Rigidbody2D
        Rigidbody2D rb2d = go.GetComponent<Rigidbody2D>();
        if (rb2d != null)
        {
            velocity = rb2d.linearVelocity;
            angularVelocity = rb2d.angularVelocity;
        }
        //SavableMonoBehaviours
        List<SavableMonoBehaviour> smbs = go.GetComponents<SavableMonoBehaviour>().ToList();
        soList = smbs.ConvertAll<SavableObject>(smb => smb.CurrentState);
    }
    public void loadState(GameObject go)
    {
        go.transform.position = position;
        go.transform.localScale = localScale;
        go.transform.rotation = rotation;
        Rigidbody2D rb2d = go.GetComponent<Rigidbody2D>();
        if (rb2d != null)
        {
            rb2d.linearVelocity = velocity;
            rb2d.angularVelocity = angularVelocity;
        }
        foreach (SavableObject so in this.soList)
        {
            SavableMonoBehaviour smb =
                (SavableMonoBehaviour)go.GetComponent(so.ScriptType);
            if (smb == null)
            {
                if (so.isSpawnedScript)
                {
                    smb = (SavableMonoBehaviour)so.addScript(go);
                }
                else
                {
                    throw new UnityException(
                        "Object " + go + " (" + go.getKey()
                        + ") is missing non-spawnable script " + so.scriptType
                        );
                }
            }
            try
            {
                smb.CurrentState = so;
            }
            catch (InvalidCastException ice)
            {
                Debug.Log($"InvalidCastException {ice}");
            }
        }
    }
}
