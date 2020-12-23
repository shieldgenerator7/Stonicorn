﻿using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Saves a list of the game objects of the savable objects in its scene
/// </summary>
public class SceneSavableList : MonoBehaviour
{
    public List<GameObject> savables = new List<GameObject>();

#if UNITY_EDITOR
    public void refreshList()
    {
        savables.Clear();
        foreach (GameObject rgo in gameObject.scene.GetRootGameObjects())
        {
            if (rgo.isSavable())
            {
                savables.Add(rgo);
            }
            foreach (Transform t in rgo.transform)
            {
                GameObject go = t.gameObject;
                if (go.isSavable())
                {
                    savables.Add(go);
                }
            }
        }
        EditorUtility.SetDirty(gameObject);
        Debug.Log(
            "Found " + savables.Count + " savables in scene " + gameObject.scene.name,
            gameObject
            );
    }
#endif
}