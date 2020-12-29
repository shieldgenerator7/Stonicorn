using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Contains list of movable objects that are last known to be in this scene.
/// Necessary because sometimes objects move from scene to scene.
/// </summary>
public class SceneLoaderSavableList : MonoBehaviour
{
    public List<SavableObjectInfoData> datas = new List<SavableObjectInfoData>();

    private SceneLoader sceneLoader;

    private void Start()
    {
        sceneLoader = GetComponent<SceneLoader>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        SavableObjectInfo info = collision.gameObject.GetComponent<SavableObjectInfo>();
        if (info)
        {
            datas.Add(info.Data);
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        SavableObjectInfo info = collision.gameObject.GetComponent<SavableObjectInfo>();
        if (info)
        {
            datas.Remove(info.Data);
        }
    }
}
