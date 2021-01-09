using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneLoader : MonoBehaviour, ISetting
{
    public string sceneName;//the name of the scene to load, not actually used in code
    public int sceneId = -1;//the build index of the scene
    private Scene scene;
    public Scene Scene
    {
        get
        {
            if (scene.buildIndex < 0)
            {
                scene = SceneManager.GetSceneByBuildIndex(sceneId);
            }
            return scene;
        }
    }
    public int lastOpenGameStateId = -1;//the gamestate id in which this scene was last open in. -1 means it is not open in any of them
    public int firstOpenGameStateId = int.MaxValue;//the gamestate in which this scene was first opened (for rewind purposes)
    private static GameObject explorerObj;//object that enters and exits triggers, causing scenes to load / unload
    public static GameObject ExplorerObject
    {
        get => explorerObj;
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

    private bool isLoading = false;
    public bool IsLoading => isLoading && !scene.isLoaded;

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
                if (SceneManager.GetSceneAt(i).buildIndex == sceneId)
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

    public void check()
    {
        bool isLoaded = IsLoaded;
        bool overlaps = Collider.OverlapPoint(ExplorerObject.transform.position);
        //Unload when player leaves
        if (isLoaded)
        {
            bool shouldUnload = !explorer.canSeeBehind(Collider);
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
        if (overlaps && isLoaded && !Managers.Scene.isSceneOpen(sceneId))
        {
            //Pause the game.
            Managers.Scene.PauseForLoadingSceneId = sceneId;
        }
    }
    public bool isPositionInScene(Vector2 pos)
    {
        return Collider.OverlapPoint(pos);
    }
    void loadLevel()
    {
        isLoading = true;
        LoadingScreen.LoadScene(sceneId);
    }
    void unloadLevel()
    {
        isLoading = false;
        SceneManager.UnloadSceneAsync(sceneId);
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

    public delegate void OnObjectMoved(GameObject go);

    private void OnTriggerEnter2D(Collider2D coll2D)
    {
        GameObject go = coll2D.gameObject;
        onObjectEntered?.Invoke(go);
    }
    public event OnObjectMoved onObjectEntered;
    private void OnTriggerExit2D(Collider2D coll2D)
    {
        GameObject go = coll2D.gameObject;
        onObjectExited?.Invoke(go);
    }
    public event OnObjectMoved onObjectExited;

    public bool overlapsPosition(GameObject go)
        => Collider.OverlapPoint(go.transform.position);

    public bool overlapsCollider(GameObject go)
        => Collider.OverlapsCollider(go.GetComponent<Collider2D>());

    public SettingObject Setting
    {
        get => new SettingObject(ID,
            "firstOpenGameStateId", firstOpenGameStateId,
            "lastOpenGameStateId", lastOpenGameStateId
            );
        set
        {
            firstOpenGameStateId = (int)value.data["firstOpenGameStateId"];
            lastOpenGameStateId = (int)value.data["lastOpenGameStateId"];
        }
    }

    public SettingScope Scope => SettingScope.SAVE_FILE;

    public string ID
    {
        get => GetType().Name;
    }
}