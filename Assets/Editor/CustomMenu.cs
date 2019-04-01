using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using UnityEditor;
using UnityEngine.SceneManagement;

public class CustomMenu
{
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

    [MenuItem("SG7/Editor/Call Merky %#`")]
    public static void callMerky()
    {
        GameObject playerObject = GameManager.Player.gameObject;
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

    [MenuItem("SG7/Editor/Toggle Ruler %`")]
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

    [MenuItem("SG7/Editor/Call Ruler to its Range Preview #`")]
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
    [MenuItem("SG7/Editor/Toggle Ruler Range Preview &`")]
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
        UnityEngine.Debug.Log("Launching: " + buildName);
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
}
