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
    public GameState(List<GameObject> list) : this()
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
        states.ForEach(os =>
        {
            GameObject go = Managers.Object.getObject(os.objectName);
            if (go != null && !ReferenceEquals(go, null))
            {
                os.loadState(go);
            }
            else
            {
                Scene scene = SceneManager.GetSceneByName(os.sceneName);
                if (scene.IsValid() && scene.isLoaded)
                {
                    //If scene is valid, Create the GameObject
                    Managers.Object.createObject(os.objectName, os.prefabGUID)
                        .Completed += (op) =>
                        {
                            os.loadState(op.Result);
                            SceneLoader.moveToScene(op.Result, os.sceneName);
                        };
                }
            }
        });
    }
    public bool loadObject(GameObject go)
    {
        ObjectState state = states.Find(
            os => os.sceneName == go.scene.name && os.objectName == go.name
            );
        if (state != null)
        {
            state.loadState(go);
            return true;
        }
        return false;
    }

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
