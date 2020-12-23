using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
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
                merky = states.Find(os => os.objectName == "merky");
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
    public GameState(ICollection<GameObject> list) : this()
    {
        //Object States
        foreach (GameObject go in list)
        {
            try
            {
                ObjectState os = new ObjectState(go);
                states.Add(os);
                if (go.name == "merky")
                {
                    Merky = os;
                }
            }
            catch (NullReferenceException)
            {
                Debug.LogError(
                    "Object " + go.name + " does not have an ObjectInfo.",
                    go
                    );
            }
        }
    }
    //Loading
    public void load()
    {
        states.ForEach(os => os.loadState());
    }
    public bool loadObject(GameObject go)
    {
        ObjectState state = states.Find(
            os => os.sceneName == go.scene.name && os.objectName == go.name
            );
        if (state != null)
        {
            state.loadState();
            return true;
        }
        return false;
    }
    //
    //Spawned Objects
    //

    //
    //Gets the list of the game objects that have object states in this game state
    //
    public List<GameObject> getGameObjects()
        => states.ConvertAll(os => os.getGameObject());

    //Returns true IFF the given GameObject has an ObjectState in this GameState
    public bool hasGameObject(GameObject go)
    {
        if (go == null)
        {
            throw new System.ArgumentNullException("GameState.hasGameObject() cannot accept null for go! go: " + go);
        }
        return states.Any(
            os => os.objectName == go.name && os.sceneName == go.scene.name
            );
    }
}
