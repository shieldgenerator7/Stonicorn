using System.Collections;
using System.Collections.Generic;
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
                instance.menuManager = FindObjectOfType<MenuManager>();
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
        FindObjectOfType<Managers>().init();
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
        Game = FindObjectOfType<GameManager>();
        Gesture = FindObjectOfType<GestureManager>();
        Dialogue = GetComponent<DialogueManager>();
        Progress = new ProgressManager();
        Event = FindObjectOfType<EventManager>();
        Stats = FindObjectOfType<GameStatistics>();
        Time = FindObjectOfType<TimeManager>();
        Rewind = FindObjectOfType<RewindManager>();
        Object = FindObjectOfType<ObjectManager>();
        Physics2DSurrogate = GetComponent<Physics2DSurrogate>();
        Music = FindObjectOfType<MusicManager>();
        Sound = FindObjectOfType<SoundManager>();
        Video = FindObjectOfType<VideoManager>();
        Effect = FindObjectOfType<EffectManager>();
        Scene = FindObjectOfType<ScenesManager>();
        Settings = FindObjectOfType<SettingsManager>();
        File = FindObjectOfType<FileManager>();
        DemoMode = FindObjectOfType<DemoModeManager>();
        Power = FindObjectOfType<PowerManager>();
        Player = FindObjectOfType<PlayerController>();
        PlayerRewind = FindObjectOfType<PlayerRewindController>();
        Camera = FindObjectOfType<CameraController>();

        //Init with game data
        foreach (Manager m in FindObjectsOfType<Manager>())
        {
            m.init(gameData);
        }
    }
}
