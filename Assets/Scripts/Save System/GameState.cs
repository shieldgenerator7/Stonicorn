using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

public class GameState
{
    public int id;
    public List<ObjectState> states = new List<ObjectState>();
    private ObjectState merky;//the object state in the list specifically for Merky
    public ObjectState Merky
    {
        get
        {
            if (merky == null)
            {
                merky = states.Find(os => os.objectId == 0);
            }
            return merky;
        }
        private set => merky = value;
    }

    public static int nextid = 0;

    //Instantiation
    public GameState()
    {
        id = nextid;
        nextid++;
    }
    public GameState(List<GameObject> list) : this()
    {
        //Object States
        foreach (GameObject go in list)
        {
            if (!go || ReferenceEquals(go, null))
            {
                //skip null objects
                continue;
            }
            try
            {
                ObjectState os = new ObjectState(go);
                if (os.objectId < 0)
                {
                    throw new UnityException("Object state object id is " + os.objectId + " for object: " + go);
                }
                states.Add(os);
            }
            catch (NullReferenceException nre)
            {
                Debug.LogError(
                    "Object " + go.name + " does not have an ObjectInfo. NRE: " + nre,
                    go
                    );
            }
        }
    }
    //Loading
    public void load()
    {
        states.ForEach(os =>
        {
            if (Managers.Object.hasObject(os.objectId))
            {
                os.loadState(Managers.Object.getObject(os.objectId));
            }
            else
            {
                Debug.Log("Object (" + os.objectId + ") not found");
                if (Managers.Scene.isObjectSceneOpen(os.objectId))
                {
                    Managers.Object.recreateObject(os.objectId);
                }
                else
                {
                    Debug.Log("Object (" + os.objectId + ") scene not open, so ignoring");
                }
            }
        });
    }
    public void loadObject(GameObject go)
    {
        int key = go.getKey();
        ObjectState state = states.Find(os => os.objectId == key);
        state.loadState(go);
    }

    //Returns true IFF the given GameObject has an ObjectState in this GameState
    public bool hasGameObject(GameObject go)
    {
        if (go == null)
        {
            throw new System.ArgumentNullException("GameState.hasGameObject() cannot accept null for go! go: " + go);
        }
        int key = go.getKey();
        return states.Any(os => os.objectId == key);
    }
}
