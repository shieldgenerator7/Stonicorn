using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEngine.U2D;
using Debug = UnityEngine.Debug;
using System.Linq;
using UnityEditor.SceneManagement;
using System;
using System.Reflection;
using UnityEditor.AddressableAssets.Settings;
using System.IO;

public class CustomMenu
{
    const int FIRST_LEVEL_INDEX = 4;

    [MenuItem("SG7/Editor/Terrain/Focus Terrain Tool %T")]
    public static void levelTerrainPoints()
    {
        SpriteShapeTool sst = GameObject.FindAnyObjectByType<SpriteShapeTool>();
        if (sst)
        {
            Selection.activeGameObject = sst.gameObject;
        }
    }

    //Find Missing Scripts
    //2018-04-13: copied from http://wiki.unity3d.com/index.php?title=FindMissingScripts
    static int go_count = 0, components_count = 0, missing_count = 0;
    [MenuItem("SG7/Editor/Refactor/Find Missing Scripts")]
    private static void FindMissingScripts()
    {
        go_count = 0;
        components_count = 0;
        missing_count = 0;
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene s = SceneManager.GetSceneAt(i);
            if (s.isLoaded)
            {
                foreach (GameObject go in s.GetRootGameObjects())
                {
                    FindInGO(go);
                }
            }
        }
        Debug.Log($"Searched {go_count} GameObjects, {components_count} components, found {missing_count} missing");
    }
    private static void FindInGO(GameObject g)
    {
        go_count++;
        Component[] components = g.GetComponents<Component>();
        for (int i = 0; i < components.Length; i++)
        {
            components_count++;
            if (components[i] == null)
            {
                missing_count++;
                string s = g.name;
                Transform t = g.transform;
                while (t.parent != null)
                {
                    s = $"{t.parent.name}/{s}";
                    t = t.parent;
                }
                Debug.Log($"{s} has an empty script attached in position: {i}", g);
            }
        }
        // Now recurse through each child GO (if there are any):
        foreach (Transform childT in g.transform)
        {
            FindInGO(childT.gameObject);
        }
    }
    [MenuItem("SG7/Editor/Refactor/Check Ability Activators")]
    private static void CheckMileStoneActivatorAbility()
    {
        int errorCount = 0;
        PlayerController pc = GameObject.FindAnyObjectByType<PlayerController>();
        GameObject.FindObjectsByType<MilestoneActivatorAbility>(FindObjectsInactive.Include, FindObjectsSortMode.None).ToList()
            .ForEach(
            maa =>
            {
                PlayerAbility pa = (PlayerAbility)pc.GetComponent(
                    maa.abilityTypeName
                    );
                if (!pa)
                {
                    errorCount++;
                    Debug.LogError(
                        $"Player does not have an ability called {maa.abilityTypeName}!",
                        maa
                        );
                }
            }
        );

        if (errorCount == 0)
        {
            Debug.Log("All Ability Activators are ok");
        }
    }

    [MenuItem("SG7/Editor/Refactor/Change HideableArea to NonTeleportableArea")]
    public static void changeTag()
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene s = SceneManager.GetSceneAt(i);
            if (s.isLoaded)
            {
                foreach (GameObject go in s.GetRootGameObjects())
                {
                    foreach (Transform tf in go.transform)
                    {
                        if (tf.gameObject.tag == "HideableArea")//NonTeleportableArea" || go.name == "HiddenAreas" || go.name == "Hidden Areas")
                        {
                            tf.gameObject.tag = "NonTeleportableArea";
                        }
                    }
                }
            }
        }
    }
    [MenuItem("SG7/Editor/Refactor/Propagate HideableArea NonTeleportableArea Tag")]
    public static void refactorHideableArea()
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene s = SceneManager.GetSceneAt(i);
            if (s.isLoaded)
            {
                foreach (GameObject go in s.GetRootGameObjects())
                {
                    foreach (Transform tf in go.transform)
                    {
                        //Find the HiddenAreas
                        if (tf.GetComponent<HiddenArea>())
                        {
                            foreach (Transform tf2 in tf)
                            {
                                //If it's a SecretAreaTrigger,
                                //It needs to be teleportable
                                if (tf2.GetComponent<SecretAreaTrigger>())
                                {
                                    tf2.gameObject.tag = "Untagged";
                                }
                                //Part of the hidden area,
                                //Needs to be NOT teleportable
                                else
                                {
                                    tf2.gameObject.tag = "NonTeleportableArea";
                                }
                            }
                        }
                    }
                }
            }
        }
    }
    [MenuItem("SG7/Editor/Refactor/Rebuild Addressable Asset %&a")]
    public static void rebuildAddressableAssets()
    {
        AddressableAssetSettings.CleanPlayerContent();
        AddressableAssetSettings.BuildPlayerContent();
    }

    [MenuItem("SG7/Editor/Toggle Editor Camera AutoRotate %R")]
    public static void toggleEditorCameraAutoRotate()
    {
        EditorCameraRotatorObject ecro =
            GameObject.FindAnyObjectByType<EditorCameraRotatorObject>();
        ecro.toggle();
    }

    [MenuItem("SG7/Editor/Mechanics/Connect selected Lantern and HiddenArea %#H")]
    public static void connectLanternToHiddenArea()
    {
        HiddenAreaConnector hac = GameObject.FindAnyObjectByType<HiddenAreaConnector>();
        Selection.activeGameObject = hac.gameObject;
        hac.connect();
    }

    [MenuItem("SG7/Editor/Mechanics/Connect selected Breakable Wall to nearby HiddenArea %&H")]
    public static void connectBreakableToHiddenArea()
    {
        //Find BreakableWall
        BreakableWall breakableWall = Selection.activeGameObject?.GetComponent<BreakableWall>();
        if (!breakableWall)
        {
            Debug.LogError("No breakable wall selected!");
            return;
        }

        //Reset secret hiders list
        breakableWall.secretHiders.Clear();

        //Find HiddenAreas
        RaycastHit2D[] rch2ds = Physics2D.BoxCastAll(breakableWall.transform.position, Vector2.one * 1, 0, Vector2.zero);
        foreach (RaycastHit2D rch in rch2ds)
        {
            GameObject go = rch.collider.gameObject;
            HiddenArea ha = go.GetComponent<HiddenArea>() ?? go.GetComponentInParent<HiddenArea>();
            if (ha)
            {
                breakableWall.secretHiders.Add(ha);
            }
        }

        //Select everything
        List<GameObject> selection = breakableWall.secretHiders.ConvertAll(ha => ha.gameObject);
        selection.Add(breakableWall.gameObject);
        Selection.objects = selection.ToArray();

        //Set dirty
        EditorUtility.SetDirty(breakableWall);
        EditorUtility.SetDirty(breakableWall.gameObject);
    }

    [MenuItem("SG7/Editor/Mechanics/Autosize BoxColliider2D to tiled sprite")]
    public static void autosizeBC2DtoTiledSprite()
    {
        List<GameObject> gos = Selection.gameObjects
                 .Where(go =>
            go.GetComponent<SpriteRenderer>()?.drawMode == SpriteDrawMode.Tiled
            && go.GetComponent<BoxCollider2D>()
            ).ToList();
        if (gos.Count == 0)
        {
            Debug.LogWarning("Select 1 or more gameobjects with both a SpriteRenderer in Tiled mode, and a BoxCollider2D");
            return;
        }
        int changedCount = 0;
        foreach (GameObject go in gos)
        {
            autosizeBC2DtoTiledSprite(go);
            changedCount++;
        }
        Debug.Log($"Autosized {changedCount} BoxCollider2Ds to SpriteRenderer tiled size");
    }
    public static void autosizeBC2DtoTiledSprite(GameObject go)
    {
        SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
        BoxCollider2D bc2d = go.GetComponent<BoxCollider2D>();
        bc2d.size = sr.size;
        EditorUtility.SetDirty(bc2d);
    }

    [MenuItem("SG7/Editor/Mechanics/Auto-extend platform width")]
    public static void autoextendPlatformWidth()
    {
        List<GameObject> gos = Selection.gameObjects
                 .Where(go =>
            go.GetComponent<SpriteRenderer>()?.drawMode == SpriteDrawMode.Tiled
            && go.GetComponent<BoxCollider2D>()
            ).ToList();
        if (gos.Count == 0)
        {
            Debug.LogWarning("Select 1 or more gameobjects with both a SpriteRenderer in Tiled mode, and a BoxCollider2D");
            return;
        }
        int changedCount = 0;
        foreach (GameObject go in gos)
        {
            //Find left and right points
            BoxCollider2D bc2d = go.GetComponent<BoxCollider2D>();
            bc2d.enabled = false;
            RaycastHit2D rch2dRight = Physics2D.Raycast(go.transform.position, go.transform.right);
            Vector2 rightPos = rch2dRight.point;
            RaycastHit2D rch2dLeft = Physics2D.Raycast(go.transform.position, -go.transform.right);
            Vector2 leftPos = rch2dLeft.point;
            bc2d.enabled = true;
            //Move platform to center
            go.transform.position = (rightPos + leftPos) / 2;
            EditorUtility.SetDirty(go);
            //Set size to distance between left and right points
            SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
            Vector2 size = sr.size;
            size.x = Vector2.Distance(leftPos, rightPos);
            sr.size = size;
            EditorUtility.SetDirty(sr);
            //Update BoxCollider2D
            autosizeBC2DtoTiledSprite(go);
            //Update counter
            changedCount++;
        }
        Debug.Log($"Auto-extended {changedCount} platform widths");
    }

    [MenuItem("SG7/Editor/List Prefabs")]
    public static void listPrefabs()
    {
        GameObject.FindObjectsByType<SavableObjectInfo>(FindObjectsSortMode.None)
            .Where(soi => soi.PrefabAddress.editorAsset != null)
            .OrderBy(soi => soi.PrefabAddress.editorAsset.name).ToList()
            .ForEach(soi =>
                Debug.Log($"Prefab: {soi.PrefabAddress.editorAsset.name}", soi.gameObject)
            );
    }

    [MenuItem("SG7/Editor/Spawn Point/Toggle Merky Spawn Point %#`")]
    public static void callMerky()
    {
        if (Application.isEditor && !Application.isPlaying)
        {
            PlayerTestSpawnPoint playerTSP = GameObject.FindAnyObjectByType<PlayerTestSpawnPoint>();
            GameObject playerSpawnObject = playerTSP.gameObject;

            //Enable it
            playerTSP.enabled = true;
            playerSpawnObject.SetActive(true);
            RulerDisplayer rd = GameObject.FindAnyObjectByType<RulerDisplayer>();
            if (rd)
            {
                rd.transform.position = RulerDisplayer.currentMousePos;
            }
            else
            {
                playerSpawnObject.transform.position = (Vector2)SceneView.GetAllSceneCameras()[0].transform.position;
            }
            Selection.activeGameObject = playerSpawnObject;
            Debug.Log($"PTSP enabled: {playerTSP.enabled}");
        }
        else
        {
            //Call the player
            GameObject playerObject = GameObject.FindAnyObjectByType<PlayerController>().gameObject;
            if (GameObject.FindAnyObjectByType<RulerDisplayer>())
            {
                playerObject.transform.position = RulerDisplayer.currentMousePos;
            }
            else
            {
                playerObject.transform.position = (Vector2)SceneView.GetAllSceneCameras()[0].transform.position;
            }
            Selection.activeGameObject = playerObject;
        }
    }

    [MenuItem("SG7/Editor/Spawn Point/Deactivate Merky Spawn Point %&`")]
    public static void uncallMerky()
    {
        if (Application.isEditor && !Application.isPlaying)
        {
            //Deactivate spawn point
            PlayerTestSpawnPoint playerTSP = GameObject.FindAnyObjectByType<PlayerTestSpawnPoint>();
            playerTSP.enabled = false;
            GameObject playerSpawnObject = playerTSP.gameObject;
            playerSpawnObject.SetActive(true);
            //Select player object
            GameObject playerObject = GameObject.FindAnyObjectByType<PlayerController>().gameObject;
            Selection.activeGameObject = playerObject;
            Debug.Log($"PTSP enabled: {playerTSP.enabled}");
        }
    }

    [MenuItem("SG7/Editor/Ruler/Toggle Ruler %`")]
    /// <summary>
    /// Turns the ruler tools on and off
    /// </summary>
    public static void toggleRulers()
    {
        bool anyOn = false;
        foreach (RulerDisplayer rd in GameObject.FindObjectsByType<RulerDisplayer>(FindObjectsSortMode.None))
        {
            if (rd.active)
            {
                anyOn = true;
                break;
            }
        }
        foreach (RulerDisplayer rd in GameObject.FindObjectsByType<RulerDisplayer>(FindObjectsSortMode.None))
        {
            rd.active = !anyOn;
        }
        anyOn = !anyOn;
        //If the rulers are activating,
        if (anyOn)
        {
            //turn off all the range previews
            foreach (RulerRangePreview rrp in GameObject.FindObjectsByType<RulerRangePreview>(FindObjectsSortMode.None))
            {
                rrp.Active = false;
            }

            //select ruler
            if (!Selection.activeGameObject)
            {
                Selection.activeGameObject = GameObject.FindAnyObjectByType<RulerDisplayer>().gameObject;
            }
        }
    }

    [MenuItem("SG7/Editor/Ruler/Call Ruler to its Range Preview #`")]
    /// <summary>
    /// Repositions the ruler to its range preview's position
    /// </summary>
    public static void callRulerToPreview()
    {
        foreach (RulerRangePreview rrp in GameObject.FindObjectsByType<RulerRangePreview>(FindObjectsSortMode.None))
        {
            rrp.callParentRuler();
        }
    }
    [MenuItem("SG7/Editor/Ruler/Toggle Ruler Range Preview &`")]
    /// <summary>
    /// Turns the ruler tools on and off
    /// </summary>
    public static void toggleRulerRangePreviews()
    {
        bool anyOn = false;
        foreach (RulerRangePreview rrp in GameObject.FindObjectsByType<RulerRangePreview>(FindObjectsSortMode.None))
        {
            if (rrp.Active)
            {
                anyOn = true;
                break;
            }
        }
        foreach (RulerRangePreview rrp in GameObject.FindObjectsByType<RulerRangePreview>(FindObjectsSortMode.None))
        {
            rrp.Active = !anyOn;
        }
    }

    public static List<Scene> getLevelScenes(Func<Scene, bool> filter = null, bool reportFailures = false)
    {
        List<Scene> scenes = new List<Scene>();
        for (int i = FIRST_LEVEL_INDEX; i < EditorBuildSettings.scenes.Length; i++)
        {
            Scene scene = EditorSceneManager.GetSceneByBuildIndex(i);
            if (filter?.Invoke(scene) ?? true)
            {
                scenes.Add(scene);
            }
            else
            {
                if (reportFailures)
                {
                    Debug.LogError($"Scene {scene.name} at index {i} failed the filter.");
                }
            }
        }
        return scenes;
    }

    [MenuItem("SG7/Editor/Load or Unload Level Scenes %&S")]
    public static void loadOrUnloadLevelScenes()
    {
        //Find out if all of the scenes are loaded
        List<Scene> levels = getLevelScenes((s) => !String.IsNullOrEmpty(s.name), true);
        bool allLoaded = allLevelScenesLoaded(levels);
        //If any are unloaded, load them all.
        //Else, unload them all.
        loadAllLevelScenes(levels, !allLoaded);
    }
    public static void loadAllLevelScenes(List<Scene> levels, bool load)
    {
        //Load or unload all the level scenes
        levels.ForEach(scene =>
        {
            if (!load)
            {
                //Unload
                EditorSceneManager.CloseScene(scene, false);
            }
            else
            {
                //Load
                try
                {
                    EditorSceneManager.OpenScene(
                        scene.path,
                        OpenSceneMode.Additive
                        );
                    SetExpanded(scene, false);
                }
                catch (ArgumentException ae)
                {
                    Debug.LogError($"scene load error ({scene.name}): {ae}");
                }
            }
        });
    }

    //2020-12-09: copied from https://forum.unity.com/threads/how-to-collapse-hierarchy-scene-nodes-via-script.605245/#post-6551890
    private static void SetExpanded(Scene scene, bool expand)
    {
        foreach (var window in Resources.FindObjectsOfTypeAll<SearchableEditorWindow>())
        {
            if (window.GetType().Name != "SceneHierarchyWindow")
                continue;

            var method = window.GetType().GetMethod("SetExpandedRecursive",
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance, null,
                new[] { typeof(int), typeof(bool) }, null);

            if (method == null)
            {
                Debug.LogError(
                    "Could not find method 'UnityEditor.SceneHierarchyWindow.SetExpandedRecursive(int, bool)'.");
                return;
            }

            var field = scene.GetType().GetField("m_Handle",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (field == null)
            {
                Debug.LogError("Could not find field 'int UnityEngine.SceneManagement.Scene.m_Handle'.");
                return;
            }

            var sceneHandle = field.GetValue(scene);
            method.Invoke(window, new[] { sceneHandle, expand });
        }
    }

    [MenuItem("SG7/Editor/Hide or Unhide Hidden Areas %h")]
    public static void hideUnhideHiddenAreas()
    {
        //2019-01-01: copied from a comment by Mikilo: https://answers.unity.com/questions/1039366/is-it-possible-to-access-layer-visibility-and-lock.html
        Tools.visibleLayers ^= 1 << LayerMask.NameToLayer("Hidden Area"); // Toggle a value in lockedLayers.
    }

    public static void hideUnhideHiddenAreas(GameObject go1, ref bool show, ref bool changeDetermined, int levelsDeep)
    {
        foreach (Transform tf in go1.transform)
        {
            GameObject go = tf.gameObject;
            if (go.CompareTag("NonTeleportableArea")
                || go.name == "HiddenAreas" || go.name == "Hidden Areas")
            {
                if (!changeDetermined)
                {
                    show = !go.activeInHierarchy;
                    changeDetermined = true;
                }
                go.SetActive(show);
            }
            if (levelsDeep > 0)
            {
                hideUnhideHiddenAreas(go, ref show, ref changeDetermined, levelsDeep - 1);
            }
        }
    }

    [MenuItem("SG7/Editor/Show or Hide All Colliders %&c")]
    public static void showHideAllColliders()
    {
        //TODO: update this with the new way of doing it
        //Physics2D.alwaysShowColliders = !Physics2D.alwaysShowColliders;
        Debug.LogError("Physics2D.alwaysShowColliders was deprecated! :(");
    }

    [MenuItem("SG7/Editor/Log Objects %l")]
    public static void logObjects()
    {
        Logger logger = GameObject.FindAnyObjectByType<Logger>();
        if (logger)
        {
            logger.logObjects.AddRange(
                Selection.GetFiltered<GameObject>(SelectionMode.Editable)
            );
            Selection.activeObject = logger;
        }
    }

    public static void ClearLog()
    {
        //2020-12-28: copied from https://stackoverflow.com/a/40578161/2336212
        var assembly = Assembly.GetAssembly(typeof(UnityEditor.Editor));
        var type = assembly.GetType("UnityEditor.LogEntries");
        var method = type.GetMethod("Clear");
        method.Invoke(new object(), null);
    }

    [MenuItem("SG7/Build/Pre-Build/Perform all Pre-Build Tasks &W")]
    public static bool performAllPreBuildTasks()
    {
        ClearLog();
        Debug.Log("Running all Pre-Build Tasks");
        //Setup
        EditorSceneManager.SaveOpenScenes();
        List<Scene> levels = getLevelScenes(
            (s) => !String.IsNullOrEmpty(s.name),
            true
            );
        loadAllLevelScenes(levels, false);
        loadAllLevelScenes(levels, true);
        int waitCount = 0;
        const int WAIT_LIMIT = 1000;
        while (!allLevelScenesLoaded(levels))
        {
            new WaitForSecondsRealtime(0.1f);
            waitCount++;
            if (waitCount >= WAIT_LIMIT)
            {
                Debug.LogError("Waited longer than limit for scenes to load, continuing anyway");
                break;
            }
        }

        //Checklist
        refreshSceneSavableObjectLists();
        bool keepScenesOpen = new List<Func<bool>>() {
                ensureSavableObjectsHaveObjectInfo,
                ensureMemoryObjectsHaveObjectInfo,
                ensureUniqueObjectIDs,
                autoInitializeInPrefabs,
                checkISetupablesInPrefabs,
                ensureHiddenAreasAreProperlySetup,
                checkTiledHitBoxes,
                checkGravityScale,
                checkForIllegalPrefabOverrides,
                checkForGroundLayerObjects,
                checkTriggersAreNotSolid,
                ensureNPCsHaveDialogueTriggers,
                checkDialogueEvents,
                checkAutoInitializeTags,
                checkISetupables,
            }
            .ConvertAll(func =>
            {
                try
                {
                    return func();
                }
                catch (Exception ex)
                {
                    Debug.LogError($"pre build task {func.Method.Name} failed: {ex}");
                    Debug.LogException(ex);
                }
                return true;
            })
            .Any(b => b);

        populateObjectManagerKnownObjectsList();

        //Cleanup
        EditorSceneManager.SaveOpenScenes();
        if (!keepScenesOpen)
        {
            loadAllLevelScenes(levels, false);
        }
        //Finish
        Debug.Log("Finished all Pre-Build Tasks");
        return keepScenesOpen;
    }

    static bool allLevelScenesLoaded(List<Scene> levels)
        => levels.All(s => s.isLoaded);

    [MenuItem("SG7/Build/Pre-Build/Refresh Scene Savable Object Lists")]
    public static void refreshSceneSavableObjectLists()
    {
        GameObject.FindObjectsByType<SceneSavableList>(FindObjectsSortMode.None).ToList()
            .ForEach(ssl => ssl.refreshList());
    }

    [MenuItem("SG7/Build/Pre-Build/Ensure savable objects have ObjectInfo")]
    public static bool ensureSavableObjectsHaveObjectInfo()
    {
        List<GameObject> savables = new List<GameObject>();
        GameObject.FindObjectsByType<SceneSavableList>(FindObjectsSortMode.None).ToList()
            .ForEach(ssl => savables.AddRange(ssl.savables));
        //Missing ObjectInfo
        List<GameObject> missingInfo = savables
            .FindAll(go => !go.GetComponent<SavableObjectInfo>());
        missingInfo.ForEach(
            go => Debug.LogError($"{go.name} does not have an SavableObjectInfo!", go)
            );
        //Null info in ObjectInfo
        List<GameObject> nullInfo = savables
            .FindAll(go =>
            {
                SavableObjectInfo info = go.GetComponent<SavableObjectInfo>();
                return info && (info.PrefabGUID == null || info.PrefabGUID == "");
            }
            );
        nullInfo.ForEach(
            go => Debug.LogError($"{go.name} has SavableObjectInfo with missing prefabGUID!", go)
            );
        //Spawn State 0
        List<GameObject> spawn0 = savables
            .FindAll(go =>
            {
                SavableObjectInfo info = go.GetComponent<SavableObjectInfo>();
                return info && info.spawnStateId != 0;
            }
            );
        spawn0.ForEach(
            go =>
            {
                SavableObjectInfo info = go.GetComponent<SavableObjectInfo>();
                Debug.LogWarning(
                    $"{go.name} has non-zero spawn state; zeroing it out... {info.spawnStateId}",
                    go
                    );
                info.spawnStateId = 0;
                EditorUtility.SetDirty(info);
                EditorSceneManager.MarkSceneDirty(info.gameObject.scene);
            }
            );
        return missingInfo.Count > 0 || nullInfo.Count > 0 || spawn0.Count > 0;
    }

    [MenuItem("SG7/Build/Pre-Build/Ensure memory objects have ObjectInfo")]
    public static bool ensureMemoryObjectsHaveObjectInfo()
    {
        List<GameObject> memories = new List<GameObject>();
        GameObject.FindObjectsByType<SceneSavableList>(FindObjectsSortMode.None).ToList()
            .ForEach(ssl => memories.AddRange(ssl.memories));
        //Missing ObjectInfo
        List<GameObject> missingInfo = memories
            .FindAll(go => !go.GetComponent<MemoryObjectInfo>());
        missingInfo.ForEach(
            go => Debug.LogError($"{go.name} does not have an MemoryObjectInfo!", go)
            );
        return missingInfo.Count > 0;
    }
    [MenuItem("SG7/Build/Pre-Build/Ensure unique object IDs among open scenes")]
    public static bool ensureUniqueObjectIDs()
    {
        int nextID = 0;
        bool changedId = false;
        const int SECTION_SIZE = 1000;
        foreach (SceneSavableList ssl in GameObject.FindObjectsByType<SceneSavableList>(FindObjectsSortMode.None))
        {
            //Use buildIndex to set next id
            nextID = ssl.gameObject.scene.buildIndex * SECTION_SIZE;
            //Get list of savables
            List<GameObject> savables = new List<GameObject>();
            savables.AddRange(ssl.savables);
            savables.AddRange(ssl.memories);
            savables.ConvertAll(go => go.GetComponent<ObjectInfo>())
                .FindAll(info => !(info is SingletonObjectInfo))
                .ForEach(info =>
                {
                    int id = nextID;
                    nextID++;
                    int prevID = info.Id;
                    info.Id = id;
                    if (id != prevID)
                    {
                        Debug.LogWarning(
                            $"Changed Id: {prevID} -> {id}",
                            info.gameObject
                            );
                        EditorUtility.SetDirty(info);
                        EditorSceneManager.MarkSceneDirty(info.gameObject.scene);
                        changedId = true;
                    }
                });
        }
        return changedId;
    }

    //2025-02-08: copied from ensureSavableObjectInfosSetupInPrefabs()
    [MenuItem("SG7/Build/Pre-Build/Auto Initialize components in Prefabs")]
    public static bool autoInitializeInPrefabs()
    {
        //2021-07-12: got help from: https://forum.unity.com/threads/how-do-i-edit-prefabs-from-scripts.685711/
        int changeCount = 0;
        int errorCount = 0;
        //
        GetFiles("Assets/")
            .Where(s => s.EndsWith(".prefab"))
            .ToList().ForEach(
                assetPath =>
                {
                    //Debug.Log($"Auto Initialize in Prefab: {assetPath}");
                    GameObject go = PrefabUtility.LoadPrefabContents(assetPath);

                    List<MonoBehaviour> mbList = go.GetComponents<MonoBehaviour>().ToList();
                    mbList.AddRange(go.GetComponentsInChildren<MonoBehaviour>());
                    (int changesGO, int errorsGO) = _autoInitializeTags(mbList);

                    //changed?
                    if (changesGO > 0)
                    {
                        changeCount += changesGO;
                        PrefabUtility.SaveAsPrefabAsset(go, assetPath);
                        EditorUtility.SetDirty(go);
                        if (changesGO > 1)
                        {
                            Debug.LogWarning($"Check AutoInitialize Tags: {go.name} changes: {changesGO}", go);
                        }
                        changeCount += changesGO;
                    }
                    if (errorsGO > 0)
                    {
                        if (errorsGO > 1)
                        {
                            Debug.LogError($"Check AutoInitialize Tags: {go.name} errors: {errorsGO}", go);
                        }
                        errorCount += errorsGO;
                    }

                    PrefabUtility.UnloadPrefabContents(go);
                }
            );
        if (changeCount > 0)
        {
            Debug.LogWarning($"Auto Initialized {changeCount} components in prefabs");
        }
        if (errorCount > 0)
        {
            Debug.LogError($"Auto Initializer: {errorCount} errors");
        }
        return changeCount > 0 || errorCount > 0;
    }

    //2025-02-09: copied from autoInitializeInPrefabs()
    [MenuItem("SG7/Build/Pre-Build/Check ISetupables in Prefabs")]
    public static bool checkISetupablesInPrefabs()
    {
        //2021-07-12: got help from: https://forum.unity.com/threads/how-do-i-edit-prefabs-from-scripts.685711/
        int changeCount = 0;
        int errorCount = 0;
        //
        GetFiles("Assets/")
            .Where(s => s.EndsWith(".prefab"))
            .ToList().ForEach(
                assetPath =>
                {
                    //Debug.Log($"Auto Initialize in Prefab: {assetPath}");
                    GameObject go = PrefabUtility.LoadPrefabContents(assetPath);

                    List<ISetupable> mbList = go.GetComponents<MonoBehaviour>().OfType<ISetupable>().ToList();
                    mbList.AddRange(go.GetComponentsInChildren<MonoBehaviour>().OfType<ISetupable>());
                    (int changesGO, int errorsGO) = _checkISetupables(mbList);

                    //changed?
                    if (changesGO > 0)
                    {
                        changeCount += changesGO;
                        PrefabUtility.SaveAsPrefabAsset(go, assetPath);
                        EditorUtility.SetDirty(go);
                        if (changesGO > 1)
                        {
                            Debug.LogWarning($"Check Prefab ISetupables: {go.name} changes: {changesGO}", go);
                        }
                        changeCount += changesGO;
                    }
                    if (errorsGO > 0)
                    {
                        if (errorsGO > 1)
                        {
                            Debug.LogError($"Check Prefabs ISetupables: {go.name} errors: {errorsGO}", go);
                        }
                        errorCount += errorsGO;
                    }

                    PrefabUtility.UnloadPrefabContents(go);
                }
            );
        if (changeCount > 0)
        {
            Debug.LogWarning($"Checked ISetupables {changeCount} in prefabs");
        }
        if (errorCount > 0)
        {
            Debug.LogError($"Checked Prefabs ISetupables: {errorCount} errors");
        }
        return changeCount > 0 || errorCount > 0;
    }

    [MenuItem("SG7/Build/Pre-Build/Ensure Hidden Areas are Properly Setup")]
    public static bool ensureHiddenAreasAreProperlySetup()
    {
        int changedCount = 0;
        string TAG = "NonTeleportableArea";
        string UNTAG = "Untagged";
        GameObject.FindObjectsByType<HiddenArea>(FindObjectsSortMode.None).ToList()
            .ForEach(ha =>
            {
                Utility.doForGameObjectAndChildren(
                    ha.gameObject,
                    (go) =>
                    {
                        //If it doesn't have the correct tag,
                        if (!go.CompareTag(TAG))
                        {
                            //And it has a renderer,
                            if (go.GetComponent<Renderer>())
                            {
                                //Then it should have the correct tag
                                go.tag = TAG;
                                EditorUtility.SetDirty(go);
                                Debug.LogWarning(
                                    $"Changed {go.name} tag to {TAG}.",
                                    go
                                    );
                                changedCount++;
                            }
                        }
                        //If it does have the tag,
                        else
                        {
                            //But does not have a renderer,
                            if (!go.GetComponent<Renderer>())
                            {
                                //Then it should not have the tag
                                go.tag = UNTAG;
                                EditorUtility.SetDirty(go);
                                Debug.LogWarning(
                                    $"Changed {go.name} tag to {UNTAG}.",
                                    go
                                    );
                                changedCount++;
                            }
                        }
                        //Position
                        if (go.transform.position.z != 0)
                        {
                            go.transform.position = (Vector2)go.transform.position;
                            EditorUtility.SetDirty(go);
                            Debug.LogWarning(
                                $"Changed {go.name} pos to {go.transform.position}.",
                                go
                                );
                            changedCount++;
                        }
                        //
                        //Renderer && Collider
                        //
                        Renderer renderer = go.GetComponent<Renderer>();
                        Collider2D coll2d = go.GetComponent<Collider2D>();
                        string layerName = "Foreground";
                        if (renderer)
                        {
                            if (renderer.sortingLayerName != layerName)
                            {
                                renderer.sortingLayerName = layerName;
                                EditorUtility.SetDirty(go);
                                Debug.LogWarning(
                                    $"Changed {go.name} layer name to {layerName}.",
                                    go
                                    );
                                changedCount++;
                            }
                            if (!coll2d)
                            {
                                Debug.LogError(
                                    $"{go.name} has renderer without a collider!",
                                    go
                                    );
                                //Fake a change
                                if (changedCount == 0)
                                {
                                    changedCount++;
                                }
                            }
                        }
                        if (coll2d)
                        {
                            if (!coll2d.isTrigger)
                            {
                                coll2d.isTrigger = true;
                                EditorUtility.SetDirty(go);
                                Debug.LogWarning(
                                    $"Changed {go.name} collider isTrigger to {coll2d.isTrigger}.",
                                    go
                                    );
                                changedCount++;
                            }
                        }
                    }
                    );
            });

        if (changedCount > 0)
        {
            Debug.LogWarning(
                $"HiddenArea changes: Made {changedCount} changes."
                );
        }
        return changedCount > 0;
    }

    [MenuItem("SG7/Build/Pre-Build/Check Tiled HitBoxes")]
    public static bool checkTiledHitBoxes()
    {
        int changedCount = 0;
        GameObject.FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None)
            .Where(sr => sr.drawMode == SpriteDrawMode.Tiled)
            .OrderBy(sr => sr.gameObject.scene.buildIndex)
            .ThenBy(sr => sr.name).ToList()
            .ForEach(sr =>
            {
                bool changedSR = false;
                //
                // Check for reasonable sprite size
                //
                Vector2 oldSRSize = sr.size;
                Vector2 newSRSize = sr.size;
                newSRSize.x = Mathf.Round(newSRSize.x * 100) / 100;
                newSRSize.y = Mathf.Round(newSRSize.y * 100) / 100;
                if (newSRSize != oldSRSize)
                {
                    sr.size = newSRSize;
                    Debug.LogWarning(
                        $"Changed {sr.name} sprite size " +
                        $"from ({oldSRSize.x}, {oldSRSize.y}) " +
                        $"to ({newSRSize.x}, {newSRSize.y}).",
                        sr
                        );
                    changedSR = true;
                }
                //
                // Check collider size
                //
                BoxCollider2D bc2d = sr.GetComponent<BoxCollider2D>();
                if (bc2d)
                {
                    Vector2 oldSize = bc2d.size;
                    Vector2 newSize = bc2d.size;
                    //If tiled vertically,
                    if (sr.size.y > sr.size.x * 2)
                    {
                        //Set collider height to match
                        newSize.y = sr.size.y;
                    }
                    //Else: it's tiled horizontally, so
                    else
                    {
                        //Set collider width to match
                        newSize.x = sr.size.x;
                    }
                    if (newSize != oldSize)
                    {
                        bc2d.size = newSize;
                        Debug.LogWarning(
                            $"Changed {sr.name} collider size " +
                            $"from ({oldSize.x}, {oldSize.y}) " +
                            $"to ({newSize.x}, {newSize.y}).",
                            sr
                            );
                        changedSR = true;
                    }
                }
                if (changedSR)
                {
                    EditorUtility.SetDirty(sr);
                    if (bc2d) { EditorUtility.SetDirty(bc2d); }
                    EditorUtility.SetDirty(sr.gameObject);
                    changedCount++;
                }
            });

        if (changedCount > 0)
        {
            Debug.LogWarning(
                $"Tiled sprites changes: Made {changedCount} changes."
                );
        }
        return changedCount > 0;
    }

    [MenuItem("SG7/Build/Pre-Build/Check gravity scale")]
    public static bool checkGravityScale()
    {
        int problemCount = 0;
        foreach (Rigidbody2D rb2d in GameObject.FindObjectsByType<Rigidbody2D>(FindObjectsSortMode.None))
        {
            //if there's a GravityAccepter, it's all good
            GravityAccepter ga = rb2d.GetComponent<GravityAccepter>();
            if (ga)
            {
                continue;
            }
            //if the gravity scale is an accepted value, it's all good
            if (rb2d.gravityScale == 1)
            {
                continue;
            }
            //if the rb2d is frozen in place on an axis (eg, the sun), it's all good
            if (rb2d.constraints != RigidbodyConstraints2D.None && rb2d.constraints != RigidbodyConstraints2D.FreezeRotation)
            {
                continue;
            }
            //If none of the above are true, there's a problem!
            problemCount++;
            Debug.LogError(
                $"Object has a gravity scale of {rb2d.gravityScale}!\nIt should be 1, or you should give it a GravityAccepter.\nobject: {rb2d.name} in scene {rb2d.gameObject.scene.name}",
                rb2d
                );
        }
        return problemCount > 0;
    }

    [MenuItem("SG7/Build/Pre-Build/Check for illegal prefab overrides")]
    public static bool checkForIllegalPrefabOverrides()
    {
        //check for prefab object with unacceptable overrides (things like polygon shape, sprite color, etc; things that arent autosaved by the savableobject)
        //overrides are problematic if the object needs to be instantiated, because then its unique overrides won't be there
        //this tool was created mostly because the breakable walls in the cave area re-instatiated as white (instead of brown) when rewinding into the forest level

        List<string> allowedPropMods = new List<string>()
        {
            //GameObject
            "m_Name",
            "m_RootOrder",
            //Transform
            "m_LocalPosition",
            "m_LocalRotation",
            "m_LocalEulerAnglesHint",
            "m_ConstrainProportionsScale",
            "m_LocalScale",
            //SaveableObjectInfo
            "id",
            "spawnStateId",
            //Known Memory Objects
            "secretHiders",
            //TEMP allowances
            "m_ConnectedRigidBody",
            "m_ConnectedAnchor",
            "m_Points",
            "m_LocalAABB",
            "m_Spline",
            //2025-01-26: these ones i might decide to keep? related to minecart
            "m_AngularOffset",
            "m_LinearOffset",
            "m_Distance",
            "m_Target",
        };


        int overrideCount = 0;
        int problemCount = 0;
        foreach (SavableObjectInfo soi in GameObject.FindObjectsByType<SavableObjectInfo>(FindObjectsSortMode.None))
        {

            List<ObjectOverride> overrides = PrefabUtility.GetObjectOverrides(soi.gameObject);
            //overrides.ForEach(ovr =>
            //{
            //    Debug.Log($"override: {ovr}, {ovr.GetType()}");
            //});
            if (overrides.Count > 0)
            {
                overrideCount += overrides.Count;
            }

            //early exit: the object doesnt have to worry about overrides, go to the next one
            if (!couldPossiblyNeedToBeInstantiated(soi.gameObject))
            {
                continue;
            }

            int prevProblemCount = problemCount;

            problemCount += PrefabUtility.GetAddedComponents(soi.gameObject).Count;
            problemCount += PrefabUtility.GetAddedGameObjects(soi.gameObject).Count;
            problemCount += PrefabUtility.GetRemovedComponents(soi.gameObject).Count;
            problemCount += PrefabUtility.GetRemovedGameObjects(soi.gameObject).Count;
            List<PropertyModification> propmods = PrefabUtility.GetPropertyModifications(soi.gameObject).ToList();
            propmods.ForEach(propmod =>
            {
                //early exit: allowed propmod
                if (allowedPropMods.Contains(propmod.propertyPath)) { return; }
                string firstPartOfPropMod = propmod.propertyPath.Split(".")[0];
                if (allowedPropMods.Any(apm => apm.StartsWith(firstPartOfPropMod))) { return; }
                //propmod.target.GetType().CustomAttributes.ToList().ForEach(attr =>
                //{
                //    Debug.Log($"propmod === target {propmod.target.GetType()} type attr {attr}, {attr.AttributeType}");
                //});
                {
                    Debug.LogError($"Problematic prefab override!: {soi.gameObject.name}: target:{propmod.target?.GetType()},                              {propmod.propertyPath}: {propmod.value}", soi.gameObject);
                    problemCount++;
                }
            });
            overrideCount += propmods.Count;

            if (problemCount != prevProblemCount)
            {
                Debug.LogError($"GameObject {soi.gameObject.name} has {problemCount - prevProblemCount} problematic prefab overrides!", soi.gameObject);
            }

        }

        Debug.Log($"There are {overrideCount} overrides");
        if (problemCount > 0)
        {
            Debug.LogError($"There are {problemCount} problem overrides!");
        }


        return problemCount > 0;
    }
    private static bool couldPossiblyNeedToBeInstantiated(GameObject go)
    {
        return go.scene.name != "PlayerScene" && (
            go.GetComponent<Rigidbody2D>() ||
            go.GetComponent<IBlastable>() != null
            );
    }


    [MenuItem("SG7/Build/Pre-Build/Check for Ground layer objects")]
    public static bool checkForGroundLayerObjects()
    {
        //check for Ground layers: a gameobject should be in the Ground layer IFF it meets ALL the following conditions:
        //-has a SpriteShapeController
        //-does NOT have a Rigidbody2D
        //-has a Collider2D that is NOT a trigger
        //-is NOT savable (does NOT have a SavableObjectInfo)

        int groundLayer = LayerMask.NameToLayer("Ground");

        int problemCount = 0;
        GameObject.FindObjectsByType<Collider2D>(FindObjectsSortMode.None).ToList()
            .ForEach(coll2d =>
            {
                bool shouldGround = shouldBeGroundLayer(coll2d.gameObject);
                bool isGround = coll2d.gameObject.layer == groundLayer;
                if (isGround && !shouldGround)
                {
                    Debug.LogError($"GameObject {coll2d.gameObject.name} should NOT be on the Ground layer!", coll2d.gameObject);
                    problemCount++;
                }
                else if (!isGround && shouldGround)
                {
                    Debug.LogError($"GameObject {coll2d.gameObject.name} should be on the Ground layer!", coll2d.gameObject);
                    problemCount++;
                }
            });

        if (problemCount > 0)
        {
            Debug.LogError($"There are {problemCount} problems with objects and the Ground layer.");
        }

        return problemCount > 0;
    }
    private static bool shouldBeGroundLayer(GameObject go)
    {
        return go.GetComponent<SpriteShapeController>()
            && !go.GetComponent<Rigidbody2D>()
            && go.GetComponents<PolygonCollider2D>().Any(pc2d => !pc2d.isTrigger)
            && !go.GetComponent<SavableObjectInfo>();
    }


    [MenuItem("SG7/Build/Pre-Build/Check for solid triggers")]
    public static bool checkTriggersAreNotSolid()
    {
        int problemCount = 0;

        //game objects in this list should NOT have any solid colliders
        List<GameObject> goToCheck = new List<GameObject>();
        List<Type> typesList = new List<Type>()
        {
            typeof(SceneLoader),
            typeof(MusicZone),
            typeof(HiddenArea),
            typeof(GravityZone),
            typeof(EventTrigger),
        };
        typesList.ForEach(type =>
        {
            goToCheck.AddRange(GameObject.FindObjectsByType(type, FindObjectsSortMode.None).ToList().ConvertAll(mb => ((MonoBehaviour)mb).gameObject));
        });

        goToCheck.ForEach(go =>
        {
            bool anySolid = go.GetComponents<Collider2D>().Any(coll2d => !coll2d.isTrigger)
                || go.GetComponentsInChildren<Collider2D>().Any(coll2d => !coll2d.isTrigger);
            if (anySolid)
            {
                Debug.LogError($"GameObject {go.Name()} has  solid colliders!", go);
                problemCount++;
            }

        });

        if (problemCount > 0)
        {
            Debug.LogError($"There are {problemCount} game objects with solid colliders that shouldn't have them!");
        }

        return problemCount > 0;
    }


    [MenuItem("SG7/Build/Pre-Build/Ensure NPCs have dialogue set up")]
    public static bool ensureNPCsHaveDialogueTriggers()
    {
        int changedCount = 0;
        int problemCount = 0;
        GameObject.FindObjectsByType<Character>(FindObjectsSortMode.None).ToList()
            .ForEach(chr =>
            {
                if (chr)
                {
                    DialogueTrigger dt = chr.GetComponent<DialogueTrigger>() ?? chr.GetComponentInChildren<DialogueTrigger>();
                    if (!dt)
                    {
                        Debug.LogError($"Character {chr.gameObject.Name()} needs a DialogueTrigger!", chr);
                        problemCount++;
                        return;
                    }
                    if (!dt.characters.Contains(chr.characterName))
                    {
                        dt.characters.Add(chr.characterName);
                        Debug.LogWarning($"Character {chr.gameObject.Name()} now has dialogue trigger set up!", chr);
                        EditorUtility.SetDirty(dt);
                        changedCount++;
                    }
                }
            });
        if (problemCount > 0)
        {
            Debug.LogError($"There are {problemCount} characters with some setup problems!");
        }
        return changedCount > 0 || problemCount > 0;
    }

    [MenuItem("SG7/Build/Pre-Build/Check dialogue events")]
    public static bool checkDialogueEvents()
    {
        int changeCount = 0;
        GameObject.FindObjectsByType<ContinuallyCheckForDialogue>(FindObjectsSortMode.None).ToList()
            .ForEach(ccfd =>
            {
                var triggers = ccfd.GetComponents<EventTrigger>().ToList();
                if (ccfd.triggers.Count != triggers.Count)
                {
                    ccfd.triggers = triggers;
                    changeCount++;
                    Debug.LogWarning($"Populated triggers of ccfd {ccfd.name}. count: {ccfd.triggers.Count}", ccfd);
                }
            });
        return changeCount > 0;
    }

    [MenuItem("SG7/Build/Pre-Build/Check ISetupables")]
    public static bool checkISetupables()
    {
        (int changeCount, int errorCount) = _checkISetupables(
            GameObject.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None)
            .OfType<ISetupable>().ToList()
            );
        if (changeCount > 0)
        {
            Debug.LogWarning($"Check ISetupables: {changeCount} changes");
        }
        if (errorCount > 0)
        {
            Debug.LogError($"Check ISetupables: {errorCount} errors");
        }
        return changeCount > 0;
    }
    private static (int, int) _checkISetupables(List<ISetupable> list)
    {
        int changeCount = 0;
        int errorCount = 0;
        list.ForEach(setup =>
         {
             MonoBehaviour mb = (MonoBehaviour)setup;

             //Check for Errors
             int errors = setup.checkForErrors();
             if (errors > 0)
             {
                 Debug.LogError($"ISetupable: {mb.name} ({mb.GetType().Name}) errors: {errors}", mb);
                 errorCount += errors;
                 return;
             }

             //Setup
             int changes = setup.setup();
             if (changes > 0)
             {
                 EditorUtility.SetDirty(mb);
                 Debug.LogWarning($"ISetupable: {mb.name} ({mb.GetType().Name}) changes: {changes}", mb);
                 changeCount += changes;
             }
         });
        return (changeCount, errorCount);
    }

    [MenuItem("SG7/Build/Pre-Build/Check AutoInitialize Tags")]
    public static bool checkAutoInitializeTags()
    {
        (int changeCount, int errorCount) = _autoInitializeTags(
            GameObject.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include,FindObjectsSortMode.None).ToList()
            );
        if (changeCount > 0)
        {
            Debug.LogWarning($"Check AutoInitialize Tags: {changeCount} changes");
        }
        if (errorCount > 0)
        {
            Debug.LogError($"Check AutoInitialize Tags: {errorCount} errors");
        }
        return changeCount > 0 || errorCount > 0;
    }
    private static (int, int) _autoInitializeTags(List<MonoBehaviour> mbList)
    {
        Type TYPE_MONOBEHAVIOUR = typeof(MonoBehaviour);
        const string NULL_STRING = "null";

        int changeCount = 0;
        int errorCount = 0;

        mbList
            .ForEach(mb =>
            {
                string className = mb.GetType().Name;

                int changes = 0;
                int errors = 0;

                Type classInfo = mb.GetType();
                List<FieldInfo> fields = new List<FieldInfo>();
                while (classInfo != TYPE_MONOBEHAVIOUR)
                {
                    fields.AddRange(classInfo.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance));
                    classInfo = classInfo.BaseType;
                }

                fields
                    .Where(field => field.GetCustomAttribute<AutoInitialize>() != null)
                    .Where(field =>
                    {

                        object value = field.GetValue(mb);
                        return value == null
                            || ReferenceEquals(value, null)
                            //this string comparison seems weird, but its for Rigidbody2D, PolygonCollider2D and other Unity components
                            //that dont play nice with regular null checks
                            || $"{value}" == NULL_STRING
                            || field.FieldType.IsList();
                    })
                    .ToList()
                    .ForEach(field =>
                    {
                        //Check to make sure it is not an interface
                        if (field.FieldType.IsInterface)
                        {
                            Debug.LogError($"Field({className}.{field.Name}) is an interface. Unfortunately, Unity does not support serializing interfaces. Thus they also can't be auto-initialized", mb);
                            errors++;
                            return;
                        }

                        //Check to make sure it is public (or private with SerializeField)
                        if (!(field.IsPublic || field.GetCustomAttribute<SerializeField>() != null))
                        {
                            Debug.LogError($"Field ({className}.{field.Name}) has AutoInitialize tag but is not public! It needs to be public or have the SerializeField tag", mb);
                            errors++;
                            return;
                        }

                            AutoInitialize auto = field.GetCustomAttribute<AutoInitialize>();
                        //Set the value
                        if (field.FieldType.IsList())
                        {
                            IList list = (IList)field.GetValue(mb);
                            int prevcount = list.Count;
                            Type componentType = field.FieldType.GetGenericArguments()[0];

                            List<Component> compsToAdd = new List<Component>();
                            compsToAdd.AddRange(mb.GetComponents(componentType));
                            if (auto.SearchParent)
                            {
                                compsToAdd.AddRange(mb.GetComponentsInParent(componentType));
                            }
                            if (auto.SearchChildren)
                            {
                                compsToAdd.AddRange(mb.GetComponentsInChildren(componentType));
                            }
                            if (auto.SearchScene)
                            {
                                compsToAdd.AddRange((Component[])GameObject.FindObjectsByType(componentType,FindObjectsInactive.Include, FindObjectsSortMode.InstanceID));
                            }
                            compsToAdd.ForEach(comp =>
                            {
                                if (!list.Contains(comp))
                                {
                                    list.Add(comp);
                                }
                            });

                            if (list.Count != prevcount)
                            {
                                Debug.LogWarning($"Changing list field on {mb.name}: {className}.{field.Name}:{field.FieldType.Name}<{componentType.Name}> = {list}", mb);
                                field.SetValue(mb, list);
                                changes++;
                            }
                            else if (list.Count == 0)
                            {
                                if (auto.AllowUnfound)
                                {
                                    return;
                                }
                                Debug.LogError($"Components for list not found! {mb.name}: {className}.{field.Name}:{field.FieldType.Name}<{componentType.Name}>", mb);
                                errors++;
                                return;
                            }
                        }
                        else {
                        Component component = mb.GetComponent(field.FieldType);
                        if (!component)
                        {
                            if (!component && auto.SearchParent)
                            {
                                component = mb.GetComponentInParent(field.FieldType);
                            }
                            if (!component && auto.SearchChildren)
                            {
                                component = mb.GetComponentInChildren(field.FieldType);
                            }
                            if (!component && auto.SearchScene)
                            {
                                Scene scene = mb.gameObject.scene;
                                component = (Component)GameObject.FindObjectsByType(field.FieldType, FindObjectsSortMode.None)
                                    //don't allow setting it to an object in a different scene
                                    .FirstOrDefault(obj =>
                                        ((Component)obj).gameObject.scene == scene
                                    );
                                //Allow graceful exit if its a prefab expecting the component to be in the scene, and thus not in the prefab
                                if (component == null && scene.buildIndex < 0)
                                {
                                    return;
                                }
                            }
                            if (!component && auto.AllowUnfound)
                            {
                                return;
                            }
                            if (component == null)
                            {
                                Debug.LogError($"Component not found! {mb.name}: {className}.{field.Name}:{field.FieldType.Name}", mb);
                                errors++;
                                return;
                            }
                        }
                        Debug.LogWarning($"Changing field on {mb.name}: {className}.{field.Name}:{field.FieldType.Name} = {component}", mb);
                        field.SetValue(mb, component);
                        changes++;
                        }
                    });

                if (changes > 0)
                {
                    EditorUtility.SetDirty(mb);
                    Debug.LogWarning($"Check AutoInitialize Tags: {mb.name} ({className}) changes: {changes}", mb);
                    changeCount += changes;
                }
                if (errors > 0)
                {
                    if (errors > 1)
                    {
                        Debug.LogError($"Check AutoInitialize Tags: {mb.name} ({className}) errors: {errors}", mb);
                    }
                    errorCount += errors;
                }
            });
        return (changeCount, errorCount);
    }

    [MenuItem("SG7/Build/Pre-Build/Populate ObjectManager known objects list")]
    public static void populateObjectManagerKnownObjectsList()
    {
        Managers managers = GameObject.FindAnyObjectByType<Managers>();
        if (!managers.gameDataContainer)
        {
            Debug.LogError("Managers object needs to have a game data container!", managers);
            return;
        }
        int prevCount = managers.gameDataContainer._gameData.knownObjects.Count;
        managers.gameDataContainer._gameData.knownObjects = new List<SavableObjectInfoData>();
        List<GameObject> savables = new List<GameObject>();
        GameObject.FindObjectsByType<SceneSavableList>(FindObjectsSortMode.None).ToList()
            .ForEach(ssl =>
            {
                managers.gameDataContainer._gameData.knownObjects.AddRange(
                    ssl.savables.ConvertAll(
                        go => go.GetComponent<SavableObjectInfo>().Data
                        )
                    );
            });
        managers.gameDataContainer._gameData.knownObjects.OrderBy(soid => soid.id);
        int newCount = managers.gameDataContainer._gameData.knownObjects.Count;
        if (prevCount != newCount)
        {
            EditorUtility.SetDirty(managers.gameDataContainer);
            EditorUtility.SetDirty(managers);
            EditorSceneManager.MarkSceneDirty(managers.gameObject.scene);
            Debug.LogWarning(
                $"ObjectManager known objects count: {prevCount} -> {newCount}",
                managers.gameObject
                );
        }
    }


    /// <summary>
    /// Recursively gather all files under the given path including all its subfolders.
    /// 2021-07-12: copied from http://answers.unity.com/answers/916074/view.html
    /// </summary>
    static IEnumerable<string> GetFiles(string path)
    {
        Queue<string> queue = new Queue<string>();
        queue.Enqueue(path);
        while (queue.Count > 0)
        {
            path = queue.Dequeue();
            try
            {
                foreach (string subDir in Directory.GetDirectories(path))
                {
                    queue.Enqueue(subDir);
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError(ex.Message);
            }
            string[] files = null;
            try
            {
                files = Directory.GetFiles(path);
            }
            catch (System.Exception ex)
            {
                Debug.LogError(ex.Message);
            }
            if (files != null)
            {
                for (int i = 0; i < files.Length; i++)
                {
                    yield return files[i];
                }
            }
        }
    }

    [MenuItem("SG7/Build/Build Windows %w")]
    public static void buildWindows()
    {
        build(BuildTarget.StandaloneWindows, "exe");
    }
    [MenuItem("SG7/Build/Build Linux")]
    public static void buildLinux()
    {
        Debug.LogError(
            "Building Linux has not been readded yet after Unity removed it in 2019.2"
            );
    }
    [MenuItem("SG7/Build/Build Mac OS X")]
    public static void buildMacOSX()
    {
        build(BuildTarget.StandaloneOSX, "");
    }
    public static void build(BuildTarget buildTarget, string extension)
    {
        string defaultPath = getDefaultBuildPath();
        if (!System.IO.Directory.Exists(defaultPath))
        {
            System.IO.Directory.CreateDirectory(defaultPath);
        }
        //2017-10-19 copied from https://docs.unity3d.com/Manual/BuildPlayerPipeline.html
        // Get filename.
        string buildName = $"{defaultPath}/{PlayerSettings.productName}.{extension}";

        //Ask for confirmation to build
        bool shouldBuild = EditorUtility.DisplayDialog(
            "Build?",
            $"Build {PlayerSettings.productName} {PlayerSettings.bundleVersion}?",
            "Build",
            "Cancel"
            );
        // User hit the cancel button.
        if (!shouldBuild)
        {
            return;
        }

        string path = defaultPath;
        Debug.Log($"BUILDNAME: {buildName}");
        Debug.Log($"PATH: {path}");
        Debug.Log($"defaultPath: {defaultPath}");


        string[] levels = new string[EditorBuildSettings.scenes.Length];
        for (int i = 0; i < EditorBuildSettings.scenes.Length; i++)
        {
            if (EditorBuildSettings.scenes[i].enabled)
            {
                levels[i] = EditorBuildSettings.scenes[i].path;
            }
            else
            {
                break;
            }
        }

        // Build player.
        BuildPipeline.BuildPlayer(levels, buildName, buildTarget, BuildOptions.None);

        // Copy a file from the project folder to the build folder, alongside the built game.
        string resourcesPath = $"{path}/Assets/Resources";
        string dialogPath = $"{resourcesPath}/Dialogue";

        if (!System.IO.Directory.Exists(dialogPath))
        {
            System.IO.Directory.CreateDirectory(resourcesPath);
        }

        if (true || EditorUtility.DisplayDialog("Dialog Refresh", $"Refresh the voice acting entries in {dialogPath}?\n\nTHIS WILL DELETE EVERY FILE IN THAT DIRECTORY.", "Yep!", "Unacceptable."))
        {
            FileUtil.DeleteFileOrDirectory(dialogPath);
            FileUtil.CopyFileOrDirectory("Assets/Resources/Dialogue/", dialogPath);
        }

        // Run the game (Process class from System.Diagnostics).
        Process proc = new Process();
        proc.StartInfo.FileName = buildName;
        proc.Start();
    }

    [MenuItem("SG7/Run/Run Windows %#w")]
    public static void runWindows()
    {//2018-08-10: copied from build()
        string extension = "exe";
        string buildName = getBuildNamePath(extension);
        Debug.Log($"Launching: {buildName}");
        // Run the game (Process class from System.Diagnostics).
        Process proc = new Process();
        proc.StartInfo.FileName = buildName;
        proc.Start();
    }

    [MenuItem("SG7/Run/Open Build Folder #w")]
    public static void openBuildFolder()
    {
        string extension = "exe";
        string buildName = getBuildNamePath(extension);
        //Open the folder where the game is located
        EditorUtility.RevealInFinder(buildName);
    }

    [MenuItem("SG7/Run/Open App Data Folder &f")]
    public static void openAppDataFolder()
    {
        string filePath = $"{Application.persistentDataPath}/merky.txt";
        if (System.IO.File.Exists(filePath))
        {
            EditorUtility.RevealInFinder(filePath);
        }
        else
        {
            EditorUtility.RevealInFinder(Application.persistentDataPath);
        }
    }

    public static string getDefaultBuildPath()
    {
        return $"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}/Unity/Stonicorn/Builds/{PlayerSettings.productName}_{PlayerSettings.bundleVersion.Replace(".", "_")}";
    }
    public static string getBuildNamePath(string extension, bool checkFolderExists = true)
    {
        string defaultPath = getDefaultBuildPath();
        if (checkFolderExists && !System.IO.Directory.Exists(defaultPath))
        {
            throw new UnityException($"You need to build the {extension} for {PlayerSettings.productName} (Version {PlayerSettings.bundleVersion}) first!");
        }
        string buildName = $"{defaultPath}/{PlayerSettings.productName}.{extension}";
        return buildName;
    }

    [MenuItem("SG7/Session/Begin Session")]
    public static void beginSession()
    {
        Debug.Log("=== Beginning session ===");
        string oldVersion = PlayerSettings.bundleVersion;
        string[] split = oldVersion.Split('.');
        string majorVersion = $"{split[0]}";
        string minorVersion = $"{int.Parse(split[1]) + 1}".PadLeft(split[1].Length, '0');
        string newVersion = $"{majorVersion}.{minorVersion}";
        PlayerSettings.bundleVersion = newVersion;
        //Save and Log
        EditorSceneManager.SaveOpenScenes();
        Debug.LogWarning($"Updated build version number from {oldVersion} to {newVersion}");
    }

    [MenuItem("SG7/Session/Finish Session")]
    public static void finishSession()
    {
        Debug.Log("=== Finishing session ===");
        bool problems = performAllPreBuildTasks();
        if (!problems)
        {
            EditorSceneManager.SaveOpenScenes();
            buildWindows();
            //Open folders
            openBuildFolder();
        }
    }

    [MenuItem("SG7/Upgrade/Force save all assets")]
    public static void forceSaveAllAssets()
    {
        AssetDatabase.ForceReserializeAssets();
    }
}
