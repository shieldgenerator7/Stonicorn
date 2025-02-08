using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Managers : MonoBehaviour, ISetupable
{
    //
    // Game Data
    //

    private GameData gameData;
    public GameDataContainer gameDataContainer;

    //
    // Managers
    //

    [SerializeField]
    private List<Manager> managerList = new List<Manager>();

    //Game Manager
    [SerializeField]
    private GameManager gameManager;
    public static GameManager Game => instance.gameManager;

    //Gesture Manager
    [SerializeField]
    private GestureManager gestureManager;
    public static GestureManager Gesture =>instance.gestureManager;

    //Dialogue Manager
    [SerializeField]
    private DialogueManager dialogueManager;
    public static DialogueManager Dialogue =>instance.dialogueManager;

    //Progress Manager
    [SerializeField]
    private ProgressManager progressManager;
    public static ProgressManager Progress =>instance.progressManager;

    //Event Manager
    //Used to store which NPC voicelines have been played
    [SerializeField]
    private EventManager eventManager;
    public static EventManager Event => instance.eventManager;

    //Game Statistics
    //Keeps track of how many times everything has happened
    [SerializeField]
    private GameStatistics gameStatistics;
    public static GameStatistics Stats =>instance.gameStatistics;

    //Time Manager
    //Used to keep track of the time since the game began,
    //Taking into account time rewind and dilation
    [SerializeField]
    private TimeManager timeManager;
    public static TimeManager Time => instance.timeManager;

    //Rewind Manager
    //Used to save and load gamestates,
    //Allowing for time to be rewound
    [SerializeField]
    private RewindManager rewindManager;
    public static RewindManager Rewind =>instance.rewindManager;

    //Object Manager
    //Manages the list of known objects
    [SerializeField]
    private ObjectManager objectManager;
    public static ObjectManager Object => instance.objectManager;

    //Physics 2D Surrogate
    //Used to enable triggers while main physics is disabled during time rewind
    [SerializeField]
    private Physics2DSurrogate physics2DSurrogate;
    public static Physics2DSurrogate Physics2DSurrogate => instance.physics2DSurrogate;

    //Music Manager
    [SerializeField]
    private MusicManager musicManager;
    public static MusicManager Music => instance.musicManager;

    //Sound Manager
    [SerializeField]
    private SoundManager soundManager;
    public static SoundManager Sound => instance.soundManager;

    //Video Manager
    [SerializeField]
    private VideoManager videoManager;
    public static VideoManager Video => instance.videoManager;

    //Effect Manager
    [SerializeField]
    private EffectManager effectManager;
    public static EffectManager Effect => instance.effectManager;

    //Scenes Manager
    [SerializeField]
    private ScenesManager scenesManager;
    public static ScenesManager Scene => instance.scenesManager;

    //Menu Manager
    [SerializeField]
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
    [SerializeField]
    private SettingsManager settingsManager;
    public static SettingsManager Settings => instance.settingsManager;

    //File Manager
    [SerializeField]
    private FileManager fileManager;
    public static FileManager File => instance.fileManager;

    //Demo Mode
    [SerializeField]
    private DemoModeManager demoModeManager;
    public static DemoModeManager DemoMode => instance.demoModeManager;

    //Power Manager
    [SerializeField]
    private PowerManager powerManager;
    public static PowerManager Power => instance.powerManager;

    //
    // Controllers
    //

    //Player Controller
    [SerializeField]
    private PlayerController playerController;
    public static PlayerController Player => instance.playerController;

    //Player Rewind Controller
    [SerializeField]
    private PlayerRewindController playerRewindController;
    public static PlayerRewindController PlayerRewind => instance.playerRewindController;

    //Player Pilot Controller
    [SerializeField]
    private PlayerPilotController playerPilotController;
    public static PlayerPilotController PlayerPilot{
    get=> instance.playerPilotController;
        set=> instance.playerPilotController = value;
    }

    //Camera Controller
    [SerializeField]
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

        //Init with game data
        managerList
            .ForEach(m =>
            {
                m.init(gameData);
            });
    }

#if UNITY_EDITOR
    public int setup()
    {
        managerList = FindObjectsByType<Manager>(FindObjectsSortMode.None).ToList();

        //Populate other managers
        gameManager = FindAnyObjectByType<GameManager>();
        gestureManager = FindAnyObjectByType<GestureManager>();
        dialogueManager = FindAnyObjectByType<DialogueManager>();
        progressManager = new ProgressManager();
        eventManager = FindAnyObjectByType<EventManager>();
        gameStatistics = FindAnyObjectByType<GameStatistics>();
        timeManager = FindAnyObjectByType<TimeManager>();
        rewindManager = FindAnyObjectByType<RewindManager>();
        objectManager = FindAnyObjectByType<ObjectManager>();
        physics2DSurrogate = FindAnyObjectByType<Physics2DSurrogate>();
        musicManager = FindAnyObjectByType<MusicManager>();
        soundManager = FindAnyObjectByType<SoundManager>();
        videoManager = FindAnyObjectByType<VideoManager>();
        effectManager = FindAnyObjectByType<EffectManager>();
        scenesManager = FindAnyObjectByType<ScenesManager>();
        settingsManager = FindAnyObjectByType<SettingsManager>();
        fileManager = FindAnyObjectByType<FileManager>();
        demoModeManager = FindAnyObjectByType<DemoModeManager>();
        powerManager = FindAnyObjectByType<PowerManager>();
        playerController = FindObjectsByType<PlayerController>(FindObjectsSortMode.None).First(pc => pc.gameObject.CompareTag("Player"));
        playerRewindController = FindAnyObjectByType<PlayerRewindController>();
        cameraController = FindAnyObjectByType<CameraController>();

        return 0;//TODO: check to see if anything changed
    }
#endif
}
