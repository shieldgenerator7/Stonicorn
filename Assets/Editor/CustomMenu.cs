using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using UnityEditor;
using UnityEngine.SceneManagement;

public class CustomMenu
{
    [MenuItem("SG7/Editor/Change HideableArea to NonTeleportableArea")]
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

    [MenuItem("SG7/Editor/Call Merky %`")]
    public static void callMerky()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag(GameManager.playerTag);
        playerObject.transform.position = (Vector2)SceneView.GetAllSceneCameras()[0].transform.position;
        Selection.activeGameObject = playerObject;
    }

    [MenuItem("SG7/Editor/Hide or Unhide Hidden Areas %h")]
    public static void hideUnhideHiddenAreas()
    {
        bool changeDetermined = false;
        bool show = false;
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene s = SceneManager.GetSceneAt(i);
            if (s.isLoaded)
            {
                foreach (GameObject go1 in s.GetRootGameObjects())
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
                    }
                }
            }
        }
    }

    [MenuItem("SG7/Runtime/Save Game State %e")]
    public static void saveGameState()
    {
        UnityEngine.Debug.Log("SAVED");
        GameManager.Save();
    }

    [MenuItem("SG7/Runtime/Load Game State %#e")]
    public static void loadGameState()
    {
        UnityEngine.Debug.Log("LOADED");
        GameManager.LoadState();
    }

    [MenuItem("SG7/Runtime/Reload Game %#r")]
    public static void reloadGame()
    {
        GameManager.resetGame();
    }

    [MenuItem("SG7/Runtime/Activate All Checkpoints &c")]
    public static void activateAllCheckpoints()
    {
        foreach (CheckPointChecker cpc in GameObject.FindObjectsOfType<CheckPointChecker>())
        {
            cpc.activate();
        }
    }

    [MenuItem("SG7/Build/Build Windows %w")]
    public static void buildWindows()
    {
        build(BuildTarget.StandaloneWindows, "exe");
    }
    [MenuItem("SG7/Build/Build Linux %l")]
    public static void buildLinux()
    {
        build(BuildTarget.StandaloneLinux, "x86");
    }
    [MenuItem("SG7/Build/Build Mac OS X %#l")]
    public static void buildMacOSX()
    {
        build(BuildTarget.StandaloneOSX, "");
    }
    public static void build(BuildTarget buildTarget, string extension)
    {
        string defaultPath = "C:/Users/steph/Documents/Unity/Stoned Builds/Builds/" + PlayerSettings.productName;
        if (!System.IO.Directory.Exists(defaultPath))
        {
            System.IO.Directory.CreateDirectory(defaultPath);
        }
        //2017-10-19 copied from https://docs.unity3d.com/Manual/BuildPlayerPipeline.html
        // Get filename.
        string buildName = EditorUtility.SaveFilePanel("Choose Location of Built Game", defaultPath, PlayerSettings.productName, extension);
        string path = buildName.Substring(0, buildName.LastIndexOf("/"));
        UnityEngine.Debug.Log("BUILDNAME: " + buildName);
        UnityEngine.Debug.Log("PATH: " + path);
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
        BuildPipeline.BuildPlayer(levels, buildName, BuildTarget.StandaloneWindows, BuildOptions.None);

        // Copy a file from the project folder to the build folder, alongside the built game.
        //NOTE: Changes to the Dialogue folder won't reflected unless you delete the Dialogue folder in the build directory
        if (!System.IO.Directory.Exists(path + "/Assets/Resources/Dialogue"))
        {
            System.IO.Directory.CreateDirectory(path + "/Assets/Resources");
            FileUtil.CopyFileOrDirectory("Assets/Resources/Dialogue/",
                path + "/Assets/Resources/Dialogue/"
                );
        }

        // Run the game (Process class from System.Diagnostics).
        Process proc = new Process();
        proc.StartInfo.FileName = buildName;
        proc.Start();
    }
}
