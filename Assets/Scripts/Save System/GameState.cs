using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

public struct GameState
{
    public int id;
    private ObjectState[] states;
    private ObjectState merky;//the object state in the list specifically for Merky
    public ObjectState Merky => merky;

    public static int nextid = 0;

    //Instantiation
    public GameState(int testId)
    {
        this.id = testId;
        states = new ObjectState[0];
        merky = null;
    }
    public GameState(List<GameObject> list)
    {
        //id
        id = nextid;
        nextid++;

        //Object States
        states = list
            .Where(go => !(!go || ReferenceEquals(go, null))).ToList()
            .ConvertAll(go => {
            try
            {
                ObjectState os = new ObjectState(go);
                if (os.objectId < 0)
                {
                    throw new UnityException($"Object state object id is ({os.objectId}) for object: {go.name}");
                }
                return os;
            }
            catch (NullReferenceException nre)
            {
                Debug.LogError(
                    $"Object {go.name} does not have an ObjectInfo. NRE: {nre}",
                    go
                    );
                return null;
            }
        })
            .Where(os => os != null)
            .OrderBy(os=>os.objectId)
            .ToArray();

        //Merky
        merky = states.First(os => os.objectId == 0);
    }
    //Loading
    public void load()
    {
        for(int i = 0; i < states.Length; i++) 
        {
            ObjectState os = states[i];
            if (Managers.Object.hasObject(os.objectId))
            {
                os.loadState(Managers.Object.getObject(os.objectId));
            }
            else
            {
                Debug.Log($"Object ({os.objectId}) not found");
                if (Managers.Scene.isObjectSceneOpen(os.objectId))
                {
                    Debug.Log($"Object ({os.objectId}) scene open, recreating");
                    Managers.Object.recreateObject(os.objectId);
                }
                else
                {
                    Debug.Log($"Object ({os.objectId}) scene not open, so not recreating");
                }
            }
        };
    }
    public void loadObject(GameObject go)
    {
        int key = go.getKey();
        ObjectState state = states.First(os => os.objectId == key);
        state.loadState(go);
    }

    /// <summary>
    /// Returns true IFF the given GameObject has an ObjectState in this GameState
    /// </summary>
    /// <param name="go"></param>
    /// <returns></returns>
    /// <exception cref="System.ArgumentNullException"></exception>
    public bool hasGameObject(GameObject go)
    {
        if (go == null)
        {
            throw new System.ArgumentNullException($"GameState.hasGameObject() cannot accept null for go! go: {go}");
        }
        int key = go.getKey();
        return hasGameObject(key);
    }

    /// <summary>
    /// Returns true IFF the given GameObject has an ObjectState in this GameState
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public bool hasGameObject(int key) { 
        return states.Any(os => os.objectId == key);
    }

    internal void processStates(Func<ObjectState, int> func)
    {
        for (int i = 0; i < states.Length; i++)
        {
            func(states[i]);
        }
    }

    public bool valid => id >= 0;
}
