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
    public GameObject playerGhost;//this is to show Merky in the past (prefab)
    public GameObject npcTalkEffect;//the particle system for the visual part of NPC talking
    public AudioSource timeRewindMusic;//the music to play while time rewinds
    private static GameObject lastTalkingNPC;//the last NPC to talk
    private int rewindId = 0;//the id to eventually load back to
    private float respawnTime = 0;//the earliest time Merky can rewind after shattering
    public float respawnDelay = 1.0f;//how long Merky must wait before rewinding after shattering
    private List<GameState> gameStates = new List<GameState>();
    private List<SceneLoader> sceneLoaders = new List<SceneLoader>();
    private List<GameObject> gameObjects = new List<GameObject>();
    //Memories
    private List<MemoryObject> memories = new List<MemoryObject>();
    //Checkpoints
    private List<CheckPointChecker> activeCheckPoints = new List<CheckPointChecker>();

    private static GameManager instance;
    private static GameObject playerObject;//the player object
    private CameraController camCtr;
    private GestureManager gestureManager;
    private MusicManager musicManager;
    private float actionTime = 0;//used to determine how often to rewind
    private const float rewindDelay = 0.05f;//how much to delay each rewind transition by
    private List<string> newlyLoadedScenes = new List<string>();
    private int loadedSceneCount = 0;
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
        camCtr.pinPoint();
        camCtr.recenter();
        camCtr.refocus();
        gestureManager = FindObjectOfType<GestureManager>();
        musicManager = FindObjectOfType<MusicManager>();
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

    /// <summary>
    /// Resets the game back to the very beginning
    /// </summary>
    public static void newGame()
    {
        //Save previous game
        Save();
        instance.saveToFile();
        //Unload all scenes and reload PlayerScene
        instance = null;
        GameState.nextid = 0;
        SceneManager.LoadScene("PlayerScene");

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
        if (go && go.transform.childCount > 0)
        {
            foreach (Transform t in go.transform)
            {
                instance.gameObjects.Remove(t.gameObject);
            }
        }
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

        foreach (SceneLoader sl in sceneLoaders)
        {
            sl.check();
        }
        if (newlyLoadedScenes.Count > 0)
        {
            refreshGameObjects();
            foreach (string s in newlyLoadedScenes)
            {
                LoadObjectsFromScene(SceneManager.GetSceneByName(s));
                loadedSceneCount++;
            }
            newlyLoadedScenes.Clear();
        }
        if (unloadedScene != null)
        {
            refreshGameObjects();
            unloadedScene = null;
        }
        if (gameStates.Count == 0 && loadedSceneCount > 0)
        {
            Save();
        }
    }

    private void FixedUpdate()
    {
        if (isRewinding())
        {
            if (Time.time > actionTime)
            {
                actionTime = Time.time + rewindDelay;
                Load(chosenId - 1);
            }
        }
    }

    void sceneLoaded(Scene s, LoadSceneMode m)
    {
        refreshGameObjects();
        newlyLoadedScenes.Add(s.name);
    }
    void sceneUnloaded(Scene s)
    {
        refreshGameObjects();
        unloadedScene = s.name;
        loadedSceneCount--;
    }
    public static void refresh() { instance.refreshGameObjects(); }
    public void refreshGameObjects()
    {
        gameObjects = new List<GameObject>();
        foreach (Rigidbody2D rb in FindObjectsOfType<Rigidbody2D>())
        {
            gameObjects.Add(rb.gameObject);
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
                    mmb.acceptMemoryObject(mo);
                    break;
                }
            }
            if (!foundMO)
            {
                instance.memories.Add(mmb.getMemoryObject());
            }
        }
    }
    public static void Save()
    {
        instance.gameStates.Add(new GameState(instance.gameObjects));
        instance.chosenId++;
        instance.rewindId++;
    }
    public static void saveMemory(MemoryMonoBehaviour mmb)
    {//2016-11-23: CODE HAZARD: mixture of static and non-static methods, will cause error if there are ever more than 1 instance of GameManager
        bool foundMO = false;
        foreach (MemoryObject mo in instance.memories)
        {
            if (mo.isFor(mmb))
            {
                foundMO = true;
                mo.found = mmb.getMemoryObject().found;//2017-04-11: TODO: refactor this so it's more flexible
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
        for (int i = gameObjects.Count - 1; i > 0; i--)
        {
            GameObject go = gameObjects[i];
            if (!gameStates[gamestateId].hasGameObject(go))
            {
                if (go == null)
                {
                    destroyObject(go);
                    continue;
                }
                foreach (SavableMonoBehaviour smb in go.GetComponents<SavableMonoBehaviour>())
                {
                    if (smb.isSpawnedObject())
                    {
                        destroyObject(go);//remove it from game objects list
                    }
                }
            }
        }
        //
        chosenId = gamestateId;
        if (chosenId == rewindId)
        {
            //After rewind is finished, refresh the game object list
            refreshGameObjects();
            musicManager.endEventSong(timeRewindMusic);
        }
        gameStates[gamestateId].load();
        if (chosenId == rewindId)
        {
            refreshGameObjects();//a second time, just to be sure
        }
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
        musicManager.endEventSong(timeRewindMusic);
    }
    void Rewind(int gamestateId)//rewinds one state at a time
    {
        musicManager.setEventSong(timeRewindMusic);
        rewindId = gamestateId;
    }
    void LoadMemories()
    {
        foreach (MemoryObject mo in memories)
        {
            GameObject go = mo.findGameObject();
            if (go != null)
            {
                MemoryMonoBehaviour mmb = go.GetComponent<MemoryMonoBehaviour>();
                if (mo.isFor(mmb))
                {
                    mmb.acceptMemoryObject(mo);
                }
            }
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

    /// <summary>
    /// Called when Merky gets shattered
    /// </summary>
    public static void playerShattered()
    {
        instance.respawnTime = Time.time + instance.respawnDelay;
    }

    public static void showPlayerGhosts()
    {
        foreach (GameState gs in instance.gameStates)
        {
            if (gs.id != instance.chosenId || playerObject.GetComponent<PlayerController>().isIntact())
            {//don't include last game state if merky is shattered
                gs.showRepresentation(instance.playerGhost, instance.chosenId);
            }
        }
    }
    public void hidePlayerGhosts()
    {
        foreach (GameState gs in gameStates)
        {
            if (gs.id != instance.chosenId || playerObject.GetComponent<PlayerController>().isIntact())
            {//don't include last game state if merky is shattered
                gs.hideRepresentation();
            }
        }
    }
    /// <summary>
    /// Returns the player ghost that is closest to the given position
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    public static GameObject getClosestPlayerGhost(Vector2 pos)
    {
        float closestDistance = float.MaxValue;
        GameObject closestObject = null;
        foreach (GameState gs in instance.gameStates)
        {
            Vector2 gsPos = gs.representation.transform.position;
            float gsDistance = Vector2.Distance(gsPos, pos);
            if (gsDistance < closestDistance)
            {
                closestDistance = gsDistance;
                closestObject = gs.representation;
            }
        }
        return closestObject;
    }

    public void processTapGesture(Vector3 curMPWorld)
    {
        Debug.Log("GameManager.pTG: curMPWorld: " + curMPWorld);
        if (respawnTime > Time.time)
        {
            //If respawn timer is not over, don't do anything
            return;
        }
        GameState final = null;
        GameState prevFinal = null;
        foreach (GameState gs in gameStates)
        {
            if (gs.id != chosenId || playerObject.GetComponent<PlayerController>().isIntact())
            {//don't include last game state if merky is shattered
                if (gs.checkRepresentation(curMPWorld))
                {
                    if (final == null || gs.id > final.id)//assuming the later ones have higher id values
                    {
                        prevFinal = final;//keep the second-to-latest one
                        final = gs;//keep the latest one
                    }
                }
            }
        }
        if (final != null)
        {
            if (final.id == chosenId)
            {
                if (prevFinal != null)
                {//if the current one overlaps a previous one, choose the previous one
                    Rewind(prevFinal.id);
                }
                else
                {
                    Load(final.id);
                }
            }
            else
            {
                Rewind(final.id);
            }
        }
        else if (!playerObject.GetComponent<PlayerController>().isIntact())
        {
            Rewind(chosenId - 1);//go back to the latest safe past merky
        }
        if (GameStatistics.counter("deathCount") == 1)
        {
            EffectManager.highlightTapArea(Vector2.zero, false);
        }
        gestureManager.switchGestureProfile("Main");
        if (camCtr.getScalePointIndex() > CameraController.SCALEPOINT_DEFAULT)
        {
            //leave this zoom level even if no past merky was chosen
            camCtr.setScalePoint(CameraController.SCALEPOINT_DEFAULT);
        }
        if (gameManagerTapProcessed != null)
        {
            gameManagerTapProcessed(curMPWorld);
        }
    }
    public static GameManagerTapProcessed gameManagerTapProcessed;
    public delegate void GameManagerTapProcessed(Vector2 curMPWorld);

    /// <summary>
    /// Used specifically to highlight last saved Merky after the first death
    /// for tutorial purposes
    /// </summary>
    /// <returns></returns>
    public static Vector2 getLatestSafeRewindGhostPosition()
    {
        return instance.gameStates[instance.chosenId - 1].merky.position;
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
            if (lastTalkingNPC != npc)
            {
                lastTalkingNPC = npc;
                instance.musicManager.setQuiet(true);
            }
        }
        else
        {
            if (npc == lastTalkingNPC)
            {
                instance.musicManager.setQuiet(false);
                instance.npcTalkEffect.GetComponent<ParticleSystem>().Stop();
            }
        }
    }
}


