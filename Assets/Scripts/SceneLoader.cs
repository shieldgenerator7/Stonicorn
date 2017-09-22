using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneLoader : MonoBehaviour
{

    public string sceneName;//the index of the scene to load
    private GameObject playerObj;
    private bool isLoaded = false;
    private Collider2D c2d;

    // Use this for initialization
    void Start()
    {
        c2d = gameObject.GetComponent<Collider2D>();
        playerObj = GameObject.FindGameObjectWithTag("Player");
        if (SceneManager.GetSceneByName(sceneName).isLoaded)
        {
            isLoaded = true;
        }
        check();
    }

    public void check()
    {
        bool overlaps = c2d.OverlapPoint(playerObj.transform.position);
        if (!isLoaded && overlaps)
        {
            isLoaded = true;
            loadLevel();
        }
        if (isLoaded && !overlaps)
        {
            isLoaded = false;
            unloadLevel();
        }
    }
    void loadLevel()
    {
        SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
    }
    void unloadLevel()
    {
        SceneManager.UnloadSceneAsync(sceneName);
    }
    public void unloadLevelIfLoaded()
    {
        if (isLoaded)
        {
            unloadLevel();
        }
    }
}
