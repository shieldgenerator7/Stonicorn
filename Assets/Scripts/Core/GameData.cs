using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GameData
{
    //
    // Savables
    //
    public Dictionary<int, MemoryObject> memories = new Dictionary<int, MemoryObject>();//memories that once turned on, don't get turned off

    public List<SavableObjectInfoData> knownObjects;

    public List<GameState> gameStates = new List<GameState>();//basically a timeline

    //
    // Runtime Vars
    //
    [ES3NonSerializable]
    public Dictionary<int, GameObject> gameObjects = new Dictionary<int, GameObject>();//list of current objects that have state to save

    /// <summary>
    /// Stores the object's id and the scene id of the scene that it's in
    /// </summary>
    [ES3NonSerializable]
    public Dictionary<int, int> objectSceneList = new Dictionary<int, int>();

    public static implicit operator bool(GameData gd)
        => gd != null;
}
