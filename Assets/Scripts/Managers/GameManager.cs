using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.AddressableAssets;

/// <summary>
/// GameManager in charge of running the other managers
/// </summary>
public class GameManager : MonoBehaviour
{
    public float CAM_ROTATE_MIN = 5;
    public AssetReference testAssetRef;

    private void Awake()
    {
        Managers.initInstance();
        //Managers.Player.init();
        Managers.Camera.init();
        Addressables.InitializeAsync();
    }

    // Use this for initialization
    void Start()
    {
#if UNITY_EDITOR
        PlayerTestSpawnPoint ptsp = FindObjectOfType<PlayerTestSpawnPoint>();
        if (ptsp && ptsp.enabled)
        {
            Managers.DemoMode.DemoMode = true;
            ptsp.init();
        }
#endif
        //Register delegates
        registerDelegates();
        //Pause time
        Managers.Time.setPause(FindObjectOfType<LoadingScreen>(), true);
        //Init Gesture
        Managers.Gesture.init();
        //init DemoMode
        Managers.DemoMode.init();
        //If in demo mode,
        if (Managers.DemoMode.DemoMode)
        {
            //Save its future files with a time stamp
            Managers.File.saveWithTimeStamp = true;
        }
        //Init the ScenesManager
        Managers.Scene.init();
        //Load default level
        Managers.Level.CurrentLevelId = 0;
        Managers.Level.registerLevelGoalDelegates();
        //Check to see which levels need loaded
        Managers.Scene.checkScenes();
        //If it's not in demo mode, and its save file exists,
        if (!Managers.DemoMode.DemoMode && ES3.FileExists("merky.txt"))
        {
            //Load the save file
            Managers.File.loadFromFile();
        }
        //Update the list of objects that have state to save
        Managers.Object.refreshGameObjects();
        //Update the game state id trackers
        Managers.Rewind.init();
        //Load the memories
        Managers.Object.LoadMemories();
        //Load Tutorials scene
#if UNITY_EDITOR
        Scene tutorialScene = SceneManager.GetSceneByName("Tutorials");
        if (tutorialScene.IsValid() && tutorialScene.isLoaded)
        {
            //don't do anything
        }
        else
        {
#endif
            SceneManager.LoadSceneAsync("Tutorials", LoadSceneMode.Additive);
#if UNITY_EDITOR
        }
#endif
        //Initialize addressables
        var aoh = Addressables.InitializeAsync();
        aoh.Completed += (ao) =>
        {
            //Create new test object just to make sure it's working
            var op = Addressables.InstantiateAsync(testAssetRef);
            op.Completed += (operation) =>
            {
                GameObject newGO = operation.Result;
                Destroy(newGO);
            };
        };
    }

    private void registerDelegates()
    {
        //Scene delegates
        Managers.Scene.onSceneLoaded += sceneLoaded;
        Managers.Scene.onSceneUnloaded += sceneUnloaded;
        Managers.Scene.onPauseForLoadingSceneIdChanged +=
            (id) => Managers.Time.setPause(Managers.Scene, id >= 0);
        Managers.Scene.onSceneObjectsLoaded += Managers.Rewind.LoadSceneObjects;
        Managers.Scene.onSceneObjectsLoaded += Managers.Object.LoadSceneObjects;
        Managers.Scene.onSceneLoaded += (s) => Managers.Power.generateConnectionMap();
        Managers.Scene.onSceneUnloaded += (s) => Managers.Power.generateConnectionMap();
        //Level delegates
        Managers.Level.onLevelChanged += onLevelChanged;
        Managers.Level.onLevelFinished += onLevelFinished;
        //Menu delegates
        MenuManager.onOpenedChanged +=
            (open) => Managers.Time.setPause(this, open);
        MenuManager.onOpenedChanged +=
            (open) =>
            {
                Managers.Camera.cameraMoveFactor = (open) ? 5 : 1.5f;
                Managers.Camera.cameraZoomSpeed = (open) ? 5 : 1.5f;
            };
        //Time delegates
        Managers.Time.onPauseChanged += Managers.NPC.pauseCurrentNPC;
        Managers.Time.onPauseChanged += (paused) =>
            Cursor.lockState = (paused) ? CursorLockMode.None : CursorLockMode.Confined;
        Managers.Time.endGameTimer.onTimeFinished += Managers.Rewind.RewindToStart;
        //Rewind delegates
        Managers.Rewind.onGameStateSaved += Managers.Scene.updateSceneLoadersForward;
        Managers.Rewind.onRewindStarted += processRewindStart;
        Managers.Rewind.onRewindFinished += processRewindEnd;
        //Object delegates
        Managers.Object.onObjectRecreated += Managers.Rewind.LoadObjectAndChildren;
        Managers.Object.onObjectRecreated +=
            (go, lastStateSeen) => Managers.Scene.registerObjectInScene(go);
        Managers.Object.onObjectRecreated +=
            (go, lastStateSeen) =>
            {
                //Don't load if it should actually not exist anymore
                SavableObjectInfo soi = go.GetComponent<SavableObjectInfo>();
                int gameStateId = Managers.Rewind.GameStateId;
                if (soi.spawnStateId > gameStateId)
                {
                    Debug.Log("Recreation of object " + go.name + "(" + soi.Id + ") " +
                        "is too late! Destroying permananetly. Created at " + soi.spawnStateId + " after " + gameStateId, go);
                    //(it's possible for an object recreation to be finished
                    //after it should have been rewound out of existence)
                    Managers.Object.destroyAndForgetObject(go);
                }
                else if (soi.destroyStateId < gameStateId)
                {
                    Debug.Log("Recreation of object " + go.name + "(" + soi.Id + ") " +
                        "is too early! Destroying. Destroyed at " + soi.destroyStateId + " before " + gameStateId, go);
                    //Destroy this object because it's still after it was originally destroyed
                    Managers.Object.destroyObject(go);
                }
                else if (soi.destroyStateId > gameStateId)
                {
                    //this object should no longer be destroyed
                    soi.destroyStateId = int.MaxValue;
                    Managers.Object.updateDestroyStateId(soi.Id, soi.destroyStateId);
                }
                else
                {
                    Debug.Log("Recreation of object " + go.name + "(" + soi.Id + ") " +
                        "is ok. GameState Id: " + Managers.Rewind.GameStateId, go);
                }
            };
        //File delegates
        Managers.File.onFileSave += Managers.Settings.saveSettings;
        Managers.File.onFileLoad += Managers.Settings.loadSettings;
        //NPC delegates
        Managers.NPC.onNPCSpeakingChanged += (speaking) => Managers.Music.Quiet = !speaking;
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
            }
            //Check to see if the camera rotation needs updated
            Vector2 camPos = Managers.Camera.transform.position;
            GravityZone gz = GravityZone.getGravityZone(camPos);
            if (gz && Vector2.Distance(gz.transform.position, camPos) >= CAM_ROTATE_MIN)
            {
                Managers.Camera.Up = (gz.radialGravity)
                    ? (camPos - (Vector2)gz.transform.position)
                    : (Vector2)gz.transform.up;
            }
        }
        Managers.Gesture.processGestures();
        Managers.Camera.updateCameraPosition();

        //If in demo mode,
        if (Managers.DemoMode.GameDemoLength > 0)
        {
            Managers.DemoMode.processDemoMode();
        }
    }

    /// <summary>
    /// Called once per physics update
    /// </summary>
    private void FixedUpdate()
    {
        //TODO: Refactor
        //Put the player ground trigger in its proper spot
        //Managers.Player.updateGroundTrigger();
    }

    #region Scene Load delegates
    void sceneLoaded(Scene scene)
    {
        if (scene.buildIndex == MenuManager.MENU_SCENE_ID)
        {
            if (Managers.Rewind.Rewinding)
            {
                //TODO: Get this back
                //Managers.Effect.showRewindEffect(false);
            }
        }
        if (Managers.Scene.isLevelScene(scene))
        {
            //Make sure the PlayerController has the right player
            Managers.Level.registerLevelGoalDelegates();
            onLevelChanged(Managers.Level.LevelInfo);
            //Load the previous state of the objects in the scene
            Managers.Scene.LoadObjectsFromScene(scene);
            //Refresh Memory Objects
            Managers.Object.refreshMemoryObjects();
            //If the game has just begun,
            if (Managers.Scene.SceneLoadingCount == 0)
            {
                Managers.Music.playFirstSong();
            }
            //Create the initial save state
            if (Managers.Rewind.GameStateCount == 0)
            {
                Managers.Rewind.Save();
            }
        }
    }
    void sceneUnloaded(Scene scene)
    {
        if (scene.buildIndex == MenuManager.MENU_SCENE_ID)
        {
            if (Managers.Rewind.Rewinding)
            {
                //TODO: Get this back
                //Managers.Effect.showRewindEffect(true);
            }
        }
        //Update the list of game objects to save
        Managers.Object.refreshGameObjects();
    }
    #endregion

    #region Level delegates
    void onLevelChanged(LevelInfo levelInfo)
    {
        Managers.Scene.getSceneLoader(levelInfo.sceneId).loadLevelIfUnLoaded();
        int stonicornId = levelInfo.stonicornId;
        if (Managers.Player.objectId != stonicornId)
        {
            Stonicorn stonicorn = FindObjectsOfType<Stonicorn>()
                .FirstOrDefault(stncrn =>
                    stncrn.GetComponent<SavableObjectInfo>().Id == stonicornId
                );
            if (stonicorn)
            {
                Managers.Player.Stonicorn = stonicorn;
                SceneLoader.ExplorerObject = stonicorn.gameObject;
                FindObjectsOfType<Follow>().ToList()
                    .ForEach(follow => follow.followObject = stonicorn.gameObject);
                FindObjectOfType<Follow>().Awake();
                Managers.Player.init();
            }
        }
    }
    void onLevelFinished()
    {
        Timer.startTimer(2, () =>
        {
            Managers.Rewind.RewindToStart();
            Managers.Level.CurrentLevelId++;
        });
    }
    #endregion

    #region Rewind Delegates

    void processRewindStart(int rewindStateId)
    {
        //Set the music speed to rewind
        Managers.Music.SongSpeed = Managers.Music.rewindSongSpeed;
        //Show rewind visual effect
        //TODO: Get this back
        //Managers.Effect.showRewindEffect(true);
        //Recenter the camera on Merky
        Managers.Camera.recenter();
        //Disable physics while rewinding
        Managers.Physics2DSurrogate.enabled = true;
        //Pause time
        Managers.Time.setPause(this, true);
        //Update Stats
        Managers.Stats.addOne(Stat.REWIND);
        //Prepare scenes
        Managers.Scene.prepareForRewind(rewindStateId);
    }
    void processRewindEnd(int rewindStateId)
    {
        //Refresh the game object list
        Managers.Object.LoadObjectsPostRewind(rewindStateId);
        if (Managers.Object.RecreatingObjects)
        {
            Managers.Object.onAllObjectsRecreated -= Managers.Object.refreshGameObjects;
            Managers.Object.onAllObjectsRecreated += Managers.Object.refreshGameObjects;
        }
        else
        {
            Managers.Object.refreshGameObjects();
        }
        //Put the music back to normal
        Managers.Music.SongSpeed = Managers.Music.normalSongSpeed;
        //Stop rewind visual effect
        //TODO: Get this back
        //Managers.Effect.showRewindEffect(false);
        //Unpause time
        Managers.Time.setPause(this, false);
        //Re-enable physics because the rewind is over
        Managers.Physics2DSurrogate.enabled = false;
        //Update SceneLoaders & scene object list
        Managers.Scene.updateSceneLoadersBackward(rewindStateId);
        Managers.Scene.updateSceneObjectList(rewindStateId);
        //Auto-Save file if rewound to beginning
        if (rewindStateId == 0)
        {
            Managers.File.saveToFile();
        }
    }
    #endregion


    //Sent to all GameObjects before the application is quit
    //Auto-save on exit
    void OnApplicationQuit()
    {
        //Save the game state and then
        Managers.Rewind.Save();
        //Save the game to file
        Managers.File.saveToFile();
    }
    private void OnApplicationPause(bool pause)
    {
        if (pause)
        {
            OnApplicationQuit();
        }
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
            Managers.Rewind.Save();
            Managers.File.saveToFile();
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

}


