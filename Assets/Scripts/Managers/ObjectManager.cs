using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class ObjectManager : MonoBehaviour, ISetting
{
    private Dictionary<int, GameObject> gameObjects = new Dictionary<int, GameObject>();//list of current objects that have state to save
    public List<GameObject> GameObjects => gameObjects.Values.ToList();
    public int GameObjectCount => gameObjects.Count;

    //Memories
    private Dictionary<int, MemoryObject> memories = new Dictionary<int, MemoryObject>();//memories that once turned on, don't get turned off

    //Create queue
    private Dictionary<int, AsyncOperationHandle<GameObject>> recreateQueue = new Dictionary<int, AsyncOperationHandle<GameObject>>();
    public List<SavableObjectInfoData> knownObjects;

    public void LoadSceneObjects(List<GameObject> sceneGOs, List<int> foreignIds, int lastStateSeen)
    {
        sceneGOs.ForEach(go => addObject(go));
        foreignIds.FindAll(id => !hasObject(id))
            .ForEach(id => recreateObject(id, lastStateSeen));
    }
    public void LoadObjectsPostRewind(List<GameState> gameStates, int gameStateId)
    {
        Debug.Log("Checking objects after rewinding to state " + gameStateId);
        //Remove null objects from the list
        cleanObjects();
        //Destroy objects not spawned yet in the new selected state
        int prevCount = GameObjects.Count;
        knownObjects
            .FindAll(soid => soid.spawnStateId > gameStateId)
            .ForEach(soid => destroyAndForgetObject(soid.id));
        int nowCount = GameObjects.Count;
        Debug.Log("Check after rewind complete: " + (prevCount - nowCount) + " objects removed");
    }

    public List<GameObject> onPreGameStateSaved()
    {
        //Remove any null objects from the list
        cleanObjects();
        //Return game object list
        return GameObjects;
    }

    public void recreateObject(int goId, int lastStateSeen = -1)
    {
        if (goId > 0)
        {
            string prefabGUID = knownObjects.Find(soid => soid.id == goId).prefabGUID;
            Debug.Log("Recreating goId: " + goId + " using prefab "
#if UNITY_EDITOR
                + AssetDatabase.GUIDToAssetPath(prefabGUID)
#endif
                );
            recreateObject(goId, prefabGUID, lastStateSeen);
        }
        else
        {
            throw new ArgumentException("Id must be 0 or greater! id: " + goId);
        }
    }

    /// <summary>
    /// Used when an object that existed previously
    /// has also previously been destroyed or unloaded,
    /// and now must be reinstated.
    /// Precondition: the game object does not already exist (has not been found).
    /// </summary>
    /// <param name="goId"></param>
    /// <param name="prefabGUID"></param>
    /// <returns></returns>
    private void /*AsyncOperationHandle<GameObject>*/ recreateObject(int goId, string prefabGUID, int lastStateSeen = -1)
    {
        if (!recreateQueue.ContainsKey(goId))
        {
            try
            {
                AssetReference assetRef = new AssetReference(prefabGUID);
                //2020-12-23: copied from https://youtu.be/uNpBS0LPhaU?t=1000
                var op = Addressables.InstantiateAsync(assetRef);
                recreateQueue.Add(goId, op);
                Debug.Log("Recreating object (" + goId + ")"
#if UNITY_EDITOR
                    + " using prefab " + assetRef.editorAsset.name
#endif
                    );
                op.Completed += (operation) =>
                {
                    GameObject newGO = operation.Result;
                    //Remove "(Clone)" at the end of the name
                    if (newGO.name.Contains("(Clone)"))
                    {
                        newGO.name = newGO.name.Split('(')[0];
                    }
                    //Init the New Game Object
                    newGO.GetComponent<ObjectInfo>().Id = goId;
                    addObject(newGO);
                    foreach (Transform t in newGO.transform)
                    {
                        if (t.gameObject.isSavable())
                        {
                            addObject(t.gameObject);
                        }
                    }
                    //Delegate
                    onObjectRecreated?.Invoke(newGO, lastStateSeen);
                    //Finish up
                    recreateQueue.Remove(goId);
                    if (!RecreatingObjects)
                    {
                        onAllObjectsRecreated?.Invoke();
                    }
                };
            }
            catch (InvalidKeyException ike)
            {
                throw new Exception("InvalidKey: (" + prefabGUID + ") for object (" + goId + "):", ike);
            }
            catch (Exception ike)
            {
                throw new Exception("InvalidKey: (" + prefabGUID + ") for object (" + goId + "):", ike);
            }
        }
        //return createQueue[goId];
    }
    public delegate void OnObjectRecreated(GameObject go, int lastStateSeen);
    public event OnObjectRecreated onObjectRecreated;
    public delegate void OnAllObjectsRecreated();
    public event OnAllObjectsRecreated onAllObjectsRecreated;

    public bool RecreatingObjects => recreateQueue.Count > 0;

    public SettingObject Setting
    {
        get => new SettingObject(ID).addList(
            "knownObjects", knownObjects
            );
        set => knownObjects = value.List<SavableObjectInfoData>("knownObjects");
    }

    public SettingScope Scope => SettingScope.SAVE_FILE;

    public string ID => "ObjectManager";

    /// <summary>
    /// Adds a newly created object to the list
    /// </summary>
    /// <param name="go"></param>
    public void addNewObject(GameObject go)
    {
        SavableObjectInfoData soid = go.GetComponent<SavableObjectInfo>().Data;
        if (!knownObjects.Contains(soid))
        {
            knownObjects.Add(soid);
        }
        addObject(go);
    }

    /// <summary>
    /// Adds an object to list of objects that have state to save
    /// </summary>
    /// <param name="go">The GameObject to add to the list</param>
    public void addObject(GameObject go)
    {
        //
        //Error checking
        //

        //If go is null
        if (go == null)
        {
            throw new System.ArgumentNullException("GameObject (" + go + ") cannot be null!");
        }

        int key = go.getKey();

        //If the key is invalid,
        if (key < 0)
        {
            Debug.LogError(
                "GameObject " + go.name + " has an invalid key: " + key + "!",
                go
                );
            return;
        }

        //If the game object doesn't have any state to save...
        if (!go.isSavable())
        {
            throw new System.ArgumentException(
                "GameObject (" + key + ") doesn't have any state to save! "
                + "Check to make sure it has a Rigidbody2D or a SavableMonoBehaviour."
                );
        }
        //If the game object's name is already in the dictionary,
        if (gameObjects.ContainsKey(key))
        {
            if (go.name != gameObjects[key].name)
            {
                Debug.LogWarning(
                      "Key (" + key + ") is already inside the gameObjects dictionary: "
                      + "GameObject " + go.name + " replacing " + gameObjects[key],
                      go
                      );
            }
            gameObjects[key] = go;
        }
        else
        {
            //Else if all good, add the object
            gameObjects.Add(key, go);
        }
    }

    public bool hasObject(int goKey)
        => gameObjects.ContainsKey(goKey);

    /// <summary>
    /// Retrieves the GameObject from the gameObjects list with the given key
    /// </summary>
    /// <param name="goKey">The unique inter-scene key of the object</param>
    /// <returns></returns>
    public GameObject getObject(int goKey)
    {
        //If the gameObjects list has the game object,
        if (gameObjects.ContainsKey(goKey))
        {
            //Return it
            return gameObjects[goKey];
        }
        Debug.LogError(
            "No object with key found: " + goKey + "!\n"
            + "Check with hasObject() before getting the object."
            );
        //Otherwise, sorry, you're out of luck
        return null;
    }

    /// <summary>
    /// Destroys the object and forgets it so that it cannot be recreated.
    /// If you want to destroy an object through normal means,
    /// use destroyObject() instead.
    /// </summary>
    /// <param name="go"></param>
    public void destroyAndForgetObject(GameObject go)
    {
        SavableObjectInfo soi = go.GetComponent<SavableObjectInfo>();
        if (soi is SingletonObjectInfo)
        {
            //don't destroy the game manager or merky
            return;
        }
        Debug.Log("Destroying object permanently: " + go.name + " (" + soi.Id + ")", go);
        destroyObject(go);
        knownObjects.RemoveAll(soid => soid.id == soi.Id);
    }

    /// <summary>
    /// Destroys the object and forgets it so that it cannot be recreated.
    /// If you want to destroy an object through normal means,
    /// use destroyObject() instead.
    /// </summary>
    /// <param name="go"></param>
    public void destroyAndForgetObject(int id)
    {
        if (hasObject(id)) {
            GameObject go = getObject(id);
            SavableObjectInfo soi = go.GetComponent<SavableObjectInfo>();
            if (soi is SingletonObjectInfo)
            {
                //don't destroy the game manager or merky
                return;
            }
            Debug.Log("Destroying object permanently: " + go.name + " (" + id + ")", go);
            destroyObject(go);
        }
        else
        {
            Debug.Log("Destroying object permanently: [unknown name] (" + id + ")");
        }
        knownObjects.RemoveAll(soid => soid.id == id);
    }

    public void destroyObject(int goKey)
    {
        GameObject go = getObject(goKey);
        if (go)
        {
            destroyObject(go);
        }
    }

    /// <summary>
    /// Destroys the given GameObject and updates lists
    /// </summary>
    /// <param name="go">The GameObject to destroy</param>
    public void destroyObject(GameObject go)
    {
        if (go.GetComponent<ObjectInfo>() is SingletonObjectInfo)
        {
            //don't destroy the game manager or merky
            return;
        }
        Debug.Log("Destroying object (" + go.getKey() + "): " + go.name);
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
        //If go is not null and has children,
        if (go && go.transform.childCount > 0)
        {
            //For each of its children,
            foreach (Transform t in go.transform)
            {
                if (t.gameObject.hasKey())
                {
                    //Remove it from the gameObjects list
                    gameObjects.Remove(t.gameObject.getKey());
                }
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
        List<int> keys = new List<int>(gameObjects.Keys);
        //Loop over copy list
        foreach (int key in keys)
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
        memories.Clear();
    }

    /// <summary>
    /// Update the list of GameObjects with state to save
    /// </summary>
    public void refreshGameObjects()
    {
        //Clear the list
        gameObjects.Clear();
        //Add objects that can move
        foreach (Rigidbody2D rb in FindObjectsOfType<Rigidbody2D>())
        {
            addObject(rb.gameObject);
        }
        //Add objects that have other variables that can get rewound
        foreach (SavableMonoBehaviour smb in FindObjectsOfType<SavableMonoBehaviour>())
        {
            addObject(smb.gameObject);
        }
        //Memories
        refreshMemoryObjects();
    }
    public void refreshMemoryObjects()
    {
        foreach (MemoryMonoBehaviour mmb in FindObjectsOfType<MemoryMonoBehaviour>())
        {
            int key = mmb.gameObject.getKey();
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

    #region Memory List Management
    /// <summary>
    /// Saves the memory to the memory list
    /// </summary>
    /// <param name="mmb"></param>
    public void saveMemory(MemoryMonoBehaviour mmb)
    {
        int key = mmb.gameObject.getKey();
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
            int key = mmb.gameObject.getKey();
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
        ES3.Save<Dictionary<int, MemoryObject>>(
            "memories",
            memories,
            fileName
            );
    }
    public void loadFromFile(string fileName)
    {
        //Load memories
        memories = ES3.Load<Dictionary<int, MemoryObject>>(
            "memories",
            fileName
            );
    }
}
