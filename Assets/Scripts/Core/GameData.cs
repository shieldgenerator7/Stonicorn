using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameData
{

    //Memories
    public Dictionary<int, MemoryObject> memories = new Dictionary<int, MemoryObject>();//memories that once turned on, don't get turned off

    [ES3NonSerializable]
    public Dictionary<int, GameObject> gameObjects = new Dictionary<int, GameObject>();//list of current objects that have state to save

    public List<SavableObjectInfoData> knownObjects;

    //Game States
    public List<GameState> gameStates = new List<GameState>();//basically a timeline

    /// <summary>
    /// Stores the object's id and the scene id of the scene that it's in
    /// </summary>
    public Dictionary<int, int> objectSceneList = new Dictionary<int, int>();
}
