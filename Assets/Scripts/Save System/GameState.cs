using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

public struct GameState
{
    [ES3Serializable]
    public int id;
    [ES3Serializable]
    private ObjectState[] states;
    [ES3Serializable]
    private ObjectState merky;//the object state in the list specifically for Merky
    public ObjectState Merky => merky;

    [ES3NonSerializable]
    public static int nextid = 0;

    private static ObjectState INVALID_STATE = new ObjectState();

    //Instantiation
    public GameState(int testId)
    {
        this.id = testId;
        states = new ObjectState[0];
        merky = null;
    }
    public GameState(List<SavableObjectInfo> list)
    {
        //id
        id = nextid;
        nextid++;

        //Object States
        states = list
            //.Where(soi => soi && !ReferenceEquals(soi, null)).ToList()//apparently sometimes things can get destroyed before the savable list can be updated
            .ConvertAll(soi =>
            {
                //2025-02-17: apparently sometimes things can get destroyed before the savable list can be updated
                //checking here to avoid recreating a list
                if (!soi || ReferenceEquals(soi, null))
                {
                    return INVALID_STATE;
                }
                    ObjectState os = new ObjectState(soi);
                    if (os.objectId < 0)
                    {
                        throw new UnityException($"Object state object id is ({os.objectId}) for object: {soi.name}");
                    }
                    return os;
            })
            .Where(os=>os.Valid)
            .OrderBy(os => os.objectId)
            .ToArray();

        //Merky
        merky = states.First(os => os.objectId == 0);
    }
    //Loading
    public void load()
    {
        for (int i = 0; i < states.Length; i++)
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
    public void loadObject(SavableObjectInfo soi)
    {
        int key = soi.Id;
        ObjectState state = states.First(os => os.objectId == key);
        state.loadState(soi);
    }

    /// <summary>
    /// Returns true IFF the given GameObject has an ObjectState in this GameState
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public bool hasGameObject(int key)
    {
        return states.Any(os => os.objectId == key);
    }

    internal void processStates(Func<ObjectState, int> func)
    {
        for (int i = 0; i < states.Length; i++)
        {
            func(states[i]);
        }
    }

    /// <summary>
    /// Work around a bug that causes merky to not be found in a gamestate
    /// </summary>
    /// <param name="merky"></param>
    internal void setMerky(SingletonObjectInfo merky)
    {
        if (Merky != null)
        {
            Debug.LogError($"GameState.setMerky(): game state ({id}) already has a Merky! {Merky}");
            return;
        }
        ObjectState os = new ObjectState(merky);
        List<ObjectState> ls = states.ToList();
        ls.Add(os);
        states = ls.ToArray();
        this.merky = os;
    }

    public bool Valid
        => id >= 0 
        && states != null && states.Length > 0 
        && merky != null;
}
