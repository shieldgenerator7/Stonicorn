using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    //
    // Settings
    //
    [Header("Settings")]
    [SerializeField]
    private float inputOffDuration = 1.0f;//how long Merky must wait before rewinding after shattering
    [SerializeField]
    private float rewindDelay = 0.05f;//the delay between rewind transitions

    [Header("Objects")]
    public GameObject playerGhostPrefab;//this is to show Merky in the past (prefab)
    [SerializeField]
    private List<SceneLoader> sceneLoaders = new List<SceneLoader>();

    [Header("Demo Mode")]
    [SerializeField]
    private bool demoBuild = false;//true to not load on open or save with date/timestamp in filename
    [SerializeField]
    private bool saveWithTimeStamp = false;//true to save with date/timestamp in filename, even when not in demo build
    [SerializeField]
    private float restartDemoDelay = 10;//how many seconds before the game can reset after the demo ends
    [SerializeField]
    private Text txtDemoTimer;//the text that shows much time is left in the demo
    [SerializeField]
    private GameObject endDemoScreen;//the picture to show the player after the game resets

    //
    // Runtime variables
    //
    private int rewindId;//the id to eventually load back to
    private int chosenId;
    private float lastRewindTime;//the last time the game rewound
    private float inputOffStartTime;//the start time when input was turned off
    private string unloadedScene = null;
    private float resetGameTimer;//the time that the game will reset at
    private static float gamePlayTime;//how long the game can be played for, 0 for indefinitely

    //
    // Runtime Lists
    //
    //Game States
    private List<GameState> gameStates = new List<GameState>();
    private Dictionary<string, GameObject> gameObjects = new Dictionary<string, GameObject>();
    private List<GameObject> forgottenObjects = new List<GameObject>();//a list of objects that are inactive and thus unfindable
    //Scene Loading
    private List<string> openScenes = new List<string>();//the list of names of the scenes that are open
    //Memories
    private List<MemoryObject> memories = new List<MemoryObject>();
    //Checkpoints
    private List<CheckPointChecker> activeCheckPoints = new List<CheckPointChecker>();

    // Use this for initialization
    void Start()
    {
        chosenId = -1;
        //If a limit has been set on the demo playtime
        if (GameDemoLength > 0)
        {
            demoBuild = true;//auto enable demo build mode
            Managers.Gesture.tapGesture += startDemoTimer;
            txtDemoTimer.transform.parent.gameObject.SetActive(true);
        }
        if (demoBuild)
        {
            saveWithTimeStamp = true;
        }
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
    public void resetGame(bool savePrevGame = true)
    {
        //Save previous game
        if (savePrevGame)
        {
            Save();
            saveToFile();
        }
        //Empty object lists
        gameObjects.Clear();
        memories.Clear();
        activeCheckPoints.Clear();
        //Unload all scenes and reload PlayerScene
        GameState.nextid = 0;
        SceneManager.LoadScene(0);
    }
    public static float GameDemoLength
    {
        get
        {
            return gamePlayTime;
        }
        set
        {
            gamePlayTime = Mathf.Max(value, 0);
        }
    }
    void startDemoTimer()
    {
        if (Managers.Camera.ZoomLevel > Managers.Camera.scalePointToZoomLevel((int)CameraController.CameraScalePoints.PORTRAIT))
        {
            resetGameTimer = GameDemoLength + Time.time;
            Managers.Gesture.tapGesture -= startDemoTimer;
        }
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

    public static void addObject(GameObject go)
    {
        Managers.Game.addObjectImpl(go);
    }
    public void addAll(List<GameObject> list)
    {
        foreach (GameObject go in list)
        {
            addObjectImpl(go);
        }
    }
    private void addObjectImpl(GameObject go)
    {
        //Error checking

        //If the game object's name is already in the dictionary...
        if (gameObjects.ContainsKey(go.name))
        {
            throw new System.ArgumentException(
                  "GameObject (" + go.name + ") is already inside the gameObjects dictionary! "
                  + "Check for 2 or more objects with the same name. scene: " + go.scene.name
                  );
        }
        //If the game object doesn't have any state to save...
        if (!go.isSavable())
        {
            throw new System.ArgumentException(
                "GameObject (" + go.name + ") doesn't have any state to save! "
                + "Check to make sure it has a Rigidbody2D or a SavableMonoBehaviour. scene: " + go.scene.name
                );
        }
        //Else if all good, add the object
        gameObjects.Add(go.name, go);
    }
    /// <summary>
    /// Destroys the given GameObject and updates lists
    /// </summary>
    /// <param name="go"></param>
    public static void destroyObject(GameObject go)
    {
        Managers.Game.removeObject(go);
        Destroy(go);
    }
    /// <summary>
    /// Removes the given GameObject from the gameObjects list
    /// </summary>
    /// <param name="go"></param>
    private void removeObject(GameObject go)
    {
        gameObjects.Remove(go.name);
        forgottenObjects.Remove(go);
        if (go && go.transform.childCount > 0)
        {
            foreach (Transform t in go.transform)
            {
                gameObjects.Remove(t.gameObject.name);
                forgottenObjects.Remove(t.gameObject);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        foreach (SceneLoader sl in sceneLoaders)
        {
            sl.check();
        }
        if (GameDemoLength > 0)
        {
            float timeLeft = 0;
            if (resetGameTimer > 0)
            {
                if (Time.time >= resetGameTimer)
                {
                    showEndDemoScreen(true);
                    //If the buffer period has ended,
                    if (Time.time >= resetGameTimer + restartDemoDelay)
                    {
                        //And user has given input,
                        if (Input.GetMouseButton(0)
                            || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended)
                            )
                        {
                            //Reset game
                            showEndDemoScreen(false);
                            resetGame();
                        }
                    }
                }
                else
                {
                    timeLeft = resetGameTimer - Time.time;
                }
            }
            else
            {
                timeLeft = GameDemoLength;
            }
            txtDemoTimer.text = string.Format("{0:0.00}", timeLeft);
        }
        if (Rewinding)
        {
            if (Time.time > lastRewindTime + rewindDelay)
            {
                lastRewindTime = Time.time;
                Load(chosenId - 1);
            }
        }
    }

    void sceneLoaded(Scene s, LoadSceneMode m)
    {
        Debug.Log("sceneLoaded: " + s.name + ", old object count: " + gameObjects.Count);
        refreshGameObjects();
        Debug.Log("sceneLoaded: " + s.name + ", new object count: " + gameObjects.Count);
        openScenes.Add(s.name);
        if (!Rewinding)
        {
            LoadObjectsFromScene(s);
            //If the game has just begun,
            if (gameStates.Count == 0)
            {
                //Create the initial save state
                Save();
            }
        }
    }
    void sceneUnloaded(Scene s)
    {
        for (int i = forgottenObjects.Count - 1; i >= 0; i--)
        {
            GameObject fgo = forgottenObjects[i];
            if (fgo == null || fgo.scene == s)
            {
                forgottenObjects.RemoveAt(i);
            }
        }
        Debug.Log("sceneUnloaded: " + s.name + ", old object count: " + gameObjects.Count);
        refreshGameObjects();
        Debug.Log("sceneUnloaded: " + s.name + ", new object count: " + gameObjects.Count);
        openScenes.Remove(s.name);
    }
    public static void refresh() { Managers.Game.refreshGameObjects(); }
    public void refreshGameObjects()
    {
        gameObjects = new Dictionary<string, GameObject>();
        foreach (Rigidbody2D rb in FindObjectsOfType<Rigidbody2D>())
        {
            addObjectImpl(rb.gameObject);
        }
        foreach (SavableMonoBehaviour smb in FindObjectsOfType<SavableMonoBehaviour>())
        {
            if (!gameObjects.ContainsValue(smb.gameObject))
            {
                addObjectImpl(smb.gameObject);
            }
        }
        //Forgotten Objects
        foreach (GameObject fgo in forgottenObjects)
        {
            if (fgo != null)
            {
                addObjectImpl(fgo);
            }
        }
        foreach (MemoryMonoBehaviour mmb in FindObjectsOfType<MemoryMonoBehaviour>())
        {
            //load state if found, save state if not foud
            bool foundMO = false;
            foreach (MemoryObject mo in memories)
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
                memories.Add(mmb.getMemoryObject());
            }
        }
    }
    public void Save()
    {
        gameStates.Add(new GameState(gameObjects.Values));
        chosenId++;
        rewindId++;
        //Open Scenes
        foreach (SceneLoader sl in sceneLoaders)
        {
            if (openScenes.Contains(sl.sceneName))
            {
                if (sl.firstOpenGameStateId > chosenId)
                {
                    sl.firstOpenGameStateId = chosenId;
                }
                sl.lastOpenGameStateId = chosenId;
            }
        }
    }
    public void saveMemory(MemoryMonoBehaviour mmb)
    {
        bool foundMO = false;
        foreach (MemoryObject mo in memories)
        {
            if (mo.isFor(mmb))
            {
                foundMO = true;
                mo.found = mmb.getMemoryObject().found;
                break;
            }
        }
        if (!foundMO)
        {
            memories.Add(mmb.getMemoryObject());
        }
    }
    public void saveCheckPoint(CheckPointChecker cpc)//checkpoints have to work across levels, so they need to be saved separately
    {
        if (!activeCheckPoints.Contains(cpc))
        {
            activeCheckPoints.Add(cpc);
        }
    }
    /// <summary>
    /// Stores the given object before it gets set inactive
    /// </summary>
    /// <param name="obj"></param>
    public void saveForgottenObject(GameObject obj, bool forget = true)
    {
        if (obj == null)
        {
            throw new System.ArgumentNullException("GameManager.saveForgottenObject() cannot accept null for obj! obj: " + obj);
        }
        if (forget)
        {
            forgottenObjects.Add(obj);
            obj.SetActive(false);
        }
        else
        {
            forgottenObjects.Remove(obj);
            obj.SetActive(true);
        }
    }
    public static Dictionary<string, GameObject> GameObjects
    {
        get { return Managers.Game.gameObjects; }
    }
    public List<GameObject> ForgottenObjects
    {
        get { return forgottenObjects; }
    }
    public void Load(int gamestateId)
    {
        //Update chosenId to game-state-now
        chosenId = gamestateId;
        //Destroy objects not spawned yet in the new selected state
        List<GameObject> destroyObjectList = new List<GameObject>();
        foreach (GameObject go in gameObjects.Values)
        {
            foreach (SavableMonoBehaviour smb in go.GetComponents<SavableMonoBehaviour>())
            {
                //If the game object was spawned during run time
                //(versus pre-placed at edit time)
                if (smb.isSpawnedObject())
                {
                    //And if the game object is not in the game state,
                    if (!gameStates[gamestateId].hasGameObject(go))
                    {
                        //remove it from game objects list
                        //by saving it to the list of gmae objects to be destroyed
                        destroyObjectList.Add(go);
                    }
                }
            }
        }
        //Actually destroy the objects that need destroyed
        for (int i = destroyObjectList.Count - 1; i >= 0; i--)
        {
            //This is to get around the problem of deleting objects from a list that you're iterating over
            destroyObject(destroyObjectList[i]);
        }
        //Actually load the game state
        gameStates[gamestateId].load();
        //If the rewind is finished,
        if (chosenId == rewindId)
        {
            //Refresh the game object list
            refreshGameObjects();
            //Put the music back to normal
            Managers.Music.SongSpeed = Managers.Music.normalSongSpeed;
            //Update Scene tracking variables
            foreach (SceneLoader sl in sceneLoaders)
            {
                //If the scene was last opened after game-state-now,
                if (sl.lastOpenGameStateId > chosenId)
                {
                    //it is now last opened game-state-now
                    sl.lastOpenGameStateId = chosenId;
                }
                //if the scene was first opened after game-state-now,
                if (sl.firstOpenGameStateId > chosenId)
                {
                    //it is now never opened
                    sl.firstOpenGameStateId = int.MaxValue;
                    sl.lastOpenGameStateId = -1;
                }
            }
            //Destroy game states in game-state-future
            for (int i = gameStates.Count - 1; i > gamestateId; i--)
            {
                Destroy(gameStates[i].Representation);
                gameStates.RemoveAt(i);
            }
            //Update the next game state id
            GameState.nextid = gamestateId + 1;
            //Re-enable physics because the rewind is over
            Physics2D.autoSimulation = true;
        }
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
        Debug.Log("LOFS: Scene " + s.name + ": last state seen: " + lastStateSeen);
        if (lastStateSeen < 0)
        {
            return;
        }
        if (lastStateSeen > chosenId)
        {
            lastStateSeen = chosenId;
        }
        int newObjectsFound = 0;
        int objectsLoaded = 0;
        //Load Each Object
        foreach (GameObject go in gameObjects.Values)
        {
            if (go.scene == s)
            {
                newObjectsFound++;
                for (int stateid = lastStateSeen; stateid >= 0; stateid--)
                {
                    if (gameStates[stateid].loadObject(go))
                    {
                        objectsLoaded++;
                        break;
                    }
                    else
                    {
                        //continue until you find the game state that has the most recent information about this object
                    }
                }
            }
        }
        Debug.Log("LOFS: Scene " + s.name + ": objects found: " + newObjectsFound + ", objects loaded: " + objectsLoaded);
    }
    public bool Rewinding
    {
        get { return chosenId > rewindId; }
    }
    public void cancelRewind()
    {
        rewindId = chosenId;
        Load(chosenId);
        Managers.Music.SongSpeed = Managers.Music.normalSongSpeed;
    }
    public void RewindToStart()
    {
        Rewind(0);
    }
    /// <summary>
    /// Sets into motion the rewind state.
    /// FixedUpdate carries out the motions of calling Load()
    /// </summary>
    /// <param name="gamestateId"></param>
    void Rewind(int gamestateId)
    {
        Managers.Music.SongSpeed = Managers.Music.rewindSongSpeed;
        rewindId = gamestateId;
        Managers.Camera.recenter();
        //Disable physics while rewinding
        Physics2D.autoSimulation = false;
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
        if (saveWithTimeStamp)
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
    void OnApplicationQuit()
    {
        Save();
        saveToFile();
    }

    public List<CheckPointChecker> ActiveCheckPoints
    {
        get { return activeCheckPoints; }
    }

    public int CurrentStateId
    {
        get { return chosenId; }
    }

    /// <summary>
    /// Used primarily for managing the delay between the player dying and respawning
    /// </summary>
    public bool AcceptsInputNow
    {
        get { return Time.time > inputOffStartTime + inputOffDuration; }
        set
        {
            bool acceptsNow = value;
            if (acceptsNow)
            {
                inputOffStartTime = 0;
            }
            else
            {
                inputOffStartTime = Time.time;
            }
        }
    }

    public void showPlayerGhosts()
    {
        bool intact = Managers.Player.HardMaterial.isIntact();
        foreach (GameState gs in gameStates)
        {
            //Don't include last game state if merky is shattered
            if (intact || gs.id != chosenId)
            {
                //Otherwise, show the game state's representation
                gs.showRepresentation(chosenId);
            }
        }
    }
    public void hidePlayerGhosts()
    {
        foreach (GameState gs in gameStates)
        {
            gs.hideRepresentation();
        }
    }
    /// <summary>
    /// Returns the player ghost that is closest to the given position
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    public GameObject getClosestPlayerGhost(Vector2 pos)
    {
        float closestDistance = float.MaxValue;
        GameObject closestObject = null;
        foreach (GameState gs in gameStates)
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

    public void processTapGesture(Vector3 curMPWorld)
    {
        Debug.Log("GameManager.pTG: curMPWorld: " + curMPWorld);
        //If respawn timer is not over,
        if (!AcceptsInputNow)
        {
            //don't do anything
            return;
        }
        GameState final = null;
        GameState prevFinal = null;
        bool intact = Managers.Player.HardMaterial.isIntact();
        //Sprite detection pass
        foreach (GameState gs in gameStates)
        {
            //don't include last game state if merky is shattered
            if (intact || gs.id != chosenId)
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
                if (intact || gs.id != chosenId)
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
        //If a previous merky was selected,
        if (final != null)
        {
            //If the tapped one is already the current one,
            if (final.id == chosenId)
            {
                //And if the current one overlaps a previous one,
                if (prevFinal != null)
                {
                    //Choose the previous one
                    Rewind(prevFinal.id);
                }
                else
                {
                    //Else, Reload the current one
                    Load(final.id);
                }
            }
            //Else if a past one was tapped,
            else
            {
                //Rewind back to it
                Rewind(final.id);
            }
        }
        //Else if merky is dead,
        else if (!intact)
        {
            //go back to the latest safe past merky
            //-1 to prevent trap saves
            Rewind(chosenId - 1);
        }
        if (GameStatistics.counter("deathCount") == 1)
        {
            Managers.Effect.highlightTapArea(Vector2.zero, false);
        }

        //leave this zoom level even if no past merky was chosen
        float defaultZoomLevel = Managers.Camera.scalePointToZoomLevel((int)CameraController.CameraScalePoints.DEFAULT);
        Managers.Camera.ZoomLevel = defaultZoomLevel;
        Managers.Gesture.switchGestureProfile("Main");

        if (tapProcessed != null)
        {
            tapProcessed(curMPWorld);
        }
    }
    public delegate void TapProcessed(Vector2 curMPWorld);
    public TapProcessed tapProcessed;

    /// <summary>
    /// Used specifically to highlight last saved Merky after the first death
    /// for tutorial purposes
    /// </summary>
    /// <returns></returns>
    public Vector2 getLatestSafeRewindGhostPosition()
    {
        return gameStates[chosenId - 1].merky.position;
    }
}


