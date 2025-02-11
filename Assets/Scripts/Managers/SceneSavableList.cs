using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Saves a list of the game objects of the savable objects in its scene
/// </summary>
public class SceneSavableList : MonoBehaviour, ISetupable
{
    [AutoInitialize(SearchScene = true)]
    public List<SavableObjectInfo> savables = new List<SavableObjectInfo>();
    [AutoInitialize(SearchScene = true)]
    public List<MemoryObjectInfo> memories = new List<MemoryObjectInfo>();

    public static SceneSavableList getFromScene(Scene s)
        => s.GetRootGameObjects()
            .Where(go => go.GetComponent<SceneSavableList>())
            .FirstOrDefault()
            .GetComponent<SceneSavableList>();


#if UNITY_EDITOR

    public int checkForErrors()
    {
        int errorCount = 0;

        //anti-prefab check
        if (gameObject.scene.buildIndex < 0)
        {
            Debug.LogError($"SceneSavableList cannot be added to a prefab!");
            errorCount++;
        }

        //make sure everyone has savable object info that need it
        Utility.doForEachGameObjectInScene(
            gameObject.scene,
            (go) =>
            {
                if (go.isSavable())
                {
                    SavableObjectInfo soi = go.GetComponent<SavableObjectInfo>();
                    if (!soi)
                    {
                        Debug.LogError($"GameObject {go} is savable but doesnt have a SavableObjectInfo!");
                        errorCount++;
                    }
                }
            }
            );

        //make sure everyone has memory object info that need it
        Utility.doForEachGameObjectInScene(
            gameObject.scene,
            (go) =>
            {
                if (go.isMemory())
                {
                    MemoryObjectInfo moi = go.GetComponent<MemoryObjectInfo>();
                    if (!moi)
                    {
                        Debug.LogError($"GameObject {go} is a memory but doesnt have a MemoryObjectInfo!");
                        errorCount++;
                    }
                }
            }
            );

        return errorCount;
    }

    public int setup()
    {
        int changeCount = 0;

        //so far, no setup other than auto init savables and memories

        return changeCount;
    }
#endif
}
