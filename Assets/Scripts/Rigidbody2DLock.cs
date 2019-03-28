using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rigidbody2DLock : SavableMonoBehaviour {
    //2017-01-31: this class is used to make RigidBody2D components kinematic on certain objects

    public List<GameObjectId> lockHolders = new List<GameObjectId>();//the objects that have a lock on this rb2d
    	
    public void lockRB2D()
    {
        Rigidbody2D rb2d = gameObject.GetComponent<Rigidbody2D>();
        rb2d.isKinematic = true;//lock rb2d
        rb2d.velocity = Vector2.zero;
        rb2d.angularVelocity = 0;
    }

	public void addLock(GameObject lockHolder)
    {
        lockRB2D();
        lockHolders.Add(GameObjectId.createId(lockHolder));
    }

    public void removeLock(GameObject lockHolder)
    {
        for (int i = lockHolders.Count-1; i >= 0; i--)
        {
            if (lockHolders[i].matches(lockHolder))
            {
                lockHolders.RemoveAt(i);
            }
        }
        if (lockHolders.Count == 0)
        {
            removeLock();
        }
    }
    private void removeLock()
    {
        gameObject.GetComponent<Rigidbody2D>().isKinematic = false;//unlock rb2d
        Destroy(this);//take off lock altogether
    }

    public bool holdsLock(GameObject lockHolder)
    {
        foreach (GameObjectId goId in lockHolders)
        {
            if (goId.matches(lockHolder))
            {
                return true;
            }
        }
        return false;
    }

    public override SavableObject getSavableObject()
    {
        SavableObject so = new SavableObject(this);
        int counter = 0;
        foreach (GameObjectId goId in lockHolders)
        {
            so.data.Add("gameObjectId" + counter, goId);
            counter++;
        }
        so.data.Add("gameObjectCount", counter);
        return so;
    }
    public override void acceptSavableObject(SavableObject savObj)
    {
        lockHolders = new List<GameObjectId>();
        for (int i = 0; i < (int)savObj.data["gameObjectCount"]; i++)
        {
            lockHolders.Add((GameObjectId)savObj.data["gameObjectId" + i]);
        }
        if (lockHolders.Count > 0)
        {
            lockRB2D();
        }
        else
        {
            removeLock();
        }
    }
    public override bool isSpawnedScript()
    {
        return true;
    }
}