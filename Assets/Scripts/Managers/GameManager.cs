using UnityEngine;
using System.Collections.Generic;
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
    private int chosenId;//the id of the current game state
    private float lastRewindTime;//the last time the game rewound
    private float inputOffStartTime;//the start time when input was turned off
    private float resetGameTimer;//the time that the game will reset at
    private static float gamePlayTime;//how long the game can be played for, 0 for indefinitely

    //
    // Runtime Lists
    //
    //Game States
    private List<GameState> gameStates = new List<GameState>();//basically a timeline
    private Dictionary<string, GameObject> gameObjects = new Dictionary<string, GameObject>();//list of current objects that have state to save
    private List<GameObject> forgottenObjects = new List<GameObject>();//a list of objects that are inactive and thus unfindable, but still have state to save
    //Scene Loading
    private List<Scene> openScenes = new List<Scene>();//the list of the scenes that are open
    //Memories
    private Dictionary<string, MemoryObject> memories = new Dictionary<string, MemoryObject>();//memories that once turned on, don't get turned off
    //Checkpoints
    private List<CheckPointChecker> activeCheckPoints = new List<CheckPointChecker>();//list of checkpoints that have been activated

    // Use this for initialization
    void Start()
    {
        //Initialize the current game state id
        //There are possibly none, so the default "current" is -1
        chosenId = -1;
        //If a limit has been set on the demo playtime,
        if (GameDemoLength > 0)
        {
            //Auto-enable demo mode
            demoBuild = true;
            //Tell the gesture manager to start the timer when the player taps in game
            Managers.Gesture.tapGesture += startDemoTimer;
            //Show the timer
            txtDemoTimer.transform.parent.gameObject.SetActive(true);
        }
        //If in demo mode,
        if (demoBuild)
        {
            //Save its future files with a time stamp
            saveWithTimeStamp = true;
        }
        //If it's not in demo mode, and its save file exists,
        if (!demoBuild && ES2.Exists("merky.txt"))
        {
            //Load the save file
            loadFromFile();
            //Update the game state id trackers
            chosenId = rewindId = gameStates.Count - 1;
            //Load the most recent game state
            Load(chosenId);
            //Load the memories
            LoadMemories();
        }
        //Update the list of objects that have state to save
        refreshGameObjects();
        //Register scene loading delegates
        SceneManager.sceneLoaded += sceneLoaded;
        SceneManager.sceneUnloaded += sceneUnloaded;
    }

    /// <summary>
    /// Resets the game back to the very beginning
    /// Basically starts a new game
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
        //Reset game state nextid static variable
        GameState.nextid = 0;
        //Unload all scenes and reload PlayerScene
        SceneManager.LoadScene(0);
    }

    /// <summary>
    /// How long the demo lasts, in seconds
    /// 0 to have no time limit
    /// </summary>
    public static float GameDemoLength
    {
        get { return gamePlayTime; }
        set { gamePlayTime = Mathf.Max(value, 0); }
    }

    /// <summary>
    /// Start the demo timer
    /// </summary>
    void startDemoTimer()
    {
        //If the menu is not open,
        if (Managers.Camera.ZoomLevel > Managers.Camera.toZoomLevel(CameraController.CameraScalePoints.PORTRAIT))
        {
            //Start the timer
            resetGameTimer = GameDemoLength + Time.time;
            //Unregister this delegate
            Managers.Gesture.tapGesture -= startDemoTimer;
        }
    }

    /// <summary>
    /// Shows the "Thanks for Playing" screen when the demo timer stops
    /// </summary>
    /// <param name="show">True to show the screen, false to hide it</param>
    private void showEndDemoScreen(bool show)
    {
        //Update the screen's active state
        endDemoScreen.SetActive(show);
        //If it should be shown,
        if (show)
        {
            //Also update its position and rotation
            //to keep it in front of the camera
            endDemoScreen.transform.position = (Vector2)Camera.main.transform.position;
            endDemoScreen.transform.localRotation = Camera.main.transform.localRotation;
        }
    }

    /// <summary>
    /// Adds an object to list of objects that have state to save
    /// </summary>
    /// <param name="go">The GameObject to add to the list</param>
    public static void addObject(GameObject go)
    {
        Managers.Game.addObjectImpl(go);
    }
    /// <summary>
    /// Adds all the given objects that have state to save
    /// </summary>
    /// <param name="list">List of GameObjects to add</param>
    public void addAll(List<GameObject> list)
    {
        foreach (GameObject go in list)
        {
            addObjectImpl(go);
        }
    }
    /// <summary>
    /// Adds an object to the list, if it passes all tests
    /// </summary>
    /// <param name="go">The GameObject to add to the list</param>
    private void addObjectImpl(GameObject go)
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
    /// Retrieves the GameObject from the gameObjects list by the given scene and object names
    /// </summary>
    /// <param name="sceneName">The scene name of the object</param>
    /// <param name="objectName">The name of the object</param>
    /// <returns></returns>
    public static GameObject getObject(string sceneName, string objectName)
    {
        string key = Utility.getKey(sceneName, objectName);
        //If the gameObjects list has the game object,
        if (Managers.Game.gameObjects.ContainsKey(key))
        {
            //Return it
            return Managers.Game.gameObjects[key];
        }
        //Otherwise, sorry, you're out of luck
        return null;
    }
    /// <summary>
    /// Destroys the given GameObject and updates lists
    /// </summary>
    /// <param name="go">The GameObject to destroy</param>
    public static void destroyObject(GameObject go)
    {
        Managers.Game.removeObject(go);
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

    // Update is called once per frame
    void Update()
    {
        //Check all the scene loaders
        //to see if their scene needs loaded or unloaded
        //(done this way because standard trigger methods in Unity
        //don't always play nice with teleporting characters)
        foreach (SceneLoader sl in sceneLoaders)
        {
            sl.check();
        }
        //If in demo mode,
        if (GameDemoLength > 0)
        {
            float timeLeft = 0;
            //And the timer has started,
            if (resetGameTimer > 0)
            {
                //If the timer has stopped,
                if (Time.time >= resetGameTimer)
                {
                    //Show the end demo screen
                    showEndDemoScreen(true);
                    //If the ignore-input buffer period has ended,
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
                //Else if the timer is ticking,
                else
                {
                    //Show the time remaining
                    timeLeft = resetGameTimer - Time.time;
                }
            }
            //Else if the timer has not started,
            else
            {
                //Show the max play time of the demo
                timeLeft = GameDemoLength;
            }
            //Update the timer on screen
            txtDemoTimer.text = string.Format("{0:0.00}", timeLeft);
        }
        //If the time is rewinding,
        if (Rewinding)
        {
            //And it's time to rewind the next step,
            if (Time.time > lastRewindTime + rewindDelay)
            {
                //Rewind to the next previous game state
                lastRewindTime = Time.time;
                Load(chosenId - 1);
            }
        }
    }

    void sceneLoaded(Scene scene, LoadSceneMode m)
    {
        //Update the list of objects with state to save
        Debug.Log("sceneLoaded: " + scene.name + ", old object count: " + gameObjects.Count);
        refreshGameObjects();
        Debug.Log("sceneLoaded: " + scene.name + ", new object count: " + gameObjects.Count);
        //Add the given scene to list of open scenes
        openScenes.Add(scene);
        //If time is moving forward,
        if (!Rewinding)
        {
            //Load the previous state of the objects in the scene
            LoadObjectsFromScene(scene);
            //If the game has just begun,
            if (gameStates.Count == 0)
            {
                //Create the initial save state
                Save();
            }
        }
    }
    void sceneUnloaded(Scene scene)
    {
        //Remove the given scene's objects from the forgotten objects list
        for (int i = forgottenObjects.Count - 1; i >= 0; i--)
        {
            GameObject fgo = forgottenObjects[i];
            if (fgo == null || fgo.scene == scene)
            {
                forgottenObjects.RemoveAt(i);
            }
        }
        //Update the list of game objects to save
        Debug.Log("sceneUnloaded: " + scene.name + ", old object count: " + gameObjects.Count);
        refreshGameObjects();
        Debug.Log("sceneUnloaded: " + scene.name + ", new object count: " + gameObjects.Count);
        //Remove the scene from the list of open scenes
        openScenes.Remove(scene);
    }
    public static void refresh() { Managers.Game.refreshGameObjects(); }
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
            addObjectImpl(rb.gameObject);
        }
        //Add objects that have other variables that can get rewound
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
        //Open Scenes
        foreach (SceneLoader sl in sceneLoaders)
        {
            //If the scene loader's scene is open,
            if (openScenes.Contains(sl.Scene))
            {
                //And it hasn't been open in any previous game state,
                if (sl.firstOpenGameStateId > chosenId)
                {
                    //It's first opened in this game state
                    sl.firstOpenGameStateId = chosenId;
                }
                //It's also last opened in this game state
                sl.lastOpenGameStateId = chosenId;
            }
        }
    }
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
    /// Saves the check point to the active check point list
    /// Check points' active state needs to be checked more often,
    /// so it's stored in a list easier to iterate through than the memory list
    /// </summary>
    /// <param name="cpc"></param>
    public void saveCheckPoint(CheckPointChecker cpc)
    {
        //If the list doesn't already contain the checkpoint,
        if (!activeCheckPoints.Contains(cpc))
        {
            //Add the checkpoint
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
    private void cleanObjects()
    {
        string cleanedKeys = "";
        List<string> keys = new List<string>(gameObjects.Keys);
        foreach (string key in keys)
        {
            if (gameObjects[key] == null)
            {
                cleanedKeys += key + ", ";
                gameObjects.Remove(key);
            }
        }
        if (cleanedKeys != "")
        {
            Debug.Log("Cleaned: " + cleanedKeys);
        }
    }
    public List<GameObject> ForgottenObjects
    {
        get { return forgottenObjects; }
    }
    public void Load(int gamestateId)
    {
        //Update chosenId to game-state-now
        chosenId = gamestateId;
        cleanObjects();
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
            //Re-enable physics because the rewind is over
            Managers.Physics2DSurrogate.enabled = false;
        }
        //Destroy game states in game-state-future
        for (int i = gameStates.Count - 1; i > gamestateId; i--)
        {
            Destroy(gameStates[i].Representation);
            gameStates.RemoveAt(i);
        }
        //Update the next game state id
        GameState.nextid = gamestateId + 1;
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
        Managers.Physics2DSurrogate.enabled = true;
        //Update Stats
        GameStatistics.addOne("Rewind");
    }
    void LoadMemories()
    {
        foreach (MemoryMonoBehaviour mmb in FindObjectsOfType<MemoryMonoBehaviour>())
        {
            string key = mmb.gameObject.getKey();
            if (memories.ContainsKey(key))
            {
                mmb.acceptMemoryObject(memories[key]);
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
        memories = ES2.LoadDictionary<string, MemoryObject>("merky.txt?tag=memories");
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
            //Update Stats
            GameStatistics.addOne("RewindPlayer");
        }
        //Else if merky is dead,
        else if (!intact)
        {
            //go back to the latest safe past merky
            //-1 to prevent trap saves
            Rewind(chosenId - 1);
            //Update Stats
            GameStatistics.addOne("RewindPlayer");
        }
        if (GameStatistics.get("Death") == 1)
        {
            Managers.Effect.highlightTapArea(Vector2.zero, false);
        }

        //leave this zoom level even if no past merky was chosen
        float defaultZoomLevel = Managers.Camera.toZoomLevel(CameraController.CameraScalePoints.DEFAULT);
        Managers.Camera.ZoomLevel = defaultZoomLevel;
        Managers.Gesture.switchGestureProfile(GestureManager.GestureProfileType.MAIN);

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


