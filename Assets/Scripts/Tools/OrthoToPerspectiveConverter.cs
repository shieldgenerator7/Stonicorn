using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrthoToPerspectiveConverter : MonoBehaviour
{
    public string sortingLayerFilter;
    public string sortingLayerToChangeTo;
    [Tooltip("How much to multiply the sorting order by to get the new Z depth. Lower Z depths get shown in front.")]
    public float zFactor;
    public List<string> excludeScenes;
    [Tooltip("The specific objects you want to convert. Leave the Size 0 if you want to convert all applicable objects (in the open scenes only).")]
    public List<SpriteRenderer> spritesToConvert;

    public void convert()
    {
        if (spritesToConvert == null || spritesToConvert.Count == 0)
        {
            selectAll();
        }
        //Filter the list for layer and scene
        List<SpriteRenderer> srs = new List<SpriteRenderer>();
        foreach (SpriteRenderer sr in spritesToConvert)
        {
            if (passesLayerFilter(sr)
                && passesSceneFilter(sr))
            {
                srs.Add(sr);
            }
        }
        //Convert them
        foreach (SpriteRenderer sr in srs)
        {
            //Convert to pos
            Vector3 pos = sr.transform.position;
            pos.z = sr.sortingOrder * zFactor;
            sr.transform.position = pos;
            //Convert sr settings
            if (sortingLayerToChangeTo != null && sortingLayerToChangeTo.Trim() != "")
            {
                sr.sortingLayerName = sortingLayerToChangeTo;
                sr.sortingOrder = 0;
            }
        }
        Debug.Log("Converted " + srs.Count + " sprites from ortho to perspective.");
    }

    private bool passesLayerFilter(SpriteRenderer sr)
    {
        return (sortingLayerFilter == null || sortingLayerFilter.Trim() != "")
            || sr.sortingLayerName == sortingLayerFilter;
    }

    private bool passesSceneFilter(SpriteRenderer sr)
    {
        return !excludeScenes.Contains(sr.gameObject.scene.name);
    }

    public void selectAll()
    {
        spritesToConvert = new List<SpriteRenderer>(FindObjectsOfType<SpriteRenderer>());
        Debug.Log("Selected " + spritesToConvert.Count + " sprites for conversion.");
    }
    public void settingsToDefault()
    {
        spritesToConvert = new List<SpriteRenderer>();
        excludeScenes = new List<string>(new string[] { "LoadingScreen", "MainMenu", "PlayerScene", "Tools" });
        sortingLayerFilter = "Background";
        sortingLayerToChangeTo = "Default";
        zFactor = 1;
        Debug.Log("Reset OrthoToPerspective Converter settings to default.");
    }
}
