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

public class CustomMenu
{
    const int FIRST_LEVEL_INDEX = 4;

    [MenuItem("SG7/Editor/Terrain/Focus Terrain Tool %T")]
    public static void levelTerrainPoints()
    {
        SpriteShapeTool sst = GameObject.FindObjectOfType<SpriteShapeTool>();
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
        Debug.Log(string.Format("Searched {0} GameObjects, {1} components, found {2} missing", go_count, components_count, missing_count));
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
                    s = t.parent.name + "/" + s;
                    t = t.parent;
                }
                Debug.Log(s + " has an empty script attached in position: " + i, g);
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
        PlayerController pc = GameObject.FindObjectOfType<PlayerController>();
        GameObject.FindObjectsOfType<MilestoneActivatorAbility>(true).ToList()
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
                        "Player does not have an ability called "
                            + maa.abilityTypeName + "!",
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
            GameObject.FindObjectOfType<EditorCameraRotatorObject>();
        ecro.toggle();
    }

    [MenuItem("SG7/Editor/Connect selected Lantern and HiddenArea %#H")]
    public static void connectLanternToHiddenArea()
    {
        HiddenAreaConnector hac = GameObject.FindObjectOfType<HiddenAreaConnector>();
        Selection.activeGameObject = hac.gameObject;
        hac.connect();
    }

    [MenuItem("SG7/Editor/List Prefabs")]
    public static void listPrefabs()
    {
        GameObject.FindObjectsOfType<SavableObjectInfo>().ToList()
            .FindAll(soi => soi.PrefabAddress.editorAsset != null)
            .OrderBy(soi => soi.PrefabAddress.editorAsset.name).ToList()
            .ForEach(soi =>
                Debug.Log("Prefab: " + soi.PrefabAddress.editorAsset.name, soi.gameObject)
            );
    }

    [MenuItem("SG7/Editor/Spawn Point/Toggle Merky Spawn Point %#`")]
    public static void callMerky()
    {
        if (Application.isEditor && !Application.isPlaying)
        {
            PlayerTestSpawnPoint playerTSP = GameObject.FindObjectOfType<PlayerTestSpawnPoint>();
            GameObject playerSpawnObject = playerTSP.gameObject;

            //Enable it
            playerTSP.enabled = true;
            playerSpawnObject.SetActive(true);
            RulerDisplayer rd = GameObject.FindObjectOfType<RulerDisplayer>();
            if (rd)
            {
                rd.transform.position = RulerDisplayer.currentMousePos;
            }
            else
            {
                playerSpawnObject.transform.position = (Vector2)SceneView.GetAllSceneCameras()[0].transform.position;
            }
            Selection.activeGameObject = playerSpawnObject;
            Debug.Log("PTSP enabled: " + playerTSP.enabled);
        }
        else
        {
            //Call the player
            GameObject playerObject = GameObject.FindObjectOfType<PlayerController>().gameObject;
            if (GameObject.FindObjectOfType<RulerDisplayer>())
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
            PlayerTestSpawnPoint playerTSP = GameObject.FindObjectOfType<PlayerTestSpawnPoint>();
            playerTSP.enabled = false;
            GameObject playerSpawnObject = playerTSP.gameObject;
            playerSpawnObject.SetActive(true);
            //Select player object
            GameObject playerObject = GameObject.FindObjectOfType<PlayerController>().gameObject;
            Selection.activeGameObject = playerObject;
            Debug.Log("PTSP enabled: " + playerTSP.enabled);
        }
    }

    [MenuItem("SG7/Editor/Ruler/Toggle Ruler %`")]
    /// <summary>
    /// Turns the ruler tools on and off
    /// </summary>
    public static void toggleRulers()
    {
        bool anyOn = false;
        foreach (RulerDisplayer rd in GameObject.FindObjectsOfType<RulerDisplayer>())
        {
            if (rd.active)
            {
                anyOn = true;
                break;
            }
        }
        foreach (RulerDisplayer rd in GameObject.FindObjectsOfType<RulerDisplayer>())
        {
            rd.active = !anyOn;
        }
        anyOn = !anyOn;
        //If the rulers are activating,
        if (anyOn)
        {
            //turn off all the range previews
            foreach (RulerRangePreview rrp in GameObject.FindObjectsOfType<RulerRangePreview>())
            {
                rrp.Active = false;
            }
        }
    }

    [MenuItem("SG7/Editor/Ruler/Call Ruler to its Range Preview #`")]
    /// <summary>
    /// Repositions the ruler to its range preview's position
    /// </summary>
    public static void callRulerToPreview()
    {
        foreach (RulerRangePreview rrp in GameObject.FindObjectsOfType<RulerRangePreview>())
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
        foreach (RulerRangePreview rrp in GameObject.FindObjectsOfType<RulerRangePreview>())
        {
            if (rrp.Active)
            {
                anyOn = true;
                break;
            }
        }
        foreach (RulerRangePreview rrp in GameObject.FindObjectsOfType<RulerRangePreview>())
        {
            rrp.Active = !anyOn;
        }
    }

    [MenuItem("SG7/Editor/Load or Unload Level Scenes %&S")]
    public static void loadOrUnloadLevelScenes()
    {
        //Find out if all of the scenes are loaded
        bool allLoaded = true;
        for (int i = FIRST_LEVEL_INDEX; i < EditorBuildSettings.scenes.Length; i++)
        {
            if (!EditorSceneManager.GetSceneByBuildIndex(i).isLoaded)
            {
                allLoaded = false;
                break;
            }
        }
        //If any are loaded, unload them all.
        //Else, load them all.
        loadAllLevelScenes(!allLoaded);
    }
    public static void loadAllLevelScenes(bool load)
    {
        //Load or unload all the level scenes
        for (int i = FIRST_LEVEL_INDEX; i < EditorBuildSettings.scenes.Length; i++)
        {
            Scene scene = EditorSceneManager.GetSceneByBuildIndex(i);
            if (!load)
            {
                //Unload
                EditorSceneManager.CloseScene(scene, false);
            }
            else
            {
                //Load
                EditorSceneManager.OpenScene(
                    "Assets/Scenes/Levels/" + scene.name + ".unity",
                    OpenSceneMode.Additive
                    );
                SetExpanded(scene, false);
            }
        }
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
        Physics2D.alwaysShowColliders = !Physics2D.alwaysShowColliders;
    }

    [MenuItem("SG7/Editor/Log Objects %l")]
    public static void logObjects()
    {
        Logger logger = GameObject.FindObjectOfType<Logger>();
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
        loadAllLevelScenes(false);
        loadAllLevelScenes(true);
        while (!allLevelScenesLoaded())
        {
            new WaitForSecondsRealtime(0.1f);
        }

        //Checklist
        bool keepScenesOpen = false;
        refreshSceneSavableObjectLists();
        //(new List<Func<bool>>()).ForEach(func => keepScenesOpen = keepScenesOpen || func);
        keepScenesOpen = ensureSavableObjectsHaveObjectInfo() || keepScenesOpen;
        keepScenesOpen = ensureMemoryObjectsHaveObjectInfo() || keepScenesOpen;
        keepScenesOpen = ensureUniqueObjectIDs() || keepScenesOpen;
        keepScenesOpen = ensureHiddenAreasAreProperlySetup() || keepScenesOpen;
        keepScenesOpen = checkTiledHitBoxes() || keepScenesOpen;

        populateObjectManagerKnownObjectsList();

        //Cleanup
        EditorSceneManager.SaveOpenScenes();
        if (!keepScenesOpen)
        {
            loadAllLevelScenes(false);
        }
        //Finish
        Debug.Log("Finished all Pre-Build Tasks");
        return keepScenesOpen;
    }

    static bool allLevelScenesLoaded()
    {
        for (int i = FIRST_LEVEL_INDEX; i < EditorBuildSettings.scenes.Length; i++)
        {
            Scene scene = EditorSceneManager.GetSceneByBuildIndex(i);
            if (!scene.isLoaded)
            {
                return false;
            }
        }
        return true;
    }
    [MenuItem("SG7/Build/Pre-Build/Refresh Scene Savable Object Lists")]
    public static void refreshSceneSavableObjectLists()
    {
        GameObject.FindObjectsOfType<SceneSavableList>().ToList()
            .ForEach(ssl => ssl.refreshList());
    }

    [MenuItem("SG7/Build/Pre-Build/Ensure savable objects have ObjectInfo")]
    public static bool ensureSavableObjectsHaveObjectInfo()
    {
        List<GameObject> savables = new List<GameObject>();
        GameObject.FindObjectsOfType<SceneSavableList>().ToList()
            .ForEach(ssl => savables.AddRange(ssl.savables));
        //Missing ObjectInfo
        List<GameObject> missingInfo = savables
            .FindAll(go => !go.GetComponent<SavableObjectInfo>());
        missingInfo.ForEach(
            go => Debug.LogError(go.name + " does not have an SavableObjectInfo!", go)
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
            go => Debug.LogError(go.name + " has SavableObjectInfo with missing prefabGUID!", go)
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
                    go.name + " has non-zero spawn state; zeroing it out..."
                    + info.spawnStateId,
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
        GameObject.FindObjectsOfType<SceneSavableList>().ToList()
            .ForEach(ssl => memories.AddRange(ssl.memories));
        //Missing ObjectInfo
        List<GameObject> missingInfo = memories
            .FindAll(go => !go.GetComponent<MemoryObjectInfo>());
        missingInfo.ForEach(
            go => Debug.LogError(go.name + " does not have an MemoryObjectInfo!", go)
            );
        return missingInfo.Count > 0;
    }
    [MenuItem("SG7/Build/Pre-Build/Ensure unique object IDs among open scenes")]
    public static bool ensureUniqueObjectIDs()
    {
        int nextID = 10;
        bool changedId = false;
        const int SECTION_SIZE = 1000;
        foreach (SceneSavableList ssl in GameObject.FindObjectsOfType<SceneSavableList>())
        {
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
                            "Changed Id: " + prevID + " -> " + id,
                            info.gameObject
                            );
                        EditorUtility.SetDirty(info);
                        EditorSceneManager.MarkSceneDirty(info.gameObject.scene);
                        changedId = true;
                    }
                });
            //Make nextID start at the next section
            nextID = (int)Mathf.Floor(nextID / SECTION_SIZE) * SECTION_SIZE + SECTION_SIZE;
        }
        return changedId;
    }

    [MenuItem("SG7/Build/Pre-Build/Ensure Hidden Areas are Properly Setup")]
    public static bool ensureHiddenAreasAreProperlySetup()
    {
        int changedCount = 0;
        string TAG = "NonTeleportableArea";
        string UNTAG = "Untagged";
        GameObject.FindObjectsOfType<HiddenArea>().ToList()
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
                                    "Changed " + go.name + " tag to " + TAG + ".",
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
                                    "Changed " + go.name + " tag to " + UNTAG + ".",
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
                                "Changed " + go.name + " pos to " + go.transform.position + ".",
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
                                    "Changed " + go.name + " layer name to " + layerName + ".",
                                    go
                                    );
                                changedCount++;
                            }
                            if (!coll2d)
                            {
                                Debug.LogError(
                                    go.name + " has renderer without a collider!",
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
                                    "Changed " + go.name + " collider isTrigger to " + coll2d.isTrigger + ".",
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
                "HiddenArea changes: Made " + changedCount + " changes."
                );
        }
        return changedCount > 0;
    }

    [MenuItem("SG7/Build/Pre-Build/Check Tiled HitBoxes")]
    public static bool checkTiledHitBoxes()
    {
        int changedCount = 0;
        GameObject.FindObjectsOfType<SpriteRenderer>().ToList()
            .FindAll(sr => sr.drawMode == SpriteDrawMode.Tiled)
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
                        "Changed " + sr.name + " sprite size " +
                        "from (" + oldSRSize.x + ", " + oldSRSize.y + ") " +
                        "to (" + newSRSize.x + ", " + newSRSize.y + ").",
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
                            "Changed " + sr.name + " collider size " +
                            "from (" + oldSize.x + ", " + oldSize.y + ") " +
                            "to (" + newSize.x + ", " + newSize.y + ").",
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
                "Tiled sprites changes: Made " + changedCount + " changes."
                );
        }
        return changedCount > 0;
    }

    [MenuItem("SG7/Build/Pre-Build/Populate ObjectManager known objects list")]
    public static void populateObjectManagerKnownObjectsList()
    {
        Managers managers = GameObject.FindObjectOfType<Managers>();
        int prevCount = managers.gameData.knownObjects.Count;
        managers.gameData.knownObjects = new List<SavableObjectInfoData>();
        List<GameObject> savables = new List<GameObject>();
        GameObject.FindObjectsOfType<SceneSavableList>().ToList()
            .ForEach(ssl =>
            {
                managers.gameData.knownObjects.AddRange(
                    ssl.savables.ConvertAll(
                        go => go.GetComponent<SavableObjectInfo>().Data
                        )
                    );
            });
        managers.gameData.knownObjects.OrderBy(soid => soid.id);
        int newCount = managers.gameData.knownObjects.Count;
        if (prevCount != newCount)
        {
            EditorUtility.SetDirty(managers);
            EditorSceneManager.MarkSceneDirty(managers.gameObject.scene);
            Debug.LogWarning(
                "ObjectManager known objects count: " + prevCount + " -> " + newCount,
                managers.gameObject
                );
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
        string buildName = EditorUtility.SaveFilePanel("Choose Location of Built Game", defaultPath, PlayerSettings.productName, extension);

        // User hit the cancel button.
        if (buildName == "")
            return;

        string path = buildName.Substring(0, buildName.LastIndexOf("/"));
        Debug.Log("BUILDNAME: " + buildName);
        Debug.Log("PATH: " + path);

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
        string resourcesPath = path + "/Assets/Resources";
        string dialogPath = resourcesPath + "/Dialogue";

        if (!System.IO.Directory.Exists(dialogPath))
        {
            System.IO.Directory.CreateDirectory(resourcesPath);
        }

        if (true || EditorUtility.DisplayDialog("Dialog Refresh", "Refresh the voice acting entries in " + dialogPath + "?\n\nTHIS WILL DELETE EVERY FILE IN THAT DIRECTORY.", "Yep!", "Unacceptable."))
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
        Debug.Log("Launching: " + buildName);
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
        string filePath = Application.persistentDataPath + "/merky.txt";
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
        return System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments) + "/Unity/Stoned Builds/Builds/" + PlayerSettings.productName + "_" + PlayerSettings.bundleVersion.Replace(".", "_");
    }
    public static string getBuildNamePath(string extension, bool checkFolderExists = true)
    {
        string defaultPath = getDefaultBuildPath();
        if (checkFolderExists && !System.IO.Directory.Exists(defaultPath))
        {
            throw new UnityException("You need to build the " + extension + " for " + PlayerSettings.productName + " (Version " + PlayerSettings.bundleVersion + ") first!");
        }
        string buildName = defaultPath + "/" + PlayerSettings.productName + "." + extension;
        return buildName;
    }

    [MenuItem("SG7/Session/Begin Session")]
    public static void beginSession()
    {
        Debug.Log("=== Beginning session ===");
        string oldVersion = PlayerSettings.bundleVersion;
        string[] split = oldVersion.Split('.');
        string newVersion = split[0] + "." + (int.Parse(split[1]) + 1);
        PlayerSettings.bundleVersion = newVersion;
        //Save and Log
        EditorSceneManager.SaveOpenScenes();
        Debug.LogWarning("Updated build version number from " + oldVersion + " to " + newVersion);
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
