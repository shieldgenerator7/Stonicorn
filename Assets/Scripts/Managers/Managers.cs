using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Managers : MonoBehaviour
{
    //
    // Managers
    //

    //Game Manager
    private GameManager gameManager;
    public static GameManager Game
    {
        get
        {
            if (instance.gameManager == null)
            {
                instance.gameManager = FindObjectOfType<GameManager>();
            }
            return instance.gameManager;
        }
    }

    //Gesture Manager
    private GestureManager gestureManager;
    public static GestureManager Gesture
    {
        get
        {
            if (instance.gestureManager == null)
            {
                instance.gestureManager = FindObjectOfType<GestureManager>();
            }
            return instance.gestureManager;
        }
    }

    //NPC Manager
    private NPCManager npcManager;
    public static NPCManager NPC
    {
        get
        {
            if (instance.npcManager == null)
            {
                instance.npcManager = FindObjectOfType<NPCManager>();
                //If the NPCManager is still not found,
                if (instance.npcManager == null)
                {
                    //Get it from this gameobject 
                    instance.npcManager = instance.GetComponent<NPCManager>();
                }
            }
            return instance.npcManager;
        }
    }

    //Game Event Manager
    //Used to store which NPC voicelines have been played
    private GameEventManager gameEventManager;
    public static GameEventManager Event
    {
        get
        {
            if (instance.gameEventManager == null)
            {
                instance.gameEventManager = FindObjectOfType<GameEventManager>();
            }
            return instance.gameEventManager;
        }
    }

    //Physics 2D Surrogate
    //Used to enable triggers while main physics is disabled during time rewind
    private Physics2DSurrogate physics2DSurrogate;
    public static Physics2DSurrogate Physics2DSurrogate
    {
        get
        {
            if (instance.physics2DSurrogate == null)
            {
                instance.physics2DSurrogate = FindObjectOfType<Physics2DSurrogate>();
                //If the Physics2DSurrogate is still not found,
                if (instance.physics2DSurrogate == null)
                {
                    //Get it from this gameobject 
                    instance.physics2DSurrogate = instance.GetComponent<Physics2DSurrogate>();
                }
            }
            return instance.physics2DSurrogate;
        }
    }

    //Music Manager
    private MusicManager musicManager;
    public static MusicManager Music
    {
        get
        {
            if (instance.musicManager == null)
            {
                instance.musicManager = FindObjectOfType<MusicManager>();
            }
            return instance.musicManager;
        }
    }

    //Sound Manager
    private SoundManager soundManager;
    public static SoundManager Sound
    {
        get
        {
            if (instance.soundManager == null)
            {
                instance.soundManager = FindObjectOfType<SoundManager>();
            }
            return instance.soundManager;
        }
    }

    //Effect Manager
    private EffectManager effectManager;
    public static EffectManager Effect
    {
        get
        {
            if (instance.effectManager == null)
            {
                instance.effectManager = FindObjectOfType<EffectManager>();
            }
            return instance.effectManager;
        }
    }

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
    private SettingsManager settingsManager;
    public static SettingsManager Settings
    {
        get
        {
            if (instance.settingsManager == null)
            {
                instance.settingsManager = FindObjectOfType<SettingsManager>();
            }
            return instance.settingsManager;
        }
    }

    //
    // Controllers
    //

    //Player Controller
    private PlayerController playerController;
    public static PlayerController Player
    {
        get
        {
            if (instance.playerController == null)
            {
                instance.playerController = FindObjectOfType<PlayerController>();
            }
            return instance.playerController;
        }
    }

    //Camera Controller
    private CameraController cameraController;
    public static CameraController Camera
    {
        get
        {
            if (instance.cameraController == null)
            {
                instance.cameraController = FindObjectOfType<CameraController>();
            }
            return instance.cameraController;
        }
    }

    //
    // Lists
    //

    //Checkpoints
    private List<CheckPointChecker> activeCheckPoints = new List<CheckPointChecker>();
    public static List<CheckPointChecker> ActiveCheckPoints
    {
        get { return instance.activeCheckPoints; }
    }
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
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            GameObject otherGO = instance.gameObject;
            Destroy(instance);
            Destroy(otherGO);
        }
        instance = this;
    }
}
