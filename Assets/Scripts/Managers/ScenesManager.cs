using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScenesManager : SavableMonoBehaviour
{
    [SerializeField]
    private List<SceneLoader> sceneLoaders = new List<SceneLoader>();
    /// <summary>
    /// Stores the object's id and the scene id of the scene that it's in
    /// </summary>
    private Dictionary<int, int> objectSceneList = new Dictionary<int, int>();

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

    public override void init()
    {
#if UNITY_EDITOR
        //Add list of already open scenes to open scene list (for editor)
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (!openScenes.Contains(scene))
            {
                openScenes.Add(scene);
            }
        }
        if (openScenes.Find(scene => scene.name == "PlayerScene") != null)
        {
            playerSceneLoaded = true;
        }
#endif

        //Register scene loading delegates
        SceneManager.sceneLoaded -= sceneLoaded;
        SceneManager.sceneLoaded += sceneLoaded;
        SceneManager.sceneUnloaded -= sceneUnloaded;
        SceneManager.sceneUnloaded += sceneUnloaded;

        //Register SceneLoaderSavableList delegates
        sceneLoaders.ForEach(sl =>
        {
            sl.onObjectEntered -= registerObjectInScene;
            sl.onObjectEntered += registerObjectInScene;
            sl.onObjectExited -= registerObjectInScene;
            sl.onObjectExited += registerObjectInScene;
        });
    }

    #region Space Management
    public void checkScenes()
    {
        foreach (SceneLoader sl in sceneLoaders)
        {
            sl.check();
        }
    }

    public bool isLevelScene(Scene s)
        => getSceneLoader(s) != null;

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
        if (isLevelScene(scene))
        {
            //And it's the scene that we paused the game for,
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
    public event OnSceneLoadedChanged onSceneLoaded;
    public event OnSceneLoadedChanged onSceneUnloaded;

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
        //Get list of savables
        List<GameObject> sceneGOs = SceneSavableList.getFromScene(scene).savables;
        //If an object from this scene is known not to be currently in this scene,
        List<GameObject> unsceneGOs = sceneGOs.FindAll(
            go => !isObjectInScene(go, scene)
            );
        //Remove it from being processed, and
        sceneGOs.RemoveAll(go => unsceneGOs.Contains(go));
        //Destroy it before it gets put into the game object list.
        unsceneGOs.ForEach(go =>
        {
            Debug.Log("Destroying now duplicate: " + go);
            Destroy(go);
        });
        //Add objects to object list
        sceneGOs.ForEach(go => Managers.Object.addObject(go));
        //Init the savables
        sceneGOs.ForEach(
            go => go.GetComponents<SavableMonoBehaviour>().ToList()
                .ForEach(smb => smb.init())
            );
        //Find the last state that this scene was saved in
        int lastStateSeen = -1;
        SceneLoader sceneLoader = sceneLoaders.Find(sl => sl.Scene == scene);
        if (sceneLoader)
        {
            lastStateSeen = sceneLoader.lastOpenGameStateId;
        }
        //If the scene was last seen after gamestate-now,
        //The scene is now last seen gamestate-now
        lastStateSeen = Mathf.Min(lastStateSeen, Managers.Rewind.GameStateId);
        //If this scene has been open before,
        if (lastStateSeen > 0)
        {
            //Load the objects
            Managers.Rewind.LoadObjects(
                sceneGOs,
                lastStateSeen
                );
        }
        //Create foreign objects that are not here
        List<int> sceneIds = sceneGOs.ConvertAll(go => go.getKey());
        getObjectsIdsInScene(scene)
            .FindAll(id => !sceneIds.Contains(id))
            .FindAll(id => !Managers.Object.hasObject(id))
            .ForEach(id => Managers.Object.createObject(id, lastStateSeen));
    }

    private SceneLoader getSceneLoader(Scene scene)
        => sceneLoaders.Find(sl => sl.Scene == scene);
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

    public List<GameObject> getObjectsInScene(Scene scene)
    {
        int sceneId = scene.buildIndex;
        return objectSceneList.ToList()
            .FindAll(entry => entry.Value == sceneId)
            .ConvertAll(entry => Managers.Object.getObject(entry.Key));
    }
    public List<int> getObjectsIdsInScene(Scene scene)
    {
        int sceneId = scene.buildIndex;
        return objectSceneList.ToList()
            .FindAll(entry => entry.Value == sceneId)
            .ConvertAll(entry => entry.Key);
    }

    public bool isObjectInScene(GameObject go, Scene scene)
    {
        int objectId = go.getKey();
        if (objectSceneList.ContainsKey(objectId))
        {
            return objectSceneList[objectId] == scene.buildIndex;
        }
        return true;
    }

    public void registerObjectInScene(GameObject go)
    {
        if (go == null || ReferenceEquals(go, null))
        {
            //don't register null or destroyed objects
            Debug.LogWarning("GameObject " + go.name + " is destroyed and will not be processed.");
            return;
        }
        if (!go.activeInHierarchy)
        {
            //don't register inactive (possibly null or destroyed) objects
            Debug.LogWarning("GameObject " + go.name + " is inactive and will not be processed.");
            return;
        }
        SavableObjectInfo soi = go.GetComponent<SavableObjectInfo>();
        //Don't add non-Savable or Singleton objects ever
        if (!soi || soi is SingletonObjectInfo)
        {
            return;
        }
        //Add it to the list that contains the position
        int objectId = soi.Id;
        if (objectId < 0)
        {
            Debug.LogError(
                "Object " + go.name + " Id is less than 0! id: " + objectId,
                go
                );
            return;
        }
        SceneLoader loader = sceneLoaders.Find(sl => sl.overlapsPosition(go));
        if (!loader)
        {
            loader = sceneLoaders.Find(sl => sl.overlapsCollider(go));
        }
        try
        {
            Scene scene = loader.Scene;
            int sceneId = scene.buildIndex;
            if (!objectSceneList.ContainsKey(objectId))
            {
                if (objectId == 0)
                {
                    Debug.LogError(
                        "Trying to add object Id " + objectId + " to the objectScenesList!",
                        go
                        );
                }
                objectSceneList.Add(objectId, sceneId);
            }
            else
            {
                objectSceneList[objectId] = sceneId;
            }
            moveToScene(go, scene);
        }
        catch (NullReferenceException nre)
        {
            throw new NullReferenceException(
                "No SL found when trying to register object " + go.name
                + " at position " + go.transform.position,
                nre
                );
        }
        catch (ArgumentException ae)
        {
            Debug.LogError(
                "No scene found when trying to register object " + go.name
                + " at position " + go.transform.position
                + "\nArgumentException: " + ae,
                go
                );
        }
    }

    public void moveToScene(GameObject go, Scene scene)
    {
        try
        {
            if (go.transform.parent != null)
            {
                Rigidbody2D rb2d = go.GetComponent<Rigidbody2D>();
                if (!rb2d)
                {
                    rb2d = go.GetComponentInParent<Rigidbody2D>();
                }
                if (rb2d)
                {
                    go = rb2d.gameObject;
                }
                if (go.transform.parent != null)
                {
                    go.transform.SetParent(null);
                }
            }
            if (go.scene != scene)
            {
                Debug.Log("Moving " + go.name + " into scene " + scene.name, go);
                SceneManager.MoveGameObjectToScene(go, scene);
                Debug.Log("Moved " + go.name + " is now in scene " + go.scene.name, go);
            }
        }
        catch (System.ArgumentException ae)
        {
            Debug.LogError(
                "Trying to move " + go.name + " into scene " + scene.name
                + " at position: " + go.transform.position
                + "\nArgumentException: " + ae
                );
        }
    }

    public void removeObject(GameObject go)
    {
        objectSceneList.Remove(go.getKey());
    }

    public override SavableObject CurrentState
    {
        get => new SavableObject(this)
            .addDictionary(
            "objectSceneList", objectSceneList
            );
        set
        {
            objectSceneList = value.Dictionary<int, int>("objectSceneList");
        }
    }
}
