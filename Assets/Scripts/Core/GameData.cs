using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class GameData : ICloneable
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
    public Dictionary<int, SavableObjectInfo> savables = new Dictionary<int, SavableObjectInfo>();//list of current objects that have state to save

    /// <summary>
    /// Stores the object's id and the scene id of the scene that it's in
    /// </summary>
    [ES3NonSerializable]
    public Dictionary<int, int> objectSceneList = new Dictionary<int, int>();


    public object Clone()
    {
        GameData gameData = new GameData();

        gameData.memories = this.memories.ToDictionary(entry => entry.Key, entry => entry.Value);
        gameData.knownObjects = this.knownObjects.ToList();
        gameData.gameStates = this.gameStates.ToList();

        gameData.savables = this.savables.ToDictionary(entry => entry.Key, entry => entry.Value);
        gameData.objectSceneList = this.objectSceneList.ToDictionary(entry => entry.Key, entry => entry.Value);


        return gameData;
    }

    public static implicit operator bool(GameData gd)
        => gd != null;
}
