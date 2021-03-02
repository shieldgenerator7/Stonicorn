﻿using System;
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
    [SerializeField]
    private float maxRewindDuration = 30;

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

    public void init()
    {
        //Initialize the current game state id
        //There are possibly none, so the default "current" is -1
        //Update the game state id trackers
        chosenId = rewindId = gameStates.Count - 1;
        //Load the most recent game state
        Load(chosenId);
    }

    public void processRewind()
    {
        //Assuming it's rewinding,
        //If it's time to rewind the next step,
        if (Time.unscaledTime > lastRewindTime + rewindDelay)
        {
            //Rewind to the next previous game state
            lastRewindTime = Time.unscaledTime;
            Load(chosenId - 1);
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
    public event OnGameStateSaved onGameStateSaved;
    /// <summary>
    /// Load the game state with the given id
    /// </summary>
    /// <param name="gamestateId">The ID of the game state to load</param>
    private void Load(int gamestateId)
    {
        //Update chosenId to game-state-now
        chosenId = Mathf.Clamp(gamestateId, -1, gameStates.Count - 1);
        if (chosenId < 0)
        {
            return;
        }
        GameState gameState = gameStates[gamestateId];
        //Remove null objects from the list
        Managers.Object.cleanObjects();
        //Destroy objects not spawned yet in the new selected state
        Managers.Object.GameObjects
            .FindAll(go => go.GetComponent<SavableObjectInfo>().spawnStateId > gamestateId)
            .ForEach(go => Managers.Object.destroyAndForgetObject(go));
        //Actually load the game state
        gameState.load();

        //Destroy game states in game-state-future
        for (int i = gameStates.Count - 1; i > gamestateId; i--)
        {
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
        onRewindState?.Invoke(gameStates, chosenId);
    }

    /// <summary>
    /// Slightly more efficient than calling LoadObject() on individual objects
    /// </summary>
    /// <param name="goList"></param>
    /// <param name="lastStateSeen"></param>
    public void LoadObjects(List<GameObject> goList, int lastStateSeen)
    {
        foreach (GameObject go in goList)
        {
            //Search through the game states to see when it was last saved
            for (int stateid = lastStateSeen; stateid >= 0; stateid--)
            {
                //If the game object was last saved in this game state,
                if (gameStates[stateid].hasGameObject(go))
                {
                    gameStates[stateid].loadObject(go);
                    //Great! It's loaded,
                    //Let's move onto the next object
                    break;
                }
                //Else,
                else
                {
                    //Continue until you find the game state 
                    //that has the most recent information about this object
                }
            }
        }
    }

    public void LoadObjectAndChildren(GameObject go, int lastStateSeen)
    {
        LoadObject(go, lastStateSeen);
        foreach (Transform t in go.transform)
        {
            if (t.gameObject.isSavable())
            {
                LoadObject(t.gameObject, lastStateSeen);
            }
        }
    }

    /// <summary>
    /// Load an object's most recent state, 
    /// with lastStateSeen as a hint as to which state has the most recent state
    /// </summary>
    /// <param name="go"></param>
    /// <param name="lastStateSeen"></param>
    public void LoadObject(GameObject go, int lastStateSeen = -1)
    {
        if (lastStateSeen < 0)
        {
            lastStateSeen = gameStates.Count - 1;
        }
        //Search through the game states to see when it was last saved
        for (int stateid = lastStateSeen; stateid >= 0; stateid--)
        {
            //If the game object was last saved in this game state,
            if (gameStates[stateid].hasGameObject(go))
            {
                gameStates[stateid].loadObject(go);
                //Great! It's loaded
                break;
            }
            //Else,
            else
            {
                //Continue until you find the game state 
                //that has the most recent information about this object
            }
        }
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
        //If already at the state you want to rewind to,
        if (gamestateId == chosenId)
        {
            //Just load it
            onRewindStarted?.Invoke(gameStates, gamestateId);
            Load(gamestateId);
            onRewindFinished?.Invoke(gameStates, gamestateId);
            //And don't actually rewind
            return;
        }
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
                if (count * rewindDelay > maxRewindDuration)
                {
                    rewindDelay = maxRewindDuration / count;
                }
                //Rewind Started Delegate
                onRewindStarted?.Invoke(gameStates, rewindId);
            }
            //Stop rewinding
            else
            {
                //Set rewindId to chosenId
                rewindId = chosenId;
                //Rewind Finished Delegate
                onRewindFinished?.Invoke(gameStates, chosenId);
            }
        }
    }
    public delegate void OnRewind(List<GameState> gameStates, int gameStateId);
    public event OnRewind onRewindStarted;
    public event OnRewind onRewindFinished;
    public event OnRewind onRewindState;
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
