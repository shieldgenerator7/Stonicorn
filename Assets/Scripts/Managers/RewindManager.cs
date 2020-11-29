using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewindManager : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField]
    private float baseRewindDelay = 0.05f;
    [SerializeField]
    private float rewindDelay = 0.05f;//the delay between rewind transitions
    [SerializeField]
    private float minRewindDuration = 1;//how many seconds a rewind should last for

    //Runtime vars
    private int rewindId;//the id to eventually load back to
    private int chosenId;//the id of the current game state
    private float lastRewindTime;//the last time the game rewound
    public int GameStateId => chosenId;

    public bool rewindInterruptableByPlayer { get; private set; } = true;

    //Game States
    private List<GameState> gameStates = new List<GameState>();//basically a timeline
    public int GameStateCount => gameStates.Count;
    private Dictionary<string, GameObject> gameObjects = new Dictionary<string, GameObject>();//list of current objects that have state to save
    public int GameObjectCount => gameObjects.Values.Count;
    private List<GameObject> forgottenObjects = new List<GameObject>();//a list of objects that are inactive and thus unfindable, but still have state to save
    public List<GameObject> ForgottenObjects => forgottenObjects;

    //Memories
    private Dictionary<string, MemoryObject> memories = new Dictionary<string, MemoryObject>();//memories that once turned on, don't get turned off

    // Use this for initialization
    void Start()
    {
        init();
    }

    public void init()
    {
        //Update the list of objects that have state to save
        refreshGameObjects();
        //Initialize the current game state id
        //There are possibly none, so the default "current" is -1
        //Update the game state id trackers
        chosenId = rewindId = gameStates.Count - 1;
        //Load the most recent game state
        Load(chosenId);
    }

    void Update()
    {
        //If the time is rewinding,
        if (Rewinding)
        {
            //And it's time to rewind the next step,
            if (Time.unscaledTime > lastRewindTime + rewindDelay)
            {
                //Rewind to the next previous game state
                lastRewindTime = Time.unscaledTime;
                Load(chosenId - 1);
            }
        }
    }

    public void saveToFile(string fileName)
    {
        ES3.Save<Dictionary<string, MemoryObject>>(
            "memories",
            memories,
            fileName
            );
        //Save game states
        ES3.Save<List<GameState>>(
            "states",
            gameStates,
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
        //Load game states
        gameStates = ES3.Load<List<GameState>>(
            "states",
            fileName
            );
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

        //getKey() returns a string containing
        //the object's name and scene name
        string key = go.getKey();

        //If the game object's name is already in the dictionary...
        if (gameObjects.ContainsKey(key))
        {
            throw new System.ArgumentException(
                  "GameObject (" + key + ") is already inside the gameObjects dictionary! "
                  + "Check for 2 or more objects with the same name."
                  );
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
    private void cleanObjects()
    {
        string cleanedKeys = "";
        //Copy the game object keys
        List<string> keys = new List<string>(gameObjects.Keys);
        //Loop over copy list
        foreach (string key in keys)
        {
            //If the key's value is null,
            if (gameObjects[key] == null)
            {
                //Clean the key out
                cleanedKeys += key + ", ";
                gameObjects.Remove(key);
            }
        }
        //Write out to the console which keys were cleaned
        if (cleanedKeys != "")
        {
            Debug.LogError("Cleaned: " + cleanedKeys);
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

    /// <summary>
    /// Saves the current game state
    /// </summary>
    public void Save()
    {
        //Remove any null objects from the list
        cleanObjects();
        //Create a new game state
        gameStates.Add(new GameState(gameObjects.Values));
        //Update game state id variables
        chosenId++;
        rewindId++;
        //Save delegate
        onGameStateSaved?.Invoke(chosenId);
    }
    public delegate void OnGameStateSaved(int gameStateId);
    public OnGameStateSaved onGameStateSaved;
    /// <summary>
    /// Load the game state with the given id
    /// </summary>
    /// <param name="gamestateId">The ID of the game state to load</param>
    public void Load(int gamestateId)
    {
        //Update chosenId to game-state-now
        chosenId = Utility.clamp(gamestateId, 0, gameStates.Count);
        //Remove null objects from the list
        cleanObjects();
        //Destroy objects not spawned yet in the new selected state
        List<GameObject> destroyObjectList = new List<GameObject>();
        foreach (GameObject go in gameObjects.Values)
        {
            foreach (SavableMonoBehaviour smb in go.GetComponents<SavableMonoBehaviour>())
            {
                //If the game object was spawned during run time
                //(versus pre-placed at edit time)
                if (smb.IsSpawnedObject)
                {
                    //And if the game object is not in the game state,
                    if (!gameStates[gamestateId].hasGameObject(go))
                    {
                        //remove it from game objects list
                        //by adding it to the list of game objects to be destroyed
                        destroyObjectList.Add(go);
                    }
                }
            }
        }
        //Actually destroy the objects that need destroyed
        for (int i = destroyObjectList.Count - 1; i >= 0; i--)
        {
            //Work around to avoid deleting objects from a list that's being iterated over
            destroyObject(destroyObjectList[i]);
        }
        //Actually load the game state
        gameStates[gamestateId].load();

        //Destroy game states in game-state-future
        for (int i = gameStates.Count - 1; i > gamestateId; i--)
        {
            Destroy(gameStates[i].Representation);
            gameStates.RemoveAt(i);
        }
        //Update the next game state id
        GameState.nextid = gamestateId + 1;

        //If the rewind is finished,
        if (chosenId == rewindId)
        {
            //Stop the rewind
            Rewinding = false;
        }
    }

    public void LoadObjects(string sceneName, int lastStateSeen, Predicate<GameObject> filter)
    {
        int newObjectsFound = 0;
        int objectsLoaded = 0;
        foreach (GameObject go in gameObjects.Values)
        {
            if (filter(go))
            {
                //Search through the game states to see when it was last saved
                for (int stateid = lastStateSeen; stateid >= 0; stateid--)
                {
                    //If the game object was last saved in this game state,
                    if (gameStates[stateid].loadObject(go))
                    {
                        //Great! It's loaded,
                        //Let's move onto the next object
                        objectsLoaded++;
                        break;
                    }
                    //Else,
                    else
                    {
                        //Continue until you find the game state that has the most recent information about this object
                    }
                }
            }
        }
#if UNITY_EDITOR
        Logger.log(this, "LOFS: Scene " + sceneName + ": objects found: " + newObjectsFound + ", objects loaded: " + objectsLoaded);
#endif
    }

    /// <summary>
    /// Rewinds back a number of states equal to count
    /// </summary>
    /// <param name="count">How many states to rewind. 0 doesn't rewind. 1 undoes 1 state</param>
    public void Rewind(int count)
    {
        RewindTo(chosenId - count, false);
    }

    /// <summary>
    /// Sets into motion the rewind state.
    /// Update carries out the motions of calling Load()
    /// </summary>
    /// <param name="gamestateId">The game state id to rewind to</param>
    void RewindTo(int gamestateId, bool playerInitiated = true)
    {
        //Set interruptable
        rewindInterruptableByPlayer = playerInitiated;
        //Set the game state tracker vars
        rewindId = Mathf.Max(0, gamestateId);
        //Start the rewind
        Rewinding = true;
    }
    /// <summary>
    /// Rewind the game all the way to the beginning
    /// </summary>
    public void RewindToStart(bool playerInitiated = false)
    {
        RewindTo(0, playerInitiated);
    }
    /// <summary>
    /// True if time is rewinding
    /// </summary>
    public bool Rewinding
    {
        get { return chosenId > rewindId; }
        private set
        {
            //Start rewinding
            if (value)
            {
                //Make sure rewind variable is set correctly
                if (rewindId == chosenId)
                {
                    //If it has not been,
                    //rewind to start
                    rewindId = 0;
                }
                //Set the music speed to rewind
                Managers.Music.SongSpeed = Managers.Music.rewindSongSpeed;
                //Show rewind visual effect
                Managers.Effect.showRewindEffect(true);
                //Set rewindDelay
                int count = chosenId - rewindId;
                rewindDelay = baseRewindDelay;
                if (count * rewindDelay < minRewindDuration)
                {
                    rewindDelay = minRewindDuration / count;
                }
                //Recenter the camera on Merky
                Managers.Camera.recenter();
                //Disable physics while rewinding
                Managers.Physics2DSurrogate.enabled = true;
                //Pause time
                Managers.Time.setPause(this, true);
                //Update Stats
                GameStatistics.addOne("Rewind");
                //Rewind Started Delegate
                onRewindFinished?.Invoke(gameStates, rewindId);
            }
            //Stop rewinding
            else
            {
                //Set rewindId to chosenId
                rewindId = chosenId;
                //Refresh the game object list
                refreshGameObjects();
                //Put the music back to normal
                Managers.Music.SongSpeed = Managers.Music.normalSongSpeed;
                //Stop rewind visual effect
                Managers.Effect.showRewindEffect(false);
                //Unpause time
                Managers.Time.setPause(this, false);
                //Re-enable physics because the rewind is over
                Managers.Physics2DSurrogate.enabled = false;
                //Rewind Finished Delegate
                onRewindFinished?.Invoke(gameStates, chosenId);
            }
        }
    }
    public delegate void OnRewind(List<GameState> gameStates, int gameStateId);
    public OnRewind onRewindStarted;
    public OnRewind onRewindFinished;
    /// <summary>
    /// Ends the rewind at the current game state
    /// </summary>
    public void cancelRewind()
    {
        //Stop the rewind
        Rewinding = false;
        //Load the current game state
        Load(chosenId);
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

}
