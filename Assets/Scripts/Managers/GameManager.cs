using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Linq;

/// <summary>
/// GameManager in charge of Time and Space
/// </summary>
public class GameManager : MonoBehaviour
{
    //
    // Settings
    //

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
    private float resetGameTimer;//the time that the game will reset at
    private float gamePlayTime;//how long the game can be played for, 0 for indefinitely


    private string pauseForLoadingSceneName = null;//the name of the scene that needs the game to pause while it's loading
    public string PauseForLoadingSceneName
    {
        get => pauseForLoadingSceneName;
        set
        {
            pauseForLoadingSceneName = value;
            if (pauseForLoadingSceneName == null || pauseForLoadingSceneName == "")
            {
                //Resume if the scene is done loading
                Managers.Time.setPause(this, false);
            }
            else
            {
                //Pause if the scene is still loading
                Managers.Time.setPause(this, true);
            }
        }
    }

    //
    // Runtime Lists
    //

    //Scene Loading
    private List<Scene> openScenes = new List<Scene>();//the list of the scenes that are open
    public bool playerSceneLoaded { get; private set; } = false;

    // Use this for initialization
    void Start()
    {
        //If a limit has been set on the demo playtime,
        if (GameDemoLength > 0)
        {
            //Auto-enable demo mode
            demoBuild = true;
            //Tell the gesture manager to start the timer when the player taps in game
            //Managers.Gesture.tapGesture += startDemoTimer;
            //Show the timer
            txtDemoTimer.transform.parent.gameObject.SetActive(true);
        }
        //If in demo mode,
        if (demoBuild)
        {
            //Save its future files with a time stamp
            saveWithTimeStamp = true;
        }
#if UNITY_EDITOR
        //Add list of already open scenes to open scene list (for editor)
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            openScenes.Add(SceneManager.GetSceneAt(i));
        }
        if (openScenes.Find(scene => scene.name == "PlayerScene") != null)
        {
            playerSceneLoaded = true;
        }
#endif
        //Check to see which levels need loaded
        checkScenes();
        //If it's not in demo mode, and its save file exists,
        if (!demoBuild && ES3.FileExists("merky.txt"))
        {
            //Load the save file
            loadFromFile();
            //Update the game state id trackers
            Managers.Rewind.init();
            //Load the memories
            Managers.Object.LoadMemories();
        }
        //Register scene loading delegates
        SceneManager.sceneLoaded += sceneLoaded;
        SceneManager.sceneUnloaded += sceneUnloaded;
        //Register rewind delegates
        Managers.Rewind.onGameStateSaved += saveSceneStateIds;
        Managers.Rewind.onRewindStarted += processRewindStart;
        Managers.Rewind.onRewindFinished += processRewindEnd;
    }

    // Update is called once per frame
    void Update()
    {
        //Check all the scene loaders
        //to see if their scene needs loaded or unloaded
        //(done this way because standard trigger methods in Unity
        //don't always play nice with teleporting characters)
        if (!Managers.Rewind.Rewinding)
        {
            checkScenes();
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
    }

    #region Space Management
    void checkScenes()
    {
        foreach (SceneLoader sl in sceneLoaders)
        {
            sl.check();
        }
    }

    void sceneLoaded(Scene scene, LoadSceneMode m)
    {
        if (scene.name == "PlayerScene")
        {
            playerSceneLoaded = true;
        }
        //Update the list of objects with state to save
#if UNITY_EDITOR
        Logger.log(this, "sceneLoaded: " + scene.name + ", old object count: " + Managers.Object.GameObjectCount);
#endif
        Managers.Object.refreshGameObjects();
#if UNITY_EDITOR
        Logger.log(this, "sceneLoaded: " + scene.name + ", new object count: " + Managers.Object.GameObjectCount);
#endif
        //Add the given scene to list of open scenes
        openScenes.Add(scene);
        //If time is moving forward,
        if (!Managers.Rewind.Rewinding)
        {
            //Load the previous state of the objects in the scene
            LoadObjectsFromScene(scene);
        }
        //If the game has just begun,
        if (Managers.Rewind.GameStateCount == 0)
        {
            //Create the initial save state
            Managers.Rewind.Save();
        }
        //If its a level scene,
        SceneLoader sceneLoader = getSceneLoaderByName(scene.name);
        if (sceneLoader)
        {
            if (scene.name == PauseForLoadingSceneName)
            {
                //Unpause the game
                PauseForLoadingSceneName = null;
            }
        }
    }
    void sceneUnloaded(Scene scene)
    {
        //Remove the given scene's objects from the forgotten objects list
        Managers.Object.ForgottenObjects.RemoveAll(fgo => fgo.scene == scene);
        //Update the list of game objects to save
#if UNITY_EDITOR
        Logger.log(this, "sceneUnloaded: " + scene.name + ", old object count: " + Managers.Object.GameObjectCount);
#endif
        Managers.Object.refreshGameObjects();
#if UNITY_EDITOR
        Logger.log(this, "sceneUnloaded: " + scene.name + ", new object count: " + Managers.Object.GameObjectCount);
#endif
        //Remove the scene from the list of open scenes
        openScenes.Remove(scene);
    }

    public bool isSceneOpen(Scene scene)
    {
        foreach (Scene s in openScenes)
        {
            if (scene == s)
            {
                return true;
            }
        }
        return false;
    }

    public bool isSceneOpenByName(string sceneName)
    {
        foreach (Scene s in openScenes)
        {
            if (sceneName == s.name)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Restores the objects in the scene to their previous state before the scene was unloaded
    /// </summary>
    /// <param name="scene">The scene whose objects need their state stored</param>
    public void LoadObjectsFromScene(Scene scene)
    {
        //Find the last state that this scene was saved in
        int lastStateSeen = -1;
        SceneLoader sceneLoader = sceneLoaders.Find(sl => sl.Scene == scene);
        if (sceneLoader)
        {
            lastStateSeen = sceneLoader.lastOpenGameStateId;
        }

#if UNITY_EDITOR
        Logger.log(this, "LOFS: Scene " + scene.name + ": last state seen: " + lastStateSeen);
#endif
        //If the scene was never seen,
        if (lastStateSeen < 0)
        {
            //Don't restore its objects' states,
            //because there's nothing to restore
            return;
        }
        //If the scene was last seen after gamestate-now,
        //The scene is now last seen gamestate-now
        lastStateSeen = Mathf.Min(lastStateSeen, Managers.Rewind.GameStateId);
        //Load the objects
        Managers.Rewind.LoadObjects(
            scene.name,
            lastStateSeen,
            go => go.scene == scene
            );
    }

    private SceneLoader getSceneLoaderByName(string sceneName)
    {
        foreach (SceneLoader sl in sceneLoaders)
        {
            if (sl.sceneName == sceneName)
            {
                return sl;
            }
        }
        return null;
    }
    #endregion

    #region Rewind Delegates
    void saveSceneStateIds(int gameStateId)
    {
        //Open Scenes
        foreach (SceneLoader sl in sceneLoaders)
        {
            //If the scene loader's scene is open,
            if (openScenes.Contains(sl.Scene))
            {
                //And it hasn't been open in any previous game state,
                if (sl.firstOpenGameStateId > gameStateId)
                {
                    //It's first opened in this game state
                    sl.firstOpenGameStateId = gameStateId;
                }
                //It's also last opened in this game state
                sl.lastOpenGameStateId = gameStateId;
            }
        }
    }
    void processRewindStart(List<GameState> gameStates, int rewindStateId)
    {
        //Set the music speed to rewind
        Managers.Music.SongSpeed = Managers.Music.rewindSongSpeed;
        //Show rewind visual effect
        Managers.Effect.showRewindEffect(true);
        //Recenter the camera on Merky
        Managers.Camera.recenter();
        //Disable physics while rewinding
        Managers.Physics2DSurrogate.enabled = true;
        //Pause time
        Managers.Time.setPause(this, true);
        //Update Stats
        GameStatistics.addOne("Rewind");
        //Load levels that Merky will be passing through
        foreach (SceneLoader sl in sceneLoaders)
        {
            for (int i = gameStates.Count - 1; i >= rewindStateId; i--)
            {
                if (sl.isPositionInScene(gameStates[i].Merky.position))
                {
                    sl.loadLevelIfUnLoaded();
                    break;
                }
            }
        }
    }
    void processRewindEnd(List<GameState> gameStates, int rewindStateId)
    {
        //Put the music back to normal
        Managers.Music.SongSpeed = Managers.Music.normalSongSpeed;
        //Stop rewind visual effect
        Managers.Effect.showRewindEffect(false);
        //Unpause time
        Managers.Time.setPause(this, false);
        //Re-enable physics because the rewind is over
        Managers.Physics2DSurrogate.enabled = false;
        //Update Scene tracking variables
        foreach (SceneLoader sl in sceneLoaders)
        {
            //If the scene was last opened after game-state-now,
            if (sl.lastOpenGameStateId > rewindStateId)
            {
                //it is now last opened game-state-now
                sl.lastOpenGameStateId = rewindStateId;
            }
            //if the scene was first opened after game-state-now,
            if (sl.firstOpenGameStateId > rewindStateId)
            {
                //it is now never opened
                sl.firstOpenGameStateId = int.MaxValue;
                sl.lastOpenGameStateId = -1;
            }
        }
    }
    #endregion

    #region File Management
    /// <summary>
    /// Saves the memories, game states, and scene cache to a save file
    /// </summary>
    public void saveToFile()
    {
        //Set the base filename
        string fileName = "merky";
        //If saving with time stamp,
        if (saveWithTimeStamp)
        {
            //Add the time stamp to the filename
            System.DateTime now = System.DateTime.Now;
            fileName += "-" + now.Ticks;
        }
        //Add an extension to the filename
        fileName += ".txt";
        //Save game states and memories
        Managers.Rewind.saveToFile(fileName);
        Managers.Object.saveToFile(fileName);
        //Save settings
        Managers.Settings.saveSettings();
        //Save file settings
        List<SettingObject> settings = new List<SettingObject>();
        foreach (Setting setting in FindObjectsOfType<MonoBehaviour>().OfType<Setting>())
        {
            if (setting.Scope == SettingScope.SAVE_FILE)
            {
                settings.Add(setting.Setting);
            }
        }
        ES3.Save<List<SettingObject>>("settings", settings, fileName);
    }
    /// <summary>
    /// Loads the game from the save file
    /// It assumes the file already exists
    /// </summary>
    public void loadFromFile()
    {
        try
        {
            //Set the base filename
            string fileName = "merky";
            //Add an extension to the filename
            fileName += ".txt";
            //Load game states and memories
            Managers.Rewind.loadFromFile(fileName);
            Managers.Object.loadFromFile(fileName);
            //Load settings
            Managers.Settings.loadSettings();
            //Load file settings
            List<SettingObject> settings = ES3.Load<List<SettingObject>>("settings", fileName);
            foreach (Setting setting in FindObjectsOfType<MonoBehaviour>().OfType<Setting>())
            {
                if (setting.Scope == SettingScope.SAVE_FILE)
                {
                    string id = setting.ID;
                    foreach (SettingObject setObj in settings)
                    {
                        if (id == setObj.id)
                        {
                            setting.Setting = setObj;
                            break;
                        }
                    }
                }
            }
        }
        catch (System.Exception e)
        {
            if (ES3.FileExists("merky.txt"))
            {
                ES3.DeleteFile("merky.txt");
            }
            resetGame(false);
        }
    }
    //Sent to all GameObjects before the application is quit
    //Auto-save on exit
    void OnApplicationQuit()
    {
        //Save the game state and then
        Managers.Rewind.Save();
        //Save the game to file
        saveToFile();
    }
    private void OnApplicationPause(bool pause)
    {
        if (pause)
        {
            OnApplicationQuit();
        }
    }
    #endregion

    #region Player Ghosts
    /// <summary>
    /// Shows the game state representations
    /// </summary>
    public void showPlayerGhosts(bool show)
    {
        //If the game state representations should be shown,
        if (show)
        {
            //Loop through all game states
            foreach (GameState gs in Managers.Rewind.GameStates)
            {
                //Show a sprite to represent them on screen
                gs.showRepresentation(Managers.Rewind.GameStateId);
            }
        }
        //Else, they should be hidden
        else
        {
            //Loop through all game states
            foreach (GameState gs in Managers.Rewind.GameStates)
            {
                //And hide their representations
                gs.hideRepresentation();
            }
        }
    }
    /// <summary>
    /// Returns the player ghost that is closest to the given position
    /// </summary>
    /// <param name="pos">The ideal position of the closest ghost</param>
    /// <returns>The player ghost that is closest to the given position</returns>
    public GameObject getClosestPlayerGhost(Vector2 pos)
    {
        float closestDistance = float.MaxValue;
        GameObject closestObject = null;
        foreach (GameState gs in Managers.Rewind.GameStates)
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
    /// <summary>
    /// Used specifically to highlight last saved Merky after the first death
    /// for tutorial purposes
    /// </summary>
    /// <returns></returns>
    public Vector2 getLatestSafeRewindGhostPosition()
        => Managers.Rewind.GameStates[
            Managers.Rewind.GameStateId - 1
            ]
            .Merky.position;

    #endregion

    #region Input Processing
    /// <summary>
    /// Processes the tap gesture at the given position
    /// </summary>
    /// <param name="curMPWorld">The position of the tap in world coordinates</param>
    public void processTapGesture(Vector3 curMPWorld)
    {
        GameState final = null;
        GameState prevFinal = null;
        //We have to do 2 passes to allow for both precision clicking and fat-fingered tapping
        //Sprite detection pass
        foreach (GameState gs in Managers.Rewind.GameStates)
        {
            //Check sprite overlap
            if (gs.checkRepresentation(curMPWorld))
            {
                //If this game state is more recent than the current picked one,
                if (final == null || gs.id > final.id)//assuming the later ones have higher id values
                {
                    //Set the current picked one to the previously picked one
                    prevFinal = final;//remember the second-to-latest one
                    //Set this game state to the current picked one
                    final = gs;//keep the latest one                    
                }
            }
        }
        //Collider detection pass
        if (final == null)
        {
            foreach (GameState gs in Managers.Rewind.GameStates)
            {
                //Check collider overlap
                if (gs.checkRepresentation(curMPWorld, false))
                {
                    //If this game state is more recent than the current picked one,
                    if (final == null || gs.id > final.id)//assuming the later ones have higher id values
                    {
                        //Set the current picked one to the previously picked one
                        prevFinal = final;//remember the second-to-latest one
                        //Set this game state to the current picked one
                        final = gs;//keep the latest one
                    }
                }
            }
        }
        //Process tapped game state
        //If a past merky was indeed selected,
        if (final != null)
        {
            //If the tapped one is already the current one,
            if (final.id == Managers.Rewind.GameStateId)
            {
                //And if the current one overlaps a previous one,
                if (prevFinal != null)
                {
                    //Choose the previous one
                    Managers.Rewind.RewindTo(prevFinal.id);
                }
                else
                {
                    //Else, Reload the current one
                    Managers.Rewind.Load(final.id);
                }
            }
            //Else if a past one was tapped,
            else
            {
                //Rewind back to it
                Managers.Rewind.RewindTo(final.id);
            }
            //Update Stats
            GameStatistics.addOne("RewindPlayer");
        }

        //Leave this zoom level even if no past merky was chosen
        float defaultZoomLevel = Managers.Camera.toZoomLevel(CameraController.CameraScalePoints.DEFAULT);
        Managers.Camera.ZoomLevel = defaultZoomLevel;
        Managers.Gesture.switchGestureProfile(GestureManager.GestureProfileType.MAIN);

        //Process tapProcessed delegates
        tapProcessed?.Invoke(curMPWorld);
    }
    public delegate void TapProcessed(Vector2 curMPWorld);
    public TapProcessed tapProcessed;
    #endregion

    #region Demo Mode Methods
    /// <summary>
    /// Resets the game back to the very beginning
    /// Basically starts a new game
    /// </summary>
    public void resetGame(bool savePrevGame = true)
    {
        //Save previous game
        if (savePrevGame)
        {
            Managers.Rewind.Save();
            saveToFile();
        }
        //Empty object lists
        Managers.Object.clearObjects();
        //Reset game state nextid static variable
        GameState.nextid = 0;
        //Unset SceneLoader static variables
        SceneLoader.ExplorerObject = null;
        //Unload all scenes and reload PlayerScene
        SceneManager.LoadScene(0);
    }

    /// <summary>
    /// How long the demo lasts, in seconds
    /// 0 to have no time limit
    /// </summary>
    public float GameDemoLength
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
            //Managers.Gesture.tapGesture -= startDemoTimer;
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
    #endregion

}


