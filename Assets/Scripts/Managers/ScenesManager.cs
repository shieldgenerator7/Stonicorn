using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScenesManager : Manager
{
    [SerializeField]
    private List<SceneLoader> sceneLoaders = new List<SceneLoader>();

    private int pauseForLoadingSceneId = -1;//the id of the scene that is currently loading
    public int PauseForLoadingSceneId
    {
        get => pauseForLoadingSceneId;
        set
        {
            pauseForLoadingSceneId = value;
            onPauseForLoadingSceneIdChanged?.Invoke(pauseForLoadingSceneId);
        }
    }
    public delegate void OnPauseForLoadingSceneIdChanged(int id);
    public event OnPauseForLoadingSceneIdChanged onPauseForLoadingSceneIdChanged;

    //
    // Runtime Lists
    //

    //Scene Loading
    private List<Scene> openScenes = new List<Scene>();//the list of the scenes that are open

    public void init()
    {
        base.init(data);
#if UNITY_EDITOR
        //Add list of already open scenes to open scene list (for editor)
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (!openScenes.Contains(scene))
            {
                openScenes.Add(scene);
            }
            if (isLevelScene(scene))
            {
                LoadObjectsFromScene(scene);
            }
        }
#endif

        //Register scene loading delegates
        SceneManager.sceneLoaded -= sceneLoaded;
        SceneManager.sceneLoaded += sceneLoaded;
        SceneManager.sceneUnloaded -= sceneUnloaded;
        SceneManager.sceneUnloaded += sceneUnloaded;

        //Register SceneLoaderSavableList delegates
        SceneLoader.ExplorerObject = Managers.Player.gameObject;
        sceneLoaders.RemoveAll(sl => !sl.enabled || !sl.gameObject.activeSelf);
        sceneLoaders.ForEach(sl =>
        {
            sl.onObjectEntered -= registerObjectInScene;
            sl.onObjectEntered += registerObjectInScene;
            sl.onObjectExited -= registerObjectInScene;
            sl.onObjectExited += registerObjectInScene;
        });

        //init scene object list
        data.gameStates
            .OrderBy(gs => gs.id).ToList()
            .ForEach(
                gs => updateSceneObjectList(gs.id)
            );
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
        //Add the given scene to list of open scenes
        openScenes.Add(scene);
        //Scene Loaded Delegate
        onSceneLoaded?.Invoke(scene);
        //If its a level scene,
        if (isLevelScene(scene))
        {
            //And it's the scene that we paused the game for,
            if (scene.buildIndex == PauseForLoadingSceneId)
            {
                //Unpause the game
                PauseForLoadingSceneId = -1;
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

    public bool isSceneOpen(int sceneId)
        => openScenes.Any(s => s.buildIndex == sceneId);

    public int SceneLoadingCount
        => sceneLoaders.Count(sl => sl.IsLoading);

    /// <summary>
    /// Restores the objects in the scene to their previous state before the scene was unloaded
    /// </summary>
    /// <param name="scene">The scene whose objects need their state stored</param>
    public void LoadObjectsFromScene(Scene scene)
    {
        //Get list of savables
        List<SavableObjectInfo> sceneGOs = SceneSavableList.getFromScene(scene).savables;
        //If an object from this scene is known not to be currently in this scene,
        List<SavableObjectInfo> unsceneGOs = sceneGOs.FindAll(
            soi => !isObjectInScene(soi, scene)
            );
        //Remove it from being processed, and
        sceneGOs.RemoveAll(go => unsceneGOs.Contains(go));
        //Destroy it before it gets put into the game object list.
        unsceneGOs.ForEach(soi =>
        {
            Debug.Log($"Destroying now duplicate: {soi} ({soi.Id})");
            Destroy(soi.gameObject);
        });
        //Register object in scene
        sceneGOs.ForEach(go => registerObjectInScene(go, scene));
        //Init the savables
        sceneGOs.ForEach(
            go => go.GetComponent<SavableObjectInfo>()
                .savables
                .ForEach(smb => smb.init())
            );
        //Find the last state that this scene was saved in
        int lastStateSeen = -1;
        SceneLoader sceneLoader = getSceneLoader(scene);
        if (sceneLoader)
        {
            lastStateSeen = sceneLoader.lastOpenGameStateId;
        }
        //Find foreign objects that are not here
        List<int> sceneIds = sceneGOs.ConvertAll(soi => soi.Id);
        List<int> foreignIds = getObjectsIdsInScene(scene)
            .FindAll(id => !sceneIds.Contains(id));
        //Delegate
        onSceneObjectsLoaded?.Invoke(sceneGOs, foreignIds, lastStateSeen);
    }
    public delegate void OnSceneObjectsLoaded(List<SavableObjectInfo> sceneGOs, List<int> foreignGOs, int lastStateSeen);
    public event OnSceneObjectsLoaded onSceneObjectsLoaded;

    private SceneLoader getSceneLoader(Scene scene)
        => sceneLoaders.Find(sl => sl.Scene == scene);
    private SceneLoader getSceneLoader(int sceneId)
        => sceneLoaders.Find(sl => sl.sceneId == sceneId);
    #endregion

    /// <summary>
    /// updates cache: which scene each object is in during the given state
    /// </summary>
    /// <param name="gameStateId"></param>
    public void updateSceneObjectList(int gameStateId)
    {
        data.gameStates[gameStateId].processStates(
            os => data.objectSceneList[os.objectId] = os.sceneId
            );
    }

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
    public void prepareForRewind(int rewindStateId)
    {
        int gameStateCount = data.gameStates.Count;
        //Load levels that Merky will be passing through
        foreach (SceneLoader sl in sceneLoaders)
        {
            if (sl.firstOpenGameStateId <= gameStateCount
                && sl.lastOpenGameStateId >= rewindStateId)
            {
                for (int i = gameStateCount - 1; i >= rewindStateId; i--)
                {
                    if (sl.isPositionInScene(data.gameStates[i].Merky.position))
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

    public List<int> getObjectsIdsInScene(Scene scene)
    {
        int sceneId = scene.buildIndex;
        return data.objectSceneList.ToList()
            .FindAll(entry => entry.Value == sceneId)
            .ConvertAll(entry => entry.Key);
    }

    public Scene getObjectScene(int objectId)
    {
        if (!data.objectSceneList.ContainsKey(objectId))
        {
            throw new ArgumentException(
                $"Cannot get scene of object with id {objectId} because it is not in the list"
                );
        }
        if (objectId < 0)
        {
            throw new ArgumentException(
                $"Cannot get scene of object with id {objectId} because its id is less than 0"
                );
        }
        return SceneManager.GetSceneByBuildIndex(data.objectSceneList[objectId]);
    }

    /// <summary>
    /// Returns true if the given object is in the scene according to the data object
    /// </summary>
    /// <param name="soi"></param>
    /// <param name="scene"></param>
    /// <returns></returns>
    public bool isObjectInScene(SavableObjectInfo soi, Scene scene)
    {
        int objectId = soi.Id;
        if (data.objectSceneList.ContainsKey(objectId))
        {
            return data.objectSceneList[objectId] == scene.buildIndex;
        }
        return true;
    }

    public bool isObjectSceneOpen(int objectId)
    {
        if (data.objectSceneList.ContainsKey(objectId))
        {
            Debug.Log($"Object's ({objectId}) scene id: {data.objectSceneList[objectId]}");
            return isSceneOpen(data.objectSceneList[objectId]);
        }
        Debug.Log($"Can't get scene for object ({objectId}) because it's not in the list");
        return false;
    }

    public void registerObjectInScene(GameObject go)
    {
        if (go == null || ReferenceEquals(go, null))
        {
            //don't register null or destroyed objects
            Debug.LogWarning($"GameObject {go?.name} is destroyed and will not be processed.");
            //removeObject(go);
            return;
        }
        SavableObjectInfo soi = go.GetComponent<SavableObjectInfo>();
        registerObjectInScene(soi);
    }

    public void registerObjectInScene(SavableObjectInfo soi)
    {

        if (!soi)
        {
            return;
        }
        //Don't add non-Savable or Singleton objects ever
        if (soi is SingletonObjectInfo)
        {
            removeObject(soi);
            return;
        }
        //Add it to the list that contains the position
        int objectId = soi.Id;
        if (objectId < 0)
        {
            Debug.LogError(
                $"Object {soi.TextLine} Id is less than 0! id: {objectId}",
                soi
                );
            removeObject(soi);
            return;
        }
        //If go is already in a scene,
        Scene scene = soi.gameObject.scene;
        int sceneId = scene.buildIndex;
        if (sceneId < 0)
        {
            sceneId = data.objectSceneList[soi.Id];
        }
        SceneLoader sl = getSceneLoader(sceneId);
        //And it's already in the right scene,
        if (sl && sl.overlapsPosition(soi.gameObject))
        {
            //Reregister but don't move it
            registerObjectInScene(soi, scene);
            return;
        }
        //Else find the scene it should be in
        SceneLoader loader = sceneLoaders.Find(sl => sl.overlapsPosition(soi.gameObject));
        if (!loader)
        {
            loader = sceneLoaders.Find(sl => sl.overlapsCollider(soi.gameObject));
        }
        //If it can't find the scene it's in,
        if (!loader)
        {
            //just don't process it
            Debug.LogWarning(
                $"Can't find a scene for object {soi.TextLine} ({objectId}) at position {soi.transform.position} ({((Vector2)soi.transform.position - Vector2.zero).magnitude} from center)",
                soi
                );
            return;
        }
        try
        {
            Scene scene2 = loader.Scene;
            int sceneId2 = scene2.buildIndex;
            if (sceneId2 < 0)
            {
                Debug.LogWarning(
                    $"SceneLoader {loader.gameObject.name} has bad scene ({scene2})! sceneId: {sceneId2}",
                    loader.gameObject
                    );
                //Don't process it
                return;
            }
            registerObjectInScene(soi, scene2);
            moveToScene(soi, scene2);
        }
        catch (NullReferenceException nre)
        {
            throw new NullReferenceException(
                $"No SL found when trying to register object {soi.TextLine} at position {soi.transform.position}",
                nre
                );
        }
        catch (ArgumentException ae)
        {
            Debug.LogError(
                $"No scene found when trying to register object {soi.TextLine} at position {soi.transform.position}" +
                $"\nArgumentException: {ae}",
                soi
                );
        }
    }

    private void registerObjectInScene(SavableObjectInfo soi, Scene scene)
    {
        int objectId = soi.Id;
        if (objectId == 0)
        {
            Debug.LogError(
                $"Trying to add object Id {objectId} to the objectScenesList!",
                soi
                );
            return;
        }
        int sceneId = scene.buildIndex;
        Debug.Log(
            $"Registering object in scene {scene.name} ({sceneId}): {soi.TextLine} ({objectId})",
            soi
            );
        if (!data.objectSceneList.ContainsKey(objectId))
        {
            data.objectSceneList.Add(objectId, sceneId);
        }
        else
        {
            data.objectSceneList[objectId] = sceneId;
        }
    }

    public void moveToScene(GameObject go, Scene scene)
    {
        SavableObjectInfo soi = go.GetComponent<SavableObjectInfo>();
        if (soi)
        {
            moveToScene(soi, scene);
        }
        else
        {
            try
            {
                if (go.scene != scene)
                {
                    if (scene.isLoaded)
                    {
                        Debug.Log($"Moving {go.name} into scene {scene.Name()}", go);
                        SceneManager.MoveGameObjectToScene(go, scene);
                        Debug.Log($"Moved {go.name} is now in scene {go.scene.Name()}", go);
                    }
                    else
                    {
                        Debug.Log($"Moving {go.name} into scene {scene.Name()}, BUT scene is unloaded, destroying instead", go);
                        Destroy(go);
                    }
                }
            }
            catch (ArgumentException ae)
            {
                Debug.LogError(
                    $"Trying to move {go.name} into scene {scene.Name()} at position: {go.transform.position}" +
                    $"\nArgumentException: {ae}"
                    );
            }
        }
    }

    /// <summary>
    /// Moves the given game object into the given scene,
    /// BUT if the scene is unloaded, it will destroy the object instead
    /// </summary>
    /// <param name="go"></param>
    /// <param name="scene"></param>
    public void moveToScene(SavableObjectInfo soi, Scene scene)
    {
        GameObject go = soi.gameObject;
        try
        {
            if (go.transform.parent != null)
            {
                //TODO: look out for SOI with a parent SOI
                go.transform.SetParent(null);
            }
            if (go.scene != scene)
            {
                if (scene.isLoaded)
                {
                    Debug.Log($"Moving {soi.TextLine} into scene {scene.Name()}", go);
                    SceneManager.MoveGameObjectToScene(go, scene);
                    Debug.Log($"Moved {soi.TextLine} is now in scene {go.scene.Name()}", go);
                }
                else
                {
                    Debug.Log($"Moving {soi.TextLine} into scene {scene.Name()}, BUT scene is unloaded, destroying instead", go);
                    Destroy(go);
                }
            }
        }
        catch (ArgumentException ae)
        {
            Debug.LogError(
                $"Trying to move {soi.TextLine} into scene {scene.Name()} at position: {go.transform.position}" +
                $"\nArgumentException: {ae}"
                );
        }
    }

    private void removeObject(SavableObjectInfo soi)
    {
        Debug.Log($"Removing object {soi.TextLine} from list", soi);
        data.objectSceneList.Remove(soi.Id);
    }

    public void printObjectSceneList()
    {
        Debug.Log(
            $"=== ScenesManager Object Scene List ({data.objectSceneList.Count})===",
            gameObject
            );
        data.objectSceneList.ToList()
            .ForEach(entry => Debug.Log($"{entry.Key} => {entry.Value}"));
    }
}
