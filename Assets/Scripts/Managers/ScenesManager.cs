using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScenesManager : MonoBehaviour
{
    [SerializeField]
    private List<SceneLoader> sceneLoaders = new List<SceneLoader>();

    private string pauseForLoadingSceneName = null;//the name of the scene that needs the game to pause while it's loading
    public string PauseForLoadingSceneName
    {
        get => pauseForLoadingSceneName;
        set
        {
            pauseForLoadingSceneName = value;
            if (pauseForLoadingSceneName == null || pauseForLoadingSceneName == "")
            {
                //Resume if the scene is done loading
                Managers.Time.setPause(this, false);
            }
            else
            {
                //Pause if the scene is still loading
                Managers.Time.setPause(this, true);
            }
        }
    }

    //
    // Runtime Lists
    //

    //Scene Loading
    private List<Scene> openScenes = new List<Scene>();//the list of the scenes that are open
    public bool playerSceneLoaded { get; private set; } = false;

    public void init()
    {
#if UNITY_EDITOR
        //Add list of already open scenes to open scene list (for editor)
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            openScenes.Add(SceneManager.GetSceneAt(i));
        }
        if (openScenes.Find(scene => scene.name == "PlayerScene") != null)
        {
            playerSceneLoaded = true;
        }
#endif

        //Register scene loading delegates
        SceneManager.sceneLoaded += sceneLoaded;
        SceneManager.sceneUnloaded += sceneUnloaded;
    }

    #region Space Management
    public void checkScenes()
    {
        foreach (SceneLoader sl in sceneLoaders)
        {
            sl.check();
        }
    }

    void sceneLoaded(Scene scene, LoadSceneMode m)
    {
        if (scene.name == "PlayerScene")
        {
            playerSceneLoaded = true;
        }
        //Add the given scene to list of open scenes
        openScenes.Add(scene);
        //Scene Loaded Delegate
        onSceneLoaded?.Invoke(scene);
        //If its a level scene,
        SceneLoader sceneLoader = getSceneLoaderByName(scene.name);
        if (sceneLoader)
        {
            if (scene.name == PauseForLoadingSceneName)
            {
                //Unpause the game
                PauseForLoadingSceneName = null;
            }
        }
    }
    void sceneUnloaded(Scene scene)
    {
        //Scene Unloaded Delegate
        onSceneUnloaded?.Invoke(scene);
        //Remove the scene from the list of open scenes
        openScenes.Remove(scene);
    }
    public delegate void OnSceneLoadedChanged(Scene scene);
    public OnSceneLoadedChanged onSceneLoaded;
    public OnSceneLoadedChanged onSceneUnloaded;

    public bool isSceneOpen(Scene scene)
    {
        foreach (Scene s in openScenes)
        {
            if (scene == s)
            {
                return true;
            }
        }
        return false;
    }

    public bool isSceneOpen(string sceneName)
    {
        foreach (Scene s in openScenes)
        {
            if (sceneName == s.name)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Restores the objects in the scene to their previous state before the scene was unloaded
    /// </summary>
    /// <param name="scene">The scene whose objects need their state stored</param>
    public void LoadObjectsFromScene(Scene scene)
    {
        //Find the last state that this scene was saved in
        int lastStateSeen = -1;
        SceneLoader sceneLoader = sceneLoaders.Find(sl => sl.Scene == scene);
        if (sceneLoader)
        {
            lastStateSeen = sceneLoader.lastOpenGameStateId;
        }

#if UNITY_EDITOR
        Logger.log(this, "LOFS: Scene " + scene.name + ": last state seen: " + lastStateSeen);
#endif
        //If the scene was never seen,
        if (lastStateSeen < 0)
        {
            //Don't restore its objects' states,
            //because there's nothing to restore
            return;
        }
        //If the scene was last seen after gamestate-now,
        //The scene is now last seen gamestate-now
        lastStateSeen = Mathf.Min(lastStateSeen, Managers.Rewind.GameStateId);
        //Load the objects
        Managers.Rewind.LoadObjects(
            scene.name,
            lastStateSeen,
            go => go.scene == scene
            );
    }

    private SceneLoader getSceneLoaderByName(string sceneName)
    {
        foreach (SceneLoader sl in sceneLoaders)
        {
            if (sl.sceneName == sceneName)
            {
                return sl;
            }
        }
        return null;
    }
    #endregion

    public void updateSceneLoadersForward(int gameStateId)
    {
        //Open Scenes
        foreach (SceneLoader sl in sceneLoaders)
        {
            //If the scene loader's scene is open,
            if (openScenes.Contains(sl.Scene))
            {
                //And it hasn't been open in any previous game state,
                if (sl.firstOpenGameStateId > gameStateId)
                {
                    //It's first opened in this game state
                    sl.firstOpenGameStateId = gameStateId;
                }
                //It's also last opened in this game state
                sl.lastOpenGameStateId = gameStateId;
            }
        }
    }
    public void prepareForRewind(List<GameState> gameStates, int rewindStateId)
    {
        //Load levels that Merky will be passing through
        foreach (SceneLoader sl in sceneLoaders)
        {
            if (sl.firstOpenGameStateId <= gameStates.Count
                && sl.lastOpenGameStateId >= rewindStateId)
            {
                for (int i = gameStates.Count - 1; i >= rewindStateId; i--)
                {
                    if (sl.isPositionInScene(gameStates[i].Merky.position))
                    {
                        sl.loadLevelIfUnLoaded();
                        break;
                    }
                }
            }
        }
    }
    public void updateSceneLoadersBackward(int gameStateId)
    {
        //Update Scene tracking variables
        foreach (SceneLoader sl in sceneLoaders)
        {
            //If the scene was last opened after game-state-now,
            if (sl.lastOpenGameStateId > gameStateId)
            {
                //it is now last opened game-state-now
                sl.lastOpenGameStateId = gameStateId;
            }
            //if the scene was first opened after game-state-now,
            if (sl.firstOpenGameStateId > gameStateId)
            {
                //it is now never opened
                sl.firstOpenGameStateId = int.MaxValue;
                sl.lastOpenGameStateId = -1;
            }
        }
    }
}
