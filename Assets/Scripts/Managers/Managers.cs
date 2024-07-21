using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Managers : MonoBehaviour
{
    //
    // Game Data
    //

    public GameData gameData;

    //
    // Managers
    //

    //Game Manager
    public static GameManager Game { get; private set; }

    //Gesture Manager
    public static GestureManager Gesture { get; private set; }

    //Dialogue Manager
    public static DialogueManager Dialogue { get; private set; }

    //Progress Manager
    public static ProgressManager Progress { get; private set; }

    //Event Manager
    //Used to store which NPC voicelines have been played
    public static EventManager Event { get; private set; }

    //Game Statistics
    //Keeps track of how many times everything has happened
    public static GameStatistics Stats { get; private set; }

    //Time Manager
    //Used to keep track of the time since the game began,
    //Taking into account time rewind and dilation
    public static TimeManager Time { get; private set; }

    //Rewind Manager
    //Used to save and load gamestates,
    //Allowing for time to be rewound
    public static RewindManager Rewind { get; private set; }

    //Object Manager
    //Manages the list of known objects
    public static ObjectManager Object { get; private set; }

    //Physics 2D Surrogate
    //Used to enable triggers while main physics is disabled during time rewind
    public static Physics2DSurrogate Physics2DSurrogate { get; private set; }

    //Music Manager
    public static MusicManager Music { get; private set; }

    //Sound Manager
    public static SoundManager Sound { get; private set; }

    //Video Manager
    public static VideoManager Video { get; private set; }

    //Effect Manager
    public static EffectManager Effect { get; private set; }

    //Scenes Manager
    public static ScenesManager Scene { get; private set; }

    //Menu Manager
    private MenuManager menuManager;
    public static MenuManager Menu
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
    public static SettingsManager Settings { get; private set; }

    //File Manager
    public static FileManager File { get; private set; }

    //Demo Mode
    public static DemoModeManager DemoMode { get; private set; }

    //Power Manager
    public static PowerManager Power { get; private set; }

    //
    // Controllers
    //

    //Player Controller
    public static PlayerController Player { get; private set; }

    //Player Rewind Controller
    public static PlayerRewindController PlayerRewind { get; private set; }

    //Player Pilot Controller
    public static PlayerPilotController PlayerPilot { get; private set; }

    //Camera Controller
    public static CameraController Camera { get; private set; }

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
        if (!gameData)
        {
            gameData = new GameData();
        }

        //Populate other managers
        Game = FindAnyObjectByType<GameManager>();
        Gesture = FindAnyObjectByType<GestureManager>();
        Dialogue = FindAnyObjectByType<DialogueManager>();
        Progress = new ProgressManager();
        Event = FindAnyObjectByType<EventManager>();
        Stats = FindAnyObjectByType<GameStatistics>();
        Time = FindAnyObjectByType<TimeManager>();
        Rewind = FindAnyObjectByType<RewindManager>();
        Object = FindAnyObjectByType<ObjectManager>();
        Physics2DSurrogate = FindAnyObjectByType<Physics2DSurrogate>();
        Music = FindAnyObjectByType<MusicManager>();
        Sound = FindAnyObjectByType<SoundManager>();
        Video = FindAnyObjectByType<VideoManager>();
        Effect = FindAnyObjectByType<EffectManager>();
        Scene = FindAnyObjectByType<ScenesManager>();
        Settings = FindAnyObjectByType<SettingsManager>();
        File = FindAnyObjectByType<FileManager>();
        DemoMode = FindAnyObjectByType<DemoModeManager>();
        Power = FindAnyObjectByType<PowerManager>();
        Player = FindObjectsByType<PlayerController>(FindObjectsSortMode.None).First(pc=>pc.gameObject.CompareTag("Player"));
        PlayerRewind = FindAnyObjectByType<PlayerRewindController>();
        PlayerPilot = FindAnyObjectByType<PlayerPilotController>();
        Camera = FindAnyObjectByType<CameraController>();

        //Init with game data
        FindObjectsByType<Manager>(FindObjectsSortMode.None)
            .ToList()
            .ForEach(m =>
            {
            m.init(gameData);
            });
    }
}
