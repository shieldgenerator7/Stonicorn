using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Saves a list of the game objects of the savable objects in its scene
/// </summary>
public class SceneSavableList : MonoBehaviour
{
    public List<GameObject> savables = new List<GameObject>();
    public List<GameObject> memories = new List<GameObject>();

    public static SceneSavableList getFromScene(Scene s)
        => s.GetRootGameObjects().ToList()
            .Find(go => go.GetComponent<SceneSavableList>())
            .GetComponent<SceneSavableList>();


#if UNITY_EDITOR
    public void refreshList()
    {
        //Savables
        savables.Clear();
        Utility.doForEachGameObjectInScene(
            gameObject.scene,
            (go) =>
            {
                if (go.isSavable())
                {
                    savables.Add(go);
                }
            }
            );
        //Memories
        memories.Clear();
        Utility.doForEachGameObjectInScene(
            gameObject.scene,
            (go) =>
            {
                if (go.isMemory())
                {
                    memories.Add(go);
                }
            }
            );
        //Save
        EditorUtility.SetDirty(gameObject);
        Debug.Log(
            $"Found {savables.Count} savables " +
            $"and {memories.Count} memories " +
            $"in scene {gameObject.scene.name}",
            gameObject
            );
    }
#endif
}
