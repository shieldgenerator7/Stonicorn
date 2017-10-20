using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using UnityEditor;

public class CustomMenu {
    
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

    [MenuItem("SG7/Build/Build Windows %w")]
    public static void buildWindows()
    {
        //2017-10-19 copied from https://docs.unity3d.com/Manual/BuildPlayerPipeline.html
        // Get filename.
        string buildName = EditorUtility.SaveFilePanel("Choose Location of Built Game", "C:/Users/shieldgenerator7/Documents/Unity/Stoned Builds/Builds", "Stonicorn 0_100", "exe");
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
