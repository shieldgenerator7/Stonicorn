using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneLoader : MonoBehaviour
{
    private static SceneLoader currentScene;//the scene that Merky is currently in

    public string sceneName;//the name of the scene to load
    private Scene scene;
    public Scene Scene
    {
        get
        {
            if (scene == null)
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
    private bool isLoaded = false;
    internal bool IsLoaded
    {
        get => isLoaded;
        set
        {
            isLoaded = value;
            if (isLoaded)
            {
                isLoading = false;
            }
            else
            {
                isUnloading = false;
            }
        }
    }
    [SerializeField]
    /// <summary>
    /// True if the level is currently loaded or is currently loading
    /// </summary>
    private bool isLoading = false;
    [SerializeField]
    /// <summary>
    /// True if the level is currently unloaded or is currently unloaded
    /// </summary>
    private bool isUnloading = false;
    private Collider2D c2d;

    // Use this for initialization
    void Start()
    {
        if (gameObject.name == "Easy Save 3 Loaded Component")
        {
            return;
        }
        c2d = gameObject.GetComponent<Collider2D>();
        IsLoaded = SceneManager.GetSceneByName(sceneName).isLoaded;
    }

    public void check()
    {
        if (!gameObject.activeSelf)
        {
            return;
        }
        bool overlaps = c2d.OverlapPoint(ExplorerObject.transform.position);
        if (overlaps)
        {
            currentScene = this;
        }
        //Unload when player leaves
        if ((IsLoaded || isLoading) && !isUnloading)
        {
            bool shouldUnload =
                (explorer)
                ? !explorer.canSeeBehind(c2d)
                : !overlaps;
            if (shouldUnload)
            {
                unloadLevel();
            }
        }
        //Load when player enters
        else if ((!IsLoaded || isUnloading) && !isLoading)
        {
            bool shouldLoad =
                (explorer)
                ? explorer.canSee(c2d)
                : overlaps;
            if (shouldLoad)
            {
                loadLevel();
            }
        }
        //If the player is in the level before it's done loading,
        if (overlaps && !IsLoaded && isLoading)
        {
            //Pause the game.
            Managers.Game.PauseForLoadingSceneName = sceneName;
        }
    }
    public bool isPositionInScene(Vector2 pos)
    {
        return c2d.OverlapPoint(pos);
    }
    void loadLevel()
    {
        isLoading = true;
        isUnloading = false;
        LoadingScreen.LoadScene(sceneName);
    }
    void unloadLevel()
    {
        isLoading = false;
        isUnloading = true;
        SceneManager.UnloadSceneAsync(sceneName);
    }
    public void loadLevelIfUnLoaded()
    {
        if ((!IsLoaded || isUnloading) && !isLoading)
        {
            loadLevel();
        }
    }
    public void unloadLevelIfLoaded()
    {
        if ((IsLoaded || isLoading) && !isUnloading)
        {
            unloadLevel();
        }
    }

    #region Static Helper Methods

    public static Scene getCurrentScene()
    {
        return SceneManager.GetSceneByName(currentScene.sceneName);
    }
    /// <summary>
    /// Moves the given object to the current scene
    /// </summary>
    /// <param name="go"></param>
    public static void moveToCurrentScene(GameObject go)
    {
        SceneManager.MoveGameObjectToScene(go, getCurrentScene());
    }

    #endregion
}