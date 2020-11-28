using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneLoader : SavableMonoBehaviour
{
    private static SceneLoader currentScene;//the scene that Merky is currently in

    public string sceneName;//the name of the scene to load
    private Scene scene;
    public Scene Scene
    {
        get
        {
            if (!scene.IsValid())
            {
                scene = SceneManager.GetSceneByName(sceneName);
            }
            return scene;
        }
    }
    public int lastOpenGameStateId = -1;//the gamestate id in which this scene was last open in. -1 means it is not open in any of them
    public int firstOpenGameStateId = int.MaxValue;//the gamestate in which this scene was first opened (for rewind purposes)
    private static GameObject explorerObj;//object that enters and exits triggers, causing scenes to load / unload
    public static GameObject ExplorerObject
    {
        get
        {
            if (explorerObj == null)
            {
                ExplorerObject = Managers.Player.gameObject;
            }
            return explorerObj;
        }
        set
        {
            explorerObj = value;
            if (explorerObj)
            {
                explorer = explorerObj.GetComponent<Explorer>();
                if (!explorer)
                {
                    explorer = explorerObj.GetComponentInChildren<Explorer>();
                }
            }
            else
            {
                explorer = null;
            }
        }
    }
    private static Explorer explorer;
    [SerializeField]
    /// <summary>
    /// True if the level is currently loaded
    /// </summary>
    private bool IsLoaded
    {
        get
        {
            //2019-12-31: copied from https://forum.unity.com/threads/loading-an-additive-scene-once-and-only-once.395653/#post-2581612
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                if (SceneManager.GetSceneAt(i).name == sceneName)
                {
                    return true;
                }
            }
            return false;
        }
    }
    private Collider2D c2d;
    protected Collider2D Collider
    {
        get
        {
            if (c2d == null)
            {
                c2d = GetComponent<Collider2D>();
            }
            return c2d;
        }
    }

    // Use this for initialization
    void Start()
    {
        if (gameObject.name == "Easy Save 3 Loaded Component")
        {
            return;
        }
    }

    public void check()
    {
        if (!gameObject.activeSelf)
        {
            return;
        }
        bool isLoaded = IsLoaded;
        bool overlaps = Collider.OverlapPoint(ExplorerObject.transform.position);
        if (overlaps)
        {
            currentScene = this;
        }
        //Unload when player leaves
        if (isLoaded)
        {
            bool shouldUnload =
                (explorer)
                ? (Managers.Game.playerSceneLoaded && !explorer.canSeeBehind(Collider))
                : !overlaps;
            if (shouldUnload)
            {
                unloadLevel();
            }
        }
        //Load when player enters
        else if (!isLoaded)
        {
            bool shouldLoad =
                overlaps ||
                explorer.canSee(Collider);
            if (shouldLoad)
            {
                loadLevel();
            }
        }
        //If the player is in the level before it's done loading,
        if (overlaps && isLoaded && !Managers.Game.isSceneOpenByName(sceneName))
        {
            //Pause the game.
            Managers.Game.PauseForLoadingSceneName = sceneName;
        }
    }
    public bool isPositionInScene(Vector2 pos)
    {
        return Collider.OverlapPoint(pos);
    }
    void loadLevel()
    {
        LoadingScreen.LoadScene(sceneName);
    }
    void unloadLevel()
    {
        SceneManager.UnloadSceneAsync(sceneName);
    }
    public void loadLevelIfUnLoaded()
    {
        if (!IsLoaded)
        {
            loadLevel();
        }
    }
    public void unloadLevelIfLoaded()
    {
        if (IsLoaded)
        {
            unloadLevel();
        }
    }

    #region Static Helper Methods

    public static Scene getCurrentScene()
    {
        return currentScene.Scene;
    }
    /// <summary>
    /// Moves the given object to the current scene
    /// </summary>
    /// <param name="go"></param>
    public static void moveToCurrentScene(GameObject go)
    {
        moveToScene(go, getCurrentScene());
    }
    public static void moveToScene(GameObject go, Scene s)
    {
        SceneManager.MoveGameObjectToScene(go, s);
    }

    #endregion

    public override SavableObject getSavableObject()
    {
        return new SavableObject(this,
            "firstOpenGameStateId", firstOpenGameStateId,
            "lastOpenGameStateId", lastOpenGameStateId
            );
    }

    public override void acceptSavableObject(SavableObject savObj)
    {
        firstOpenGameStateId = (int)savObj.data["firstOpenGameStateId"];
        lastOpenGameStateId = (int)savObj.data["lastOpenGameStateId"];
    }
}