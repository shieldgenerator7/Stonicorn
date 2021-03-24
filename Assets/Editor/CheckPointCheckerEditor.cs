using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[CustomEditor(typeof(CheckPointChecker))]
[CanEditMultipleObjects]
public class CheckPointCheckerEditor : Editor
{

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        GUI.enabled = EditorApplication.isPlaying;
        if (GUILayout.Button("Generate Preview Sprite (Play Mode)"))
        {
            new List<Object>(targets).ForEach(
                c =>
                {
                    CheckPointChecker cpc = (CheckPointChecker)c;
                    string filename = cpc.grabCheckPointCameraData();
                    string srcFolder = Application.persistentDataPath + "/";
                    string dstFolder = "Assets/Sprites/Checkpoints/";
                    string dstFile = dstFolder + filename;
                    FileUtil.DeleteFileOrDirectory(dstFile);
                    FileUtil.MoveFileOrDirectory(srcFolder + filename, dstFile);
                });
        }
        if (GUILayout.Button("Toggle InWorkingOrder (Play Mode)"))
        {
            new List<Object>(targets).ForEach(
                c =>
                {
                    CheckPointChecker cpc = (CheckPointChecker)c;
                    cpc.InWorkingOrder = !cpc.InWorkingOrder;
                });
        }
        GUI.enabled = !EditorApplication.isPlaying;
        if (GUILayout.Button("Fetch Preview Sprite (Edit Mode)"))
        {
            new List<Object>(targets).ForEach(
                c =>
                {
                    CheckPointChecker cpc = (CheckPointChecker)c;
                    Sprite ghostSprite = (Sprite)AssetDatabase.LoadAssetAtPath(
                        "Assets/Sprites/Checkpoints/" + cpc.transform.parent.name + ".png",
                        typeof(Sprite)
                        );
                    cpc.ghostSprite = ghostSprite;
                    EditorUtility.SetDirty(cpc);
                    EditorSceneManager.MarkSceneDirty(cpc.gameObject.scene);
                });
        }
    }
}
