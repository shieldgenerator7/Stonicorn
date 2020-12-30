using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Contains list of movable objects that are last known to be in this scene.
/// Necessary because sometimes objects move from scene to scene.
/// </summary>
public class SceneLoaderSavableList : MonoBehaviour
{
    private HashSet<SavableObjectInfoData> datas = new HashSet<SavableObjectInfoData>();

    private SceneLoader sceneLoader;
    private Collider2D coll2d;

    private void Start()
    {
        sceneLoader = GetComponent<SceneLoader>();
        coll2d = GetComponent<Collider2D>();
    }

    public delegate void OnObjectMoved(GameObject go);

    private void OnTriggerEnter2D(Collider2D collision)
    {
        SavableObjectInfo info = collision.gameObject.GetComponent<SavableObjectInfo>();
        if (info)
        {
            onObjectEntered?.Invoke(info.gameObject);
            datas.Add(info.Data);
        }
    }
    public event OnObjectMoved onObjectEntered;
    private void OnTriggerExit2D(Collider2D collision)
    {
        SavableObjectInfo info = collision.gameObject.GetComponent<SavableObjectInfo>();
        if (info)
        {
            onObjectExited?.Invoke(info.gameObject);
            datas.Remove(info.Data);
        }
    }
    public event OnObjectMoved onObjectExited;

    public void add(GameObject go)
        => datas.Add(go.GetComponent<SavableObjectInfo>().Data);

    public bool contains(GameObject go)
        => datas.Contains(go.GetComponent<SavableObjectInfo>().Data);

    public void remove(GameObject go)
        => datas.Remove(go.GetComponent<SavableObjectInfo>().Data);

    public bool overlapsPosition(GameObject go)
        => coll2d.OverlapPoint(go.transform.position);

    public bool overlapsCollider(GameObject go)
        => coll2d.OverlapsCollider(go.GetComponent<Collider2D>());

    public Scene Scene
        => sceneLoader.Scene;
}
