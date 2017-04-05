﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine.SceneManagement;


public class GameManager : MonoBehaviour
{
    public bool save = false;
    public bool load = false;
    public bool demoBuild = false;//true to not load on open and save with date/timestamp in filename
    public int chosenId = 0;
    public int amount = 0;
    public GameObject playerGhost;//this is to show Merky in the past (prefab)
    public GameObject npcTalkEffect;//the particle system for the visual part of NPC talking
    private static GameObject lastTalkingNPC;//the last NPC to talk
    private int rewindId = 0;//the id to eventually load back to
    private List<GameState> gameStates = new List<GameState>();
    private List<SceneLoader> sceneLoaders = new List<SceneLoader>();
    private List<GameObject> gameObjects = new List<GameObject>();
    public static List<Collider2D> gravityColliderList = new List<Collider2D>();
    //Memories
    private List<MemoryObject> memories = new List<MemoryObject>();
    //Checkpoints
    private List<CheckPointChecker> activeCheckPoints = new List<CheckPointChecker>();

    private static GameManager instance;
    private static GameObject playerObject;//the player object
    private CameraController camCtr;
    private float actionTime = 0;//used to determine how often to rewind
    private const float rewindDelay = 0.05f;//how much to delay each rewind transition by
    private string newlyLoadedScene = null;
    private string unloadedScene = null;

    // Use this for initialization
    void Start()
    {
        playerObject = GameObject.FindGameObjectWithTag("Player");
        foreach (GameObject go in SceneManager.GetSceneByName("SceneLoaderTriggers").GetRootGameObjects())
        {
            sceneLoaders.Add(go.GetComponent<SceneLoader>());
        }
        camCtr = FindObjectOfType<CameraController>();
        CameraController cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraController>();
        cam.pinPoint();
        cam.recenter();
        cam.refocus();
        chosenId = -1;
        if (!demoBuild && ES2.Exists("merky.txt"))
        {
            loadFromFile();
            chosenId = rewindId = gameStates.Count - 1;
            Load(chosenId);
            LoadMemories();
        }

        refreshGameObjects();
        SceneManager.sceneLoaded += sceneLoaded;
        SceneManager.sceneUnloaded += sceneUnloaded;
        FindObjectOfType<Canvas>().gameObject.AddComponent<Fader>();
    }
    public static void addObject(GameObject go)
    {
        instance.gameObjects.Add(go);
    }
    public void addAll(List<GameObject> list)
    {
        foreach (GameObject go in list)
        {
            gameObjects.Add(go);
        }
    }
    /// <summary>
    /// Destroys the given GameObject and updates lists
    /// </summary>
    /// <param name="go"></param>
    public static void destroyObject(GameObject go)
    {
        removeObject(go);
        Destroy(go);
    }
    /// <summary>
    /// Removes the given GameObject from the gameObjects list
    /// </summary>
    /// <param name="go"></param>
    public static void removeObject(GameObject go)
    {
        instance.gameObjects.Remove(go);
    }

    // Update is called once per frame
    void Update()
    {
        if (save == true)
        {
            save = false;
            Save();
        }
        if (load == true)
        {
            load = false;
            Load(chosenId);
        }
        if (isRewinding())
        {
            if (Time.time > actionTime)
            {
                actionTime = Time.time + rewindDelay;
                Load(chosenId - 1);
            }
        }
        foreach (SceneLoader sl in sceneLoaders)
        {
            sl.check();
        }
        if (newlyLoadedScene != null)
        {
            refreshGameObjects();
            LoadObjectsFromScene(SceneManager.GetSceneByName(newlyLoadedScene));
            newlyLoadedScene = null;
        }
        if (unloadedScene != null)
        {
            refreshGameObjects();
            unloadedScene = null;
        }
        if (gameStates.Count == 0)
        {
            Save();
        }
    }

    void sceneLoaded(Scene s, LoadSceneMode m)
    {
        refreshGameObjects();
        newlyLoadedScene = s.name;
    }
    void sceneUnloaded(Scene s)
    {
        refreshGameObjects();
        unloadedScene = s.name;
    }

    public void refreshGameObjects()
    {
        gameObjects = new List<GameObject>();
        gravityColliderList = new List<Collider2D>();
        foreach (Rigidbody2D rb in FindObjectsOfType<Rigidbody2D>())
        {
            gameObjects.Add(rb.gameObject);
            GameObject go = rb.gameObject;
            Collider2D coll = go.GetComponent<PolygonCollider2D>();
            if (coll != null)
            {
                gravityColliderList.Add(coll);
            }
            else {
                coll = go.GetComponent<BoxCollider2D>();
                if (coll != null)
                {
                    gravityColliderList.Add(coll);
                }
                else {
                    coll = go.GetComponent<CircleCollider2D>();
                    if (coll != null)
                    {
                        gravityColliderList.Add(coll);
                    }
                    else {
                    }
                }
            }
        }
        //Debug.Log("GM Collider List: " + gravityColliderList.Count);
        foreach (SavableMonoBehaviour smb in FindObjectsOfType<SavableMonoBehaviour>())
        {
            if (!gameObjects.Contains(smb.gameObject))
            {
                gameObjects.Add(smb.gameObject);
            }
        }
        foreach (MemoryMonoBehaviour mmb in FindObjectsOfType<MemoryMonoBehaviour>())
        {
            //load state if found, save state if not foud
            bool foundMO = false;
            foreach (MemoryObject mo in instance.memories)
            {
                if (mo.isFor(mmb))
                {
                    foundMO = true;
                    mo.loadState(mmb.gameObject);
                    break;
                }
            }
            if (!foundMO)
            {
                instance.memories.Add(mmb.getMemoryObject());
            }
        }
    }

    public void Save()
    {
        gameStates.Add(new GameState(gameObjects));
        amount++;
        chosenId++;
        rewindId++;
    }
    public static void saveMemory(MemoryMonoBehaviour mmb)
    {//2016-11-23: CODE HAZARD: mixture of static and non-static methods, will cause error if there are ever more than 1 instance of GameManager
        bool foundMO = false;
        foreach (MemoryObject mo in instance.memories)
        {
            if (mo.isFor(mmb))
            {
                foundMO = true;
                mo.saveState(mmb);
                break;
            }
        }
        if (!foundMO)
        {
            instance.memories.Add(mmb.getMemoryObject());
        }
    }
    public static void saveCheckPoint(CheckPointChecker cpc)//checkpoints have to work across levels, so they need to be saved separately
    {
        instance.activeCheckPoints.Add(cpc);
    }
    public void Load(int gamestateId)
    {
        //Destroy objects not spawned yet in the new selected state
        //chosenId is the previous current gamestate, which is in the future compared to gamestateId
        foreach (GameObject go in gameStates[chosenId].getGameObjects())
        {
            if (!gameStates[gamestateId].hasGameObject(go))
            {
                destroyObject(go);//remove it from game objects list
            }
        }
        //
        chosenId = gamestateId;
        gameStates[gamestateId].load();
        for (int i = gameStates.Count - 1; i > gamestateId; i--)
        {
            Destroy(gameStates[i].representation);
            gameStates.RemoveAt(i);
        }
        GameState.nextid = gamestateId + 1;
        //Recenter the camera
        camCtr.recenter();
        camCtr.refocus();
    }
    public void LoadObjectsFromScene(Scene s)
    {
        foreach (GameObject go in gameObjects)
        {
            if (go.scene.Equals(s))
            {
                for (int stateid = chosenId; stateid >= 0; stateid--)
                {
                    if (gameStates[stateid].loadObject(go))
                    {
                        break;
                    }
                    else
                    {
                        //continue until you find the game state that has the most recent information about this object
                    }
                }
            }
        }
    }
    public bool isRewinding()
    {
        return chosenId > rewindId;
    }
    public void cancelRewind()
    {
        rewindId = chosenId;
    }
    void Rewind(int gamestateId)//rewinds one state at a time
    {
        rewindId = gamestateId;
    }
    void LoadMemories()
    {
        foreach (MemoryObject mo in memories)
        {
            mo.loadState();
        }
    }
    public void saveToFile()
    {
        string fileName = "merky";
        if (demoBuild)
        {
            System.DateTime now = System.DateTime.Now;
            fileName += "-" + now.Ticks;
        }
        fileName += ".txt";
        ES2.Save(memories, fileName + "?tag=memories");
        ES2.Save(gameStates, fileName + "?tag=states");
    }
    public void loadFromFile()
    {
        memories = ES2.LoadList<MemoryObject>("merky.txt?tag=memories");
        gameStates = ES2.LoadList<GameState>("merky.txt?tag=states");
    }
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        SceneManager.LoadScene("SceneLoaderTriggers", LoadSceneMode.Additive);//load the SceneLoaderTriggers scene
        SceneManager.LoadScene("CheckPointScene", LoadSceneMode.Additive);//load the CheckPointScene scene
    }
    void OnApplicationQuit()
    {
        Save();
        saveToFile();
    }

    public static List<CheckPointChecker> getActiveCheckPoints()
    {
        return instance.activeCheckPoints;
    }

    public static GameObject getPlayerObject()
    {
        return playerObject;
    }

    /// <summary>
    /// Returns true if the given GameObject is touching Merky's teleport range
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public static bool isInTeleportRange(GameObject other)
    {
        float range = playerObject.GetComponent<PlayerController>().range;
        return (other.transform.position - playerObject.transform.position).sqrMagnitude <= range * range;
    }

    public void showPlayerGhosts()
    {
        foreach (GameState gs in gameStates)
        {
            gs.showRepresentation(playerGhost);
        }
    }
    public void hidePlayerGhosts()
    {
        foreach (GameState gs in gameStates)
        {
            gs.hideRepresentation();
        }
    }
    public void processTapGesture(Vector3 curMPWorld)
    {
        Debug.Log("GameManager.pTG: curMPWorld: " + curMPWorld);
        GameState final = null;
        GameState prevFinal = null;
        foreach (GameState gs in gameStates)
        {
            if (gs.checkRepresentation(curMPWorld))
            {
                if (final == null || gs.id > final.id)//assuming the later ones have higher id values
                {
                    prevFinal = final;//keep the second-to-latest one
                    final = gs;//keep the latest one
                }
            }
        }
        if (final != null)
        {
            hidePlayerGhosts();
            if (final.id == chosenId)
            {
                if (prevFinal != null)
                {//if the current one overlaps a previous one, choose the previous one
                    Rewind(prevFinal.id);
                }
                else {
                    Load(final.id);
                }
            }
            else {
                Rewind(final.id);
            }
        }
        //leave this zoom level even if no past merky was chosen
        camCtr.adjustScalePoint(-1);
    }

    /// <summary>
    /// Activates the visual effects for the given npc talking
    /// </summary>
    /// <param name="npc"></param>
    /// <param name="talking">Whether to activate or deactivate the visual effects</param>
    public static void speakNPC(GameObject npc, bool talking)
    {
        if (talking)
        {
            instance.npcTalkEffect.transform.position = npc.transform.position;
            if (!instance.npcTalkEffect.GetComponent<ParticleSystem>().isPlaying)
            {
                instance.npcTalkEffect.GetComponent<ParticleSystem>().Play();
            }
            lastTalkingNPC = npc;
        }
        else
        {
            if (npc == lastTalkingNPC)
            {
                instance.npcTalkEffect.GetComponent<ParticleSystem>().Stop();
            }
        }
    }
}


