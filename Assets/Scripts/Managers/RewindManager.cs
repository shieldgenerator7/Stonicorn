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
    public List<GameState> GameStates => gameStates;
    public int GameStateCount => gameStates.Count;

    // Use this for initialization
    void Start()
    {
        init();
    }

    public void init()
    {
        //Update the list of objects that have state to save
        Managers.Object.refreshGameObjects();
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
        //Save game states
        ES3.Save<List<GameState>>(
            "states",
            gameStates,
            fileName
            );
    }
    public void loadFromFile(string fileName)
    {
        //Load game states
        gameStates = ES3.Load<List<GameState>>(
            "states",
            fileName
            );
    }

    /// <summary>
    /// Saves the current game state
    /// </summary>
    public void Save()
    {
        //Remove any null objects from the list
        Managers.Object.cleanObjects();
        //Create a new game state
        gameStates.Add(new GameState(Managers.Object.GameObjects));
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
        chosenId = Mathf.Clamp(gamestateId, -1, gameStates.Count - 1);
        if (chosenId < 0)
        {
            return;
        }
        //Remove null objects from the list
        Managers.Object.cleanObjects();
        //Destroy objects not spawned yet in the new selected state
        List<GameObject> destroyObjectList = new List<GameObject>();
        foreach (GameObject go in Managers.Object.GameObjects)
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
            Managers.Object.destroyObject(destroyObjectList[i]);
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
        foreach (GameObject go in Managers.Object.GameObjects)
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
    public void RewindTo(int gamestateId, bool playerInitiated = true)
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
        get => chosenId > rewindId;
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
                //Set rewindDelay
                int count = chosenId - rewindId;
                rewindDelay = baseRewindDelay;
                if (count * rewindDelay < minRewindDuration)
                {
                    rewindDelay = minRewindDuration / count;
                }
                //Rewind Started Delegate
                onRewindStarted?.Invoke(gameStates, rewindId);
            }
            //Stop rewinding
            else
            {
                //Set rewindId to chosenId
                rewindId = chosenId;
                //Refresh the game object list
                Managers.Object.refreshGameObjects();
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

}
