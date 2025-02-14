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
    public SavableObject[] soList;
    //Name
    public int objectId = -1;
    public int sceneId = -1;

    public ObjectState() { }
    public ObjectState(SavableObjectInfo info)
    {
        objectId = info.Id;
        sceneId = info.gameObject.scene.buildIndex;
        saveState(info);
    }

    private void saveState(SavableObjectInfo info)
    {
        //Transform
        position = info.transform.position;
        localScale = info.transform.localScale;
        rotation = info.transform.rotation;
        //Rigidbody2D
        Rigidbody2D rb2d = info.Rigidbody2D;
        if (rb2d != null)
        {
            velocity = rb2d.linearVelocity;
            angularVelocity = rb2d.angularVelocity;
        }
        //SavableMonoBehaviours
        soList = info.savables.ConvertAll<SavableObject>(smb => smb.CurrentState).ToArray();
    }
    public void loadState(SavableObjectInfo soi)
    {
        soi.transform.position = position;
        soi.transform.localScale = localScale;
        soi.transform.rotation = rotation;
        Rigidbody2D rb2d = soi.Rigidbody2D;
        if (rb2d != null)
        {
            rb2d.linearVelocity = velocity;
            rb2d.angularVelocity = angularVelocity;
        }
        foreach (SavableObject so in this.soList)
        {
            SavableMonoBehaviour smb = soi.getSavableMonoBehaviour(so.ScriptType);
            if (smb == null)
            {
                if (!so.isSpawnedScript)
                {
                    throw new UnityException($"Object {soi} ({soi.Id}) is missing non-spawnable script {so.scriptType}");
                }
                //Add the spawnable script
                    smb = so.addScript(soi);
            }
            //load state of the savable script
            try
            {
                smb.CurrentState = so;
            }
            catch (InvalidCastException ice)
            {
                Debug.Log($"InvalidCastException on go {soi.name} ({soi.Id}): {smb.GetType()}: {ice}");
            }
        }
    }
}
