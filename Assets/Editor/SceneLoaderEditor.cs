using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[CustomEditor(typeof(SceneLoader))]
[CanEditMultipleObjects]
public class SceneLoaderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GUI.enabled = !EditorApplication.isPlaying;
        if (GUILayout.Button("Autoset Scene Id (Edit Mode)"))
        {
            targets.ToList().ForEach(t =>
            {
                SceneLoader sl = (SceneLoader)t;
                Scene scene = EditorSceneManager.GetSceneByName(sl.sceneName);
                sl.sceneId = scene.buildIndex;
                EditorUtility.SetDirty(sl);
                EditorSceneManager.MarkSceneDirty(scene);
            });
        }
    }
}
