using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class ObjectManager : Manager, ISetting
{
    //Create queue
    private Dictionary<int, AsyncOperationHandle<GameObject>> recreateQueue = new Dictionary<int, AsyncOperationHandle<GameObject>>();


    public void LoadSceneObjects(List<SavableObjectInfo> sceneGOs, List<int> foreignIds, int lastStateSeen)
    {
        sceneGOs.ForEach(soi => addObject(soi));
        foreignIds.FindAll(id => !hasObject(id))
            .ForEach(id => recreateObject(id, lastStateSeen));
    }
    public void LoadObjectsPostRewind(int gameStateId)
    {
        Debug.Log($"Checking objects after rewinding to state {gameStateId}");
        //Remove null objects from the list
        cleanObjects();
        //Destroy objects not spawned yet in the new selected state
        data.knownObjects
            .Where(soid => soid.spawnStateId != 0 && soid.spawnStateId >= gameStateId).ToList()
            .ForEach(soid => destroyAndForgetObject(soid.id));
    }

    public void recreateObject(int goId, int lastStateSeen = -1)
    {
        if (goId > 0)
        {
            string prefabGUID = data.knownObjects.Find(soid => soid.id == goId).prefabGUID;
            Debug.Log($"Recreating goId: ({goId})"
#if UNITY_EDITOR
                + $" using prefab {AssetDatabase.GUIDToAssetPath(prefabGUID)}"
#endif
                );
            recreateObject(goId, prefabGUID, lastStateSeen);
        }
        else
        {
            throw new ArgumentException($"Id must be 0 or greater! id: ({goId})");
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
    private void /*AsyncOperationHandle<GameObject>*/ recreateObject(int goId, string prefabGUID, int lastStateSeen)
    {
        if (!recreateQueue.ContainsKey(goId))
        {
            try
            {
                AssetReference assetRef = new AssetReference(prefabGUID);
                if (assetRef == null)
                {
                    Debug.LogError($"Asset Ref is null! id: {goId}, prefab: {prefabGUID}");
                    return;
                }
#if UNITY_EDITOR
                if (assetRef.editorAsset == null)
                {
                    Debug.LogError($"Asset Ref editor asset is null! id: {goId}, prefab: {prefabGUID}");
                    EditorApplication.isPaused = true;
                    return;
                }
#endif
                //2020-12-23: copied from https://youtu.be/uNpBS0LPhaU?t=1000
                var op = Addressables.InstantiateAsync(assetRef);
                recreateQueue.Add(goId, op);
                Debug.Log($"Recreating object ({goId})"
                    + $" using prefabGUID {prefabGUID},"
#if UNITY_EDITOR
                    + $" using prefab {assetRef.editorAsset.name}"
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
                    SavableObjectInfo soi = newGO.GetComponent<SavableObjectInfo>();
                    SavableObjectInfoData soid = data.knownObjects.Find(soid => soid.id == goId);
                    soi.Data = soid;
                    addObject(soi);
                    //TODO: use SOI childrenSOI list
                    foreach (Transform t in newGO.transform)
                    {
                        SavableObjectInfo soiT = t.gameObject.GetComponent<SavableObjectInfo>();
                        if (soiT)
                        {
                            SavableObjectInfoData soidT = data.knownObjects.Find(soid => soid.id == soiT.Id);
                            soiT.Data = soidT;
                            addObject(soiT);
                        }
                    }
                    Debug.Log($"Recreated object {newGO.Name()}: spawned: {soi.spawnStateId}, destroyed: {soi.destroyStateId}");
                    //Delegate
                    onObjectRecreated?.Invoke(soi, lastStateSeen);
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
                throw new Exception($"InvalidKey: ({prefabGUID}) for object ({goId}):", ike);
            }
            catch (Exception ike)
            {
                throw new Exception($"InvalidKey: ({prefabGUID}) for object ({goId}):", ike);
            }
        }
        //return createQueue[goId];
    }
    public delegate void OnObjectRecreated(SavableObjectInfo soi, int lastStateSeen);
    public event OnObjectRecreated onObjectRecreated;
    public delegate void OnAllObjectsRecreated();
    public event OnAllObjectsRecreated onAllObjectsRecreated;

    public bool RecreatingObjects => recreateQueue.Count > 0;


    /// <summary>
    /// Instantiates a GameObject so that it can be rewound.
    /// Only works on game objects that are "registered" to be rewound
    /// </summary>
    /// <param name="prefab"></param>
    /// <returns></returns>
    public GameObject Instantiate(GameObject prefab)
    {
        return Instantiate(prefab, Vector2.zero);
    }
    public GameObject Instantiate(GameObject prefab, Vector2 position)
    {
        //Checks to make sure it's rewindable
        bool isContainer = prefab.containsSavables();
        bool isSavable = prefab.isSavable();
        if (!isContainer)
        {
            if (!isSavable)
            {
                throw new UnityException($"Prefab {prefab.name} cannot be instantiated as a rewindable object because it does not have a RigidBody2D or a SavableMonoBehaviour.");
            }
            bool hasInfo = prefab.GetComponent<SavableObjectInfo>();
            if (!hasInfo)
            {
                throw new UnityException($"Prefab {prefab.name} cannot be instantiated as a rewindable object because it does not have an SavableObjectInfo.");
            }
        }
        //Instantiate
        GameObject newObj = GameObject.Instantiate(prefab, position, Quaternion.identity);
        int baseId = (int)System.DateTime.Now.Ticks;
        string spawnTag = $"---{baseId}";
        newObj.name += spawnTag;
        int id = -1;
        if (isSavable || isContainer)
        {
            id = data.claimNextId();
        }
        if (isSavable)
        {
            SavableObjectInfo soi = newObj.GetComponent<SavableObjectInfo>();
            soi.Id = data.claimNextId();
            soi.spawnStateId = Managers.Rewind.GameStateId;
            addNewObject(soi);
            Managers.Scene.registerObjectInScene(soi);
            Debug.Log($"Spawned object {soi.TextLine}", newObj);
        }
        //Container children
        if (isContainer)
        {
            ISavableContainer container = newObj.GetComponent<ISavableContainer>();
            int nextId = id;
            container.Savables.ForEach(savable =>
            {
                savable.name += spawnTag;
                savable.Id = nextId;
                savable.spawnStateId = Managers.Rewind.GameStateId;
                nextId++;
                addNewObject(savable);
                Managers.Scene.registerObjectInScene(savable);
            });
            Debug.Log($"Spawned container {newObj.name}", newObj);
        }
        //Return spawned object
        return newObj;
    }


    public override SettingObject Setting
    {
        get => new SettingObject(ID).addList(
            "knownObjects", data.knownObjects
            );
        set => data.knownObjects = value.List<SavableObjectInfoData>("knownObjects");
    }

    public override SettingScope Scope => SettingScope.SAVE_FILE;

    public override string ID => "ObjectManager";

    /// <summary>
    /// Adds a newly created object to the list
    /// </summary>
    /// <param name="soi"></param>
    public void addNewObject(SavableObjectInfo soi)
    {
        SavableObjectInfoData soid = soi.Data;
        if (!data.knownObjects.Contains(soid))
        {
            data.knownObjects.Add(soid);
        }
        addObject(soi);
    }

    /// <summary>
    /// Adds an object to list of objects that have state to save
    /// </summary>
    /// <param name="go">The GameObject to add to the list</param>
    public void addObject(SavableObjectInfo soi)
    {
        //
        //Error checking
        //


        int key = soi.Id;

        //If the key is invalid,
        if (key < 0)
        {
            Debug.LogError(
                $"GameObject {soi.TextLine} has an invalid key: {key}!",
                soi
                );
            return;
        }

        //If the game object's name is already in the dictionary,
        if (data.savables.ContainsKey(key))
        {
            if (data.savables[key] != null && soi.name != data.savables[key].name)
            {
                Debug.LogWarning(
                      $"Key ({key}) is already inside the gameObjects dictionary: "
                      + $"GameObject {soi.TextLine} replacing {data.savables[key].TextLine}",
                      soi
                      );
            }
            data.savables[key] = soi;
        }
        else
        {
            //Else if all good, add the object
            data.savables.Add(key, soi);
        }
    }

    public bool hasObject(int goKey)
        => data.savables.ContainsKey(goKey) && data.savables[goKey] != null;

    /// <summary>
    /// Retrieves the GameObject from the gameObjects list with the given key
    /// </summary>
    /// <param name="goKey">The unique inter-scene key of the object</param>
    /// <returns></returns>
    public SavableObjectInfo getObject(int goKey)
    {
        //If the gameObjects list has the game object,
        if (data.savables.ContainsKey(goKey))
        {
            //Return it
            return data.savables[goKey];
        }
        Debug.LogError(
            $"No object with key found: {goKey}!\n"
            + "Check with hasObject() before getting the object."
            );
        //Otherwise, sorry, you're out of luck
        return null;
    }

    internal List<T> getObjects<T>()
    {
        return data.savables
            .Values.ToList()
            .ConvertAll(value => value.GetComponent<T>())
            .FindAll(t => t != null);
    }

    /// <summary>
    /// Destroys the object and forgets it so that it cannot be recreated.
    /// If you want to destroy an object through normal means,
    /// use destroyObject() instead.
    /// </summary>
    /// <param name="go"></param>
    public void destroyAndForgetObject(SavableObjectInfo soi)
    {
        if (soi is SingletonObjectInfo)
        {
            //don't destroy the game manager or merky
            return;
        }
        Debug.Log($"Destroying object permanently: {soi.TextLine}", soi);
        destroyObject(soi);
        forgetObject(soi.Id);
    }

    /// <summary>
    /// Destroys the object and forgets it so that it cannot be recreated.
    /// If you want to destroy an object through normal means,
    /// use destroyObject() instead.
    /// </summary>
    /// <param name="go"></param>
    public void destroyAndForgetObject(int id)
    {
        if (hasObject(id))
        {
            SavableObjectInfo soi = getObject(id);
            if (soi is SingletonObjectInfo)
            {
                //don't destroy the game manager or merky
                return;
            }
            Debug.Log($"Destroying object permanently: {soi.TextLine}", soi);
            destroyObject(soi);
        }
        else
        {
            Debug.Log($"Destroying object permanently: [unknown name] ({id})");
        }
        forgetObject(id);
    }

    private void forgetObject(int id)
    {
        data.knownObjects.RemoveAll(soid => soid.id == id);
        data.savables.Remove(id);
        data.objectSceneList.Remove(id);
    }

    public void destroyObject(int goKey)
    {
        SavableObjectInfo soi = getObject(goKey);
        if (soi)
        {
            destroyObject(soi);
        }
    }

    /// <summary>
    /// Destroys the given GameObject and updates lists
    /// </summary>
    /// <param name="soi">The GameObject to destroy</param>
    public void destroyObject(SavableObjectInfo soi)
    {
        if (soi is SingletonObjectInfo)
        {
            //don't destroy the game manager or merky
            return;
        }
        Debug.Log($"Destroying object {soi.TextLine}");
        int gameStateId = Managers.Rewind.GameStateId;
        if (soi.destroyStateId > gameStateId)
        {
            soi.destroyStateId = gameStateId;
            updateDestroyStateId(soi.Id, gameStateId);
        }
        removeObject(soi);
        Destroy(soi.gameObject);
    }

    /// <summary>
    /// Removes the given GameObject from the gameObjects list
    /// </summary>
    /// <param name="soi">The GameObject to remove from the list</param>
    private void removeObject(SavableObjectInfo soi)
    {
        data.savables.Remove(soi.Id);
        //TODO: use SOI childrenSOI list
        //If go is not null and has children,
        if (soi && soi.transform.childCount > 0)
        {
            //For each of its children,
            foreach (Transform t in soi.transform)
            {
                if (t.gameObject.hasKey())
                {
                    //Remove it from the gameObjects list
                    data.savables.Remove(t.gameObject.getKey());
                }
            }
        }
    }

    public void updateDestroyStateId(int soiId, int stateId)
    {
        SavableObjectInfoData soid = data.knownObjects
            .Find(ksoid => ksoid.id == soiId);
        soid.destroyStateId = stateId;
    }

    /// <summary>
    /// Remove null objects from the gameObjects list
    /// </summary>
    public void cleanObjects()
    {
        string cleanedKeys = "";
        //Copy the game object keys
        List<int> keys = new List<int>(data.savables.Keys);
        //Loop over copy list
        foreach (int key in keys)
        {
            //If the key's value is null,
            if (data.savables[key] == null
                || ReferenceEquals(data.savables[key], null))
            {
                //Clean the key out
                cleanedKeys += key + ", ";
                data.savables.Remove(key);
            }
        }
        //Write out to the console which keys were cleaned
        if (cleanedKeys != "")
        {
            Debug.LogWarning($"Cleaned: {cleanedKeys}");
        }
    }

    /// <summary>
    /// Clear all objects from the list
    /// </summary>
    public void clearObjects()
    {
        data.savables.Clear();
        data.memories.Clear();
    }

    /// <summary>
    /// Update the list of GameObjects with state to save
    /// </summary>
    public void refreshGameObjects()
    {
        //Clear the list
        data.savables.Clear();
        //Add objects that have other variables that can get rewound
        foreach (SavableObjectInfo soi in FindObjectsByType<SavableObjectInfo>(FindObjectsSortMode.None))
        {
            addObject(soi);
        }
        //Memories
        refreshMemoryObjects();
    }
    public void refreshMemoryObjects()
    {
        //TODO: search for MemoryObjectInfo instead
        foreach (MemoryMonoBehaviour mmb in FindObjectsByType<MemoryMonoBehaviour>(FindObjectsSortMode.None))
        {
            int key = mmb.gameObject.getKey();
            //If the memory has already been stored,
            if (data.memories.ContainsKey(key))
            {
                //Load the memory
                mmb.acceptMemoryObject(data.memories[key]);
            }
            //Else
            else
            {
                //Save the memory
                data.memories.Add(key, mmb.getMemoryObject());
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
        if (data.memories.ContainsKey(key))
        {
            //Update it
            data.memories[key] = mo;
        }
        //Else
        else
        {
            //Add it
            data.memories.Add(key, mo);
        }
    }
    /// <summary>
    /// Restore all saved memories of game objects that have a memory saved
    /// </summary>
    public void LoadMemories()
    {
        //Find all the game objects that can have memories
        foreach (MemoryMonoBehaviour mmb in FindObjectsByType<MemoryMonoBehaviour>(FindObjectsSortMode.None))
        {
            int key = mmb.gameObject.getKey();
            //If there's a memory saved for this object,
            if (data.memories.ContainsKey(key))
            {
                //Tell that object what it is
                mmb.acceptMemoryObject(data.memories[key]);
            }
        }
    }
    #endregion
}
