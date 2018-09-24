using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CheckPointChecker))]
public class CheckPointCheckerEditor : Editor
{

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        CheckPointChecker cpc = (CheckPointChecker)target;
        GUI.enabled = EditorApplication.isPlaying;
        if (GUILayout.Button("Generate Preview Sprite (Play Mode)"))
        {
            if (!EditorApplication.isPlaying)
            {
                throw new UnityException("You must be in Play Mode to use this function!");
            }
            string filename = cpc.grabCheckPointCameraData();
            string srcFolder = "C:/Users/steph/AppData/LocalLow/shieldgenerator7/"+Application.productName+"/";
            string dstFolder = "Assets/Sprites/Checkpoints/";
            string dstFile = dstFolder + filename;
            FileUtil.DeleteFileOrDirectory(dstFile);
            FileUtil.MoveFileOrDirectory(srcFolder + filename, dstFile);
        }
        GUI.enabled = !EditorApplication.isPlaying;
        if (GUILayout.Button("Fetch Preview Sprite (Edit Mode)"))
        {
            if (EditorApplication.isPlaying)
            {
                throw new UnityException("You must be in Edit Mode to use this function!");
            }
            Debug.Log("Sorry, \"Fetch Preview Sprite\" doesn't work. You're going to have to go and update it yourself." +
                "\n The sprite will be in Assets/Sprites/Checkpoints/"+cpc.name+".png");
        }
    }
}
