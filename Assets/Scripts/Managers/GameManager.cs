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
        //Init the ScenesManager
        Managers.Scene.init();
        //Check to see which levels need loaded
        Managers.Scene.checkScenes();
        //If it's not in demo mode, and its save file exists,
        if (!demoBuild && ES3.FileExists("merky.txt"))
        {
            //Load the save file
            loadFromFile();
        }
        //Update the list of objects that have state to save
        Managers.Object.refreshGameObjects();
        //Update the game state id trackers
        Managers.Rewind.init();
        //Load the memories
        Managers.Object.LoadMemories();
        //Scene delegates
        Managers.Scene.onSceneLoaded += sceneLoaded;
        Managers.Scene.onSceneUnloaded += sceneUnloaded;
        //Register rewind delegates
        Managers.Rewind.onGameStateSaved += Managers.Scene.updateSceneLoadersForward;
        Managers.Rewind.onRewindStarted += processRewindStart;
        Managers.Rewind.onRewindFinished += processRewindEnd;
    }

    // Update is called once per frame
    void Update()
    {

        if (MenuManager.Open)
        {
            //do nothing
        }
        else
        {
            if (Managers.Rewind.Rewinding)
            {
                Managers.Rewind.processRewind();
                Managers.Physics2DSurrogate.processFrame();
            }
            else
            {
                //Check all the scene loaders
                //to see if their scene needs loaded or unloaded
                //(done this way because standard trigger methods in Unity
                //don't always play nice with teleporting characters)
                Managers.Scene.checkScenes();
                //Camera screen dimensions
                Managers.Camera.checkScreenDimensions();
                //NPC Dialogue
                if (Managers.NPC.enabled)
                {
                    Managers.NPC.processDialogue();
                }
                //Music Fade
                Managers.Music.processFade();
                //Visual Effects
                Managers.Effect.processEffects();
            }
        }
        Managers.Gesture.processGestures();
        Managers.Camera.updateCameraPosition();
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

    #region Scene Load delegates
    void sceneLoaded(Scene scene)
    {
        if (scene.name == "MainMenu")
        {
            Managers.Menu.opened();
            if (Managers.Rewind.Rewinding)
            {
                Managers.Effect.showRewindEffect(false);
            }
        }
        //Update the list of objects with state to save
#if UNITY_EDITOR
        Logger.log(this, "sceneLoaded: " + scene.name + ", old object count: " + Managers.Object.GameObjectCount);
#endif
        Managers.Object.refreshGameObjects();
#if UNITY_EDITOR
        Logger.log(this, "sceneLoaded: " + scene.name + ", new object count: " + Managers.Object.GameObjectCount);
#endif
        //If time is moving forward,
        if (!Managers.Rewind.Rewinding)
        {
            //Load the previous state of the objects in the scene
            Managers.Scene.LoadObjectsFromScene(scene);
        }
        //If the game has just begun,
        if (Managers.Rewind.GameStateCount == 0)
        {
            //Create the initial save state
            Managers.Rewind.Save();
        }
    }
    void sceneUnloaded(Scene scene)
    {
        if (scene.name == "MainMenu")
        {
            if (Managers.Rewind.Rewinding)
            {
                Managers.Effect.showRewindEffect(true);
            }
        }
        //Remove the given scene's objects from the forgotten objects list
        Managers.Object.ForgottenObjects.RemoveAll(
            fgo => fgo == null || ReferenceEquals(fgo, null)
            || fgo.scene == scene
            );
        //Update the list of game objects to save
#if UNITY_EDITOR
        Logger.log(this, "sceneUnloaded: " + scene.name + ", old object count: " + Managers.Object.GameObjectCount);
#endif
        Managers.Object.refreshGameObjects();
#if UNITY_EDITOR
        Logger.log(this, "sceneUnloaded: " + scene.name + ", new object count: " + Managers.Object.GameObjectCount);
#endif
    }
    #endregion

    #region Rewind Delegates
    
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
        //Prepare scenes
        Managers.Scene.prepareForRewind(gameStates, rewindStateId);
    }
    void processRewindEnd(List<GameState> gameStates, int rewindStateId)
    {
        //Refresh the game object list
        Managers.Object.refreshGameObjects();
        //Put the music back to normal
        Managers.Music.SongSpeed = Managers.Music.normalSongSpeed;
        //Stop rewind visual effect
        Managers.Effect.showRewindEffect(false);
        //Unpause time
        Managers.Time.setPause(this, false);
        //Re-enable physics because the rewind is over
        Managers.Physics2DSurrogate.enabled = false;
        //Update SceneLoaders
        Managers.Scene.updateSceneLoadersBackward(rewindStateId);
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


