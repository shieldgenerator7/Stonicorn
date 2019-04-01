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
            }
            return instance.npcManager;
        }
    }

    //Game Event Manager
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
    // Singleton
    //
    private static Managers instance;
    private void Awake()
    {
        if (instance != null)
        {
            Destroy(instance.gameObject);
        }
        instance = this;
    }
}
