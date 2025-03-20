using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Managers : MonoBehaviour
{
    //
    // Game Data
    //

    private GameData gameData;
    public GameDataContainer gameDataContainer;

    //
    // Managers
    //

    [AutoInitialize(SearchScene = true), SerializeField]
    private List<Manager> managerList;

    //Game Manager
    [AutoInitialize(SearchScene = true), SerializeField, HideInInspector]
    private GameManager gameManager;
    public static GameManager Game => instance.gameManager;

    //Gesture Manager
    [AutoInitialize(SearchScene = true), SerializeField, HideInInspector]
    private GestureManager gestureManager;
    public static GestureManager Gesture => instance.gestureManager;

    //Dialogue Manager
    [AutoInitialize(SearchScene = true), SerializeField, HideInInspector]
    private DialogueManager dialogueManager;
    public static DialogueManager Dialogue => instance.dialogueManager;

    //Progress Manager
    private ProgressManager progressManager;
    public static ProgressManager Progress => instance.progressManager;

    //Event Manager
    //Used to store which NPC voicelines have been played
    [AutoInitialize(SearchScene = true), SerializeField, HideInInspector]
    private EventManager eventManager;
    public static EventManager Event => instance.eventManager;

    //Game Statistics
    //Keeps track of how many times everything has happened
    [AutoInitialize(SearchScene = true), SerializeField, HideInInspector]
    private GameStatistics gameStatistics;
    public static GameStatistics Stats => instance.gameStatistics;

    //Time Manager
    //Used to keep track of the time since the game began,
    //Taking into account time rewind and dilation
    [AutoInitialize(SearchScene = true), SerializeField, HideInInspector]
    private TimeManager timeManager;
    public static TimeManager Time => instance.timeManager;

    //Rewind Manager
    //Used to save and load gamestates,
    //Allowing for time to be rewound
    [AutoInitialize(SearchScene = true), SerializeField, HideInInspector]
    private RewindManager rewindManager;
    public static RewindManager Rewind => instance.rewindManager;

    //Object Manager
    //Manages the list of known objects
    [AutoInitialize(SearchScene = true), SerializeField, HideInInspector]
    private ObjectManager objectManager;
    public static ObjectManager Object => instance.objectManager;

    //Physics 2D Surrogate
    //Used to enable triggers while main physics is disabled during time rewind
    [AutoInitialize(SearchScene = true), SerializeField, HideInInspector]
    private Physics2DSurrogate physics2DSurrogate;
    public static Physics2DSurrogate Physics2DSurrogate => instance.physics2DSurrogate;

    //Music Manager
    [AutoInitialize(SearchScene = true), SerializeField, HideInInspector]
    private MusicManager musicManager;
    public static MusicManager Music => instance.musicManager;

    //Sound Manager
    [AutoInitialize(SearchScene = true), SerializeField, HideInInspector]
    private SoundManager soundManager;
    public static SoundManager Sound => instance.soundManager;

    //Video Manager
    [AutoInitialize(SearchScene = true), SerializeField, HideInInspector]
    private VideoManager videoManager;
    public static VideoManager Video => instance.videoManager;

    //Effect Manager
    [AutoInitialize(SearchScene = true), SerializeField, HideInInspector]
    private EffectManager effectManager;
    public static EffectManager Effect => instance.effectManager;

    //Scenes Manager
    [AutoInitialize(SearchScene = true), SerializeField, HideInInspector]
    private ScenesManager scenesManager;
    public static ScenesManager Scene => instance.scenesManager;

    //Menu Manager
    //[AutoInitialize(SearchScene = true), SerializeField, HideInInspector]
    private MenuManager menuManager;
    public static MenuManager Menu //TODO: update this later when menu is part of PlayerScene
    {
        get
        {
            if (instance.menuManager == null || ReferenceEquals(instance.menuManager, null))
            {
                instance.menuManager = FindAnyObjectByType<MenuManager>();
            }
            return instance.menuManager;
        }
    }

    //Settings Manager
    [AutoInitialize(SearchScene = true), SerializeField, HideInInspector]
    private SettingsManager settingsManager;
    public static SettingsManager Settings => instance.settingsManager;

    //File Manager
    [AutoInitialize(SearchScene = true), SerializeField, HideInInspector]
    private FileManager fileManager;
    public static FileManager File => instance.fileManager;

    //Demo Mode
    [AutoInitialize(SearchScene = true), SerializeField, HideInInspector]
    private DemoModeManager demoModeManager;
    public static DemoModeManager DemoMode => instance.demoModeManager;

    //Power Manager
    [AutoInitialize(SearchScene = true), SerializeField, HideInInspector]
    private PowerManager powerManager;
    public static PowerManager Power => instance.powerManager;

    //Skin Manager
    [AutoInitialize(SearchScene = true), SerializeField, HideInInspector]
    private SkinManager skinManager;
    public static SkinManager Skin => instance.skinManager;

    //
    // Controllers
    //

    //Player Controller
    [SerializeField]
    private PlayerController playerController;
    public static PlayerController Player => instance.playerController;
    [SerializeField, HideInInspector]
    private SingletonObjectInfo playerSingletonObjectInfo;
    public static SingletonObjectInfo PlayerSingletonObjectInfo => instance.playerSingletonObjectInfo;

    //Player Rewind Controller
    [AutoInitialize(SearchScene = true), SerializeField]
    private PlayerRewindController playerRewindController;
    public static PlayerRewindController PlayerRewind => instance.playerRewindController;

    //Player Pilot Controller
    [SerializeField]
    private PlayerPilotController playerPilotController;
    public static PlayerPilotController PlayerPilot
    {
        get => instance.playerPilotController;
        set => instance.playerPilotController = value;
    }

    //Camera Controller
    [AutoInitialize(SearchScene = true), SerializeField]
    private CameraController cameraController;
    public static CameraController Camera => instance.cameraController;

    //
    // Lists
    //

    //Checkpoints
    private List<CheckPointChecker> activeCheckPoints = new List<CheckPointChecker>();
    public static List<CheckPointChecker> ActiveCheckPoints
        => instance.activeCheckPoints;

    /// <summary>
    /// Saves the check point to the active check point list
    /// </summary>
    /// <param name="cpc"></param>
    public static void saveCheckPoint(CheckPointChecker cpc)
    {
        //If the list doesn't already contain the checkpoint,
        if (!ActiveCheckPoints.Contains(cpc))
        {
            //Add the checkpoint
            ActiveCheckPoints.Add(cpc);
        }
    }

    //
    // Singleton
    //
    private static Managers instance;
    public static void initInstance()
    {
        FindAnyObjectByType<Managers>().init();
    }
    void init()
    {
        if (instance != null && instance != this)
        {
            GameObject otherGO = instance.gameObject;
            Destroy(instance);
            Destroy(otherGO);
        }
        instance = this;

        //GameData
        gameData = gameDataContainer?.GameData;
        if (!gameData)
        {
            gameData = new GameData();
        }

        //ProgressManager
        progressManager = new ProgressManager();

        //Init with game data
        managerList
            .ForEach(m =>
            {
                m.init(gameData);
            });
    }

#if UNITY_EDITOR

    [Initializer(0)]
    private PlayerController init_playerController
        => FindObjectsByType<PlayerController>(FindObjectsSortMode.None).FirstOrDefault(pc => pc.gameObject.CompareTag("Player"));

    [Initializer(1)]
    private SingletonObjectInfo init_playerSingletonObjectInfo
        => playerController?.gameObject.GetComponent<SingletonObjectInfo>() ?? null;
#endif
}
