using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine.SceneManagement;


public class GameManager : MonoBehaviour
{
    public bool demoBuild = false;//true to not load on open and save with date/timestamp in filename
    public int chosenId = 0;
    public GameObject playerGhost;//this is to show Merky in the past (prefab)
    public AudioSource timeRewindMusic;//the music to play while time rewinds
    private int rewindId = 0;//the id to eventually load back to
    private float respawnTime = 0;//the earliest time Merky can rewind after shattering
    public float respawnDelay = 1.0f;//how long Merky must wait before rewinding after shattering
    private List<GameState> gameStates = new List<GameState>();
    [SerializeField]
    private List<SceneLoader> sceneLoaders = new List<SceneLoader>();
    private List<GameObject> gameObjects = new List<GameObject>();
    private List<GameObject> forgottenObjects = new List<GameObject>();//a list of objects that are inactive and thus unfindable
    private List<string> openScenes = new List<string>();//the list of names of the scenes that are open
    //Memories
    private List<MemoryObject> memories = new List<MemoryObject>();
    //Checkpoints
    private List<CheckPointChecker> activeCheckPoints = new List<CheckPointChecker>();

    private static GameManager instance;
    private static GameObject playerObject;//the player object
    public static string playerTag = "Player";
    private CameraController camCtr;
    private GestureManager gestureManager;
    public static GestureManager GestureManager
    {
        get { return instance.gestureManager; }
        private set { instance.gestureManager = value; }
    }
    private MusicManager musicManager;
    private float actionTime = 0;//used to determine how often to rewind
    private const float rewindDelay = 0.05f;//how much to delay each rewind transition by
    private List<string> newlyLoadedScenes = new List<string>();
    private int loadedSceneCount = 0;
    private string unloadedScene = null;
    private static float resetGameTimer = 0.0f;//the time that the game will reset at
    private static float gamePlayTime = 0.0f;//how long the game can be played for, 0 for indefinitely
    public GameObject endDemoScreen;//the picture to show the player after the game resets


    // Use this for initialization
    void Start()
    {
        playerObject = GameObject.FindGameObjectWithTag(playerTag);
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
    }

    /// <summary>
    /// Resets the game back to the very beginning
    /// </summary>
    public static void resetGame(bool savePrevGame = true)
    {
        //Save previous game
        if (savePrevGame)
        {
            Save();
            instance.saveToFile();
        }
        //Unload all scenes and reload PlayerScene
        instance = null;
        GameState.nextid = 0;
        SceneManager.LoadScene(0);
    }
    /// <summary>
    /// Schedules the game reset in the future
    /// </summary>
    /// <param name="timeUntilReset">How many seconds until the reset should occur</param>
    public static void setResetTimer(float timeUntilReset)
    {
        if (timeUntilReset < 0)
        {
            timeUntilReset = 0;
        }
        gamePlayTime = timeUntilReset;
        if (gamePlayTime != 0)
        {
            resetGameTimer = gamePlayTime + Time.time;
        }
        else
        {
            resetGameTimer = 0;
        }
        instance.showEndDemoScreen(false);
    }

    private void showEndDemoScreen(bool show)
    {
        endDemoScreen.SetActive(show);
        if (show)
        {
            endDemoScreen.transform.position = (Vector2)Camera.main.transform.position;
            endDemoScreen.transform.localRotation = Camera.main.transform.localRotation;
        }
    }

    private void OnGUI()
    {
        if (Time.time < resetGameTimer)
        {
            GUI.Label(Camera.main.pixelRect, "Time left: " + (resetGameTimer - Time.time));
        }
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
    private static void removeObject(GameObject go)
    {
        instance.gameObjects.Remove(go);
        instance.forgottenObjects.Remove(go);
        if (go && go.transform.childCount > 0)
        {
            foreach (Transform t in go.transform)
            {
                instance.gameObjects.Remove(t.gameObject);
                instance.forgottenObjects.Remove(t.gameObject);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (instance == null)
        {
            //2018-06-04: band aid code
            instance = this;
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
        if (gamePlayTime > 0)
        {
            if (Time.time >= resetGameTimer)
            {
                showEndDemoScreen(true);
                if ((Input.GetMouseButton(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended))
                    && Time.time >= resetGameTimer + 10)//+10 for buffer period where input doesn't interrupt it
                {
                    setResetTimer(gamePlayTime);
                    resetGame();
                }
            }
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
        openScenes.Add(s.name);
    }
    void sceneUnloaded(Scene s)
    {
        foreach (GameObject fgo in forgottenObjects)
        {
            if (fgo != null && fgo.scene == s)
            {
                forgottenObjects.Remove(fgo);
            }
        }
        refreshGameObjects();
        unloadedScene = s.name;
        openScenes.Remove(s.name);
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
        //Forgotten Objects
        foreach (GameObject dgo in forgottenObjects)
        {
            if (dgo != null)
            {
                gameObjects.Add(dgo);
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
        //Open Scenes
        foreach (SceneLoader sl in instance.sceneLoaders)
        {
            if (instance.openScenes.Contains(sl.sceneName))
            {
                if (sl.firstOpenGameStateId > instance.chosenId)
                {
                    sl.firstOpenGameStateId = instance.chosenId;
                }
                sl.lastOpenGameStateId = instance.chosenId;
            }
        }
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
        if (!instance.activeCheckPoints.Contains(cpc))
        {
            instance.activeCheckPoints.Add(cpc);
        }
    }
    /// <summary>
    /// Stores the given object before it gets set inactive
    /// </summary>
    /// <param name="obj"></param>
    public static void saveForgottenObject(GameObject obj, bool forget = true)
    {
        if (forget)
        {
            instance.forgottenObjects.Add(obj);
            obj.SetActive(false);
        }
        else
        {
            instance.forgottenObjects.Remove(obj);
            obj.SetActive(true);
        }
    }
    public static List<GameObject> getForgottenObjects()
    {
        return instance.forgottenObjects;
    }
    public static void LoadState()
    {
        instance.Load(instance.chosenId);
    }
    private void Load(int gamestateId)
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
            //Open Scenes
            foreach (SceneLoader sl in sceneLoaders)
            {
                if (sl.lastOpenGameStateId > chosenId)
                {
                    sl.lastOpenGameStateId = chosenId;
                }
                if (sl.firstOpenGameStateId > chosenId)
                {
                    sl.firstOpenGameStateId = int.MaxValue;
                    sl.lastOpenGameStateId = -1;
                }
            }
        }
        gameStates[gamestateId].load();
        if (chosenId <= rewindId)
        {
            refreshGameObjects();//a second time, just to be sure
        }
        for (int i = gameStates.Count - 1; i > gamestateId; i--)
        {
            Destroy(gameStates[i].Representation);
            gameStates.RemoveAt(i);
        }
        GameState.nextid = gamestateId + 1;
        //Recenter the camera
        camCtr.recenter();
        camCtr.refocus();
    }
    public void LoadObjectsFromScene(Scene s)
    {
        //Find the last state that this scene was saved in
        int lastStateSeen = -1;
        foreach (SceneLoader sl in sceneLoaders)
        {
            if (s.name == sl.sceneName)
            {
                lastStateSeen = sl.lastOpenGameStateId;
                break;
            }
        }
        if (lastStateSeen < 0)
        {
            return;
        }
        if (lastStateSeen > chosenId)
        {
            lastStateSeen = chosenId;
        }
        //Load Each Object
        foreach (GameObject go in gameObjects)
        {
            if (go.scene.Equals(s))
            {
                for (int stateid = lastStateSeen; stateid >= 0; stateid--)
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
    public static bool isRewinding()
    {
        return instance.chosenId > instance.rewindId;
    }
    public void cancelRewind()
    {
        rewindId = chosenId;
        Load(chosenId);
        musicManager.endEventSong(timeRewindMusic);
    }
    public static void RewindToStart()
    {
        instance.Rewind(0);
    }
    /// <summary>
    /// Sets into motion the rewind state.
    /// FixedUpdate carries out the motions of calling Load()
    /// </summary>
    /// <param name="gamestateId"></param>
    void Rewind(int gamestateId)
    {
        musicManager.setEventSong(timeRewindMusic);
        rewindId = gamestateId;
        camCtr.recenter();
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
        ES2.Save(sceneLoaders, fileName + "?tag=scenes");
    }
    public void loadFromFile()
    {
        memories = ES2.LoadList<MemoryObject>("merky.txt?tag=memories");
        gameStates = ES2.LoadList<GameState>("merky.txt?tag=states");
        //Scenes
        List<SceneLoader> rsls = ES2.LoadList<SceneLoader>("merky.txt?tag=scenes");
        foreach (SceneLoader sl in sceneLoaders)//all scene loaders
        {
            foreach (SceneLoader rsl in rsls)//read in scene loaders
            {
                if (rsl != null && sl.sceneName == rsl.sceneName && rsl != sl)
                {
                    sl.lastOpenGameStateId = rsl.lastOpenGameStateId;
                    Destroy(rsl);
                    break;
                }
            }
        }
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
        if (playerObject == null)
        {
            playerObject = GameObject.FindGameObjectWithTag(playerTag);
        }
        return playerObject;
    }
    public static int getCurrentStateId()
    {
        return instance.chosenId;
    }

    /// <summary>
    /// Returns true if the given GameObject is touching Merky's teleport range
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public static bool isInTeleportRange(GameObject other)
    {
        float range = playerObject.GetComponent<PlayerController>().Range;
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
                gs.showRepresentation(instance.chosenId);
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
            Vector2 gsPos = gs.Representation.transform.position;
            float gsDistance = Vector2.Distance(gsPos, pos);
            if (gsDistance < closestDistance)
            {
                closestDistance = gsDistance;
                closestObject = gs.Representation;
            }
        }
        return closestObject;
    }
    public static GameObject getPlayerGhostPrefab()
    {
        return instance.playerGhost;
    }

    public static void showMainMenu(bool show)
    {
        if (show)
        {
            SceneManager.LoadScene("MainMenu", LoadSceneMode.Additive);
        }
        else
        {
            SceneManager.UnloadSceneAsync("MainMenu");
        }
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
        //Sprite detection pass
        foreach (GameState gs in gameStates)
        {
            //don't include last game state if merky is shattered
            if (gs.id != chosenId || playerObject.GetComponent<PlayerController>().isIntact())
            {
                //Check sprite overlap
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
        //Collider detection pass
        if (final == null)
        {
            foreach (GameState gs in gameStates)
            {
                //don't include last game state if merky is shattered
                if (gs.id != chosenId || playerObject.GetComponent<PlayerController>().isIntact())
                {
                    //Check collider overlap
                    if (gs.checkRepresentation(curMPWorld, false))
                    {
                        if (final == null || gs.id > final.id)//assuming the later ones have higher id values
                        {
                            prevFinal = final;//keep the second-to-latest one
                            final = gs;//keep the latest one
                        }
                    }
                }
            }
        }
        //Process tapped game state
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

        //leave this zoom level even if no past merky was chosen
        float defaultZoomLevel = camCtr.scalePointToZoomLevel((int)CameraController.CameraScalePoints.DEFAULT);
        camCtr.ZoomLevel = defaultZoomLevel;
        gestureManager.switchGestureProfile("Main");

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
}


