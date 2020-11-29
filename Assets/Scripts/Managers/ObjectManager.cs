using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ObjectManager : MonoBehaviour
{
    private Dictionary<string, GameObject> gameObjects = new Dictionary<string, GameObject>();//list of current objects that have state to save
    public List<GameObject> GameObjects => gameObjects.Values.ToList();
    public int GameObjectCount => gameObjects.Count;
    private List<GameObject> forgottenObjects = new List<GameObject>();//a list of objects that are inactive and thus unfindable, but still have state to save
    public List<GameObject> ForgottenObjects => forgottenObjects;

    //Memories
    private Dictionary<string, MemoryObject> memories = new Dictionary<string, MemoryObject>();//memories that once turned on, don't get turned off

    /// <summary>
    /// Adds an object to list of objects that have state to save
    /// </summary>
    /// <param name="go">The GameObject to add to the list</param>
    public void addObject(GameObject go, bool isSpawnedObject = false)
    {
        //
        //Error checking
        //

        //If go is null
        if (go == null)
        {
            throw new System.ArgumentNullException("GameObject (" + go + ") cannot be null!");
        }

        //getKey() returns a string containing
        //the object's name and scene name
        string key = go.getKey();

        //If the game object's name is already in the dictionary,
        if (gameObjects.ContainsKey(key)
            //And it's not null...
            && !(gameObjects[key] == null
            || ReferenceEquals(gameObjects[key], null)))
        {
            if (isSpawnedObject)
            {
                Debug.LogWarning("ObjectManager.adObject(): replacing" +
                    gameObjects[key] + " with " + go);
                gameObjects[key] = go;
                return;
            }
            else
            {
                throw new System.ArgumentException(
                      "GameObject (" + key + ") is already inside the gameObjects dictionary! "
                      + "Check for 2 or more objects with the same name."
                      );
            }
        }
        //If the game object doesn't have any state to save...
        if (!go.isSavable())
        {
            throw new System.ArgumentException(
                "GameObject (" + key + ") doesn't have any state to save! "
                + "Check to make sure it has a Rigidbody2D or a SavableMonoBehaviour."
                );
        }
        //Else if all good, add the object
        gameObjects.Add(key, go);
    }

    /// <summary>
    /// Retrieves the GameObject from the gameObjects list with the given scene and object names
    /// </summary>
    /// <param name="sceneName">The scene name of the object</param>
    /// <param name="objectName">The name of the object</param>
    /// <returns></returns>
    public GameObject getObject(string sceneName, string objectName)
    {
        string key = Utility.getKey(sceneName, objectName);
        //If the gameObjects list has the game object,
        if (gameObjects.ContainsKey(key))
        {
            //Return it
            return gameObjects[key];
        }
        //Otherwise, sorry, you're out of luck
        return null;
    }

    public List<GameObject> getObjectsWithName(string startsWith)
    {
        List<GameObject> matchingGOs = new List<GameObject>();
        //Search for GameObjects that start with the given string
        foreach (GameObject go in gameObjects.Values)
        {
            if (go.name.StartsWith(startsWith))
            {
                matchingGOs.Add(go);
            }
        }
        return matchingGOs;
    }
    /// <summary>
    /// Destroys the given GameObject and updates lists
    /// </summary>
    /// <param name="go">The GameObject to destroy</param>
    public void destroyObject(GameObject go)
    {
        removeObject(go);
        Destroy(go);
    }
    /// <summary>
    /// Removes the given GameObject from the gameObjects list
    /// </summary>
    /// <param name="go">The GameObject to remove from the list</param>
    private void removeObject(GameObject go)
    {
        gameObjects.Remove(go.getKey());
        forgottenObjects.Remove(go);
        //If go is not null and has children,
        if (go && go.transform.childCount > 0)
        {
            //For each of its children,
            foreach (Transform t in go.transform)
            {
                //Remove it from the gameObjects list
                gameObjects.Remove(t.gameObject.getKey());
                //And from the forgotten objects list
                forgottenObjects.Remove(t.gameObject);
            }
        }
    }
    /// <summary>
    /// Remove null objects from the gameObjects list
    /// </summary>
    public void cleanObjects()
    {
        string cleanedKeys = "";
        //Copy the game object keys
        List<string> keys = new List<string>(gameObjects.Keys);
        //Loop over copy list
        foreach (string key in keys)
        {
            //If the key's value is null,
            if (gameObjects[key] == null
                || ReferenceEquals(gameObjects[key], null))
            {
                //Clean the key out
                cleanedKeys += key + ", ";
                gameObjects.Remove(key);
            }
        }
        //Write out to the console which keys were cleaned
        if (cleanedKeys != "")
        {
            Debug.LogWarning("Cleaned: " + cleanedKeys);
        }
    }

    /// <summary>
    /// Clear all objects from the list
    /// </summary>
    public void clearObjects()
    {
        gameObjects.Clear();
        forgottenObjects.Clear();
        memories.Clear();
    }

    /// <summary>
    /// Update the list of GameObjects with state to save
    /// </summary>
    public void refreshGameObjects()
    {
        //Make a new dictionary for the list
        gameObjects = new Dictionary<string, GameObject>();
        //Add objects that can move
        foreach (Rigidbody2D rb in FindObjectsOfType<Rigidbody2D>())
        {
            addObject(rb.gameObject);
        }
        //Add objects that have other variables that can get rewound
        foreach (SavableMonoBehaviour smb in FindObjectsOfType<SavableMonoBehaviour>())
        {
            if (!gameObjects.ContainsValue(smb.gameObject))
            {
                addObject(smb.gameObject);
            }
        }
        //Forgotten Objects
        foreach (GameObject fgo in forgottenObjects)
        {
            if (fgo != null)
            {
                addObject(fgo);
            }
        }
        //Memories
        foreach (MemoryMonoBehaviour mmb in FindObjectsOfType<MemoryMonoBehaviour>())
        {
            string key = mmb.gameObject.getKey();
            //If the memory has already been stored,
            if (memories.ContainsKey(key))
            {
                //Load the memory
                mmb.acceptMemoryObject(memories[key]);
            }
            //Else
            else
            {
                //Save the memory
                memories.Add(key, mmb.getMemoryObject());
            }
        }
    }

    /// <summary>
    /// Stores the given object before it gets set inactive
    /// </summary>
    /// <param name="obj"></param>
    public void saveForgottenObject(GameObject obj, bool forget = true)
    {
        //Error checking
        if (obj == null)
        {
            throw new System.ArgumentNullException("GameManager.saveForgottenObject() cannot accept null for obj! obj: " + obj);
        }
        //If it's about to be set inactive,
        if (forget)
        {
            //Add it to the list,
            forgottenObjects.Add(obj);
            //And set it inactive
            obj.SetActive(false);
        }
        //Else,
        else
        {
            //Remove it from the list,
            forgottenObjects.Remove(obj);
            //And set it active again
            obj.SetActive(true);
        }
    }

    #region Memory List Management
    /// <summary>
    /// Saves the memory to the memory list
    /// </summary>
    /// <param name="mmb"></param>
    public void saveMemory(MemoryMonoBehaviour mmb)
    {
        string key = mmb.gameObject.getKey();
        MemoryObject mo = mmb.getMemoryObject();
        //If the memory is already stored,
        if (memories.ContainsKey(key))
        {
            //Update it
            memories[key] = mo;
        }
        //Else
        else
        {
            //Add it
            memories.Add(key, mo);
        }
    }
    /// <summary>
    /// Restore all saved memories of game objects that have a memory saved
    /// </summary>
    public void LoadMemories()
    {
        //Find all the game objects that can have memories
        foreach (MemoryMonoBehaviour mmb in FindObjectsOfType<MemoryMonoBehaviour>())
        {
            string key = mmb.gameObject.getKey();
            //If there's a memory saved for this object,
            if (memories.ContainsKey(key))
            {
                //Tell that object what it is
                mmb.acceptMemoryObject(memories[key]);
            }
        }
    }
    #endregion

    public void saveToFile(string fileName)
    {
        ES3.Save<Dictionary<string, MemoryObject>>(
            "memories",
            memories,
            fileName
            );
    }
    public void loadFromFile(string fileName)
    {
        //Load memories
        memories = ES3.Load<Dictionary<string, MemoryObject>>(
            "memories",
            fileName
            );
    }
}
