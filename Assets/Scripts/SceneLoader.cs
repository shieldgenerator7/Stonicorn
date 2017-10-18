using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneLoader : MonoBehaviour
{

    public string sceneName;//the index of the scene to load
    public int lastOpenGameStateId = -1;//the gamestate id in which this scene was last open in. -1 means it is not open in any of them
    private GameObject playerObj;
    private bool isLoaded = false;
    private Collider2D c2d;

    // Use this for initialization
    void Start()
    {
        if (gameObject.name == "Easy Save 2 Loaded Component")
        {
            return;
        }
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
            if (!SceneManager.GetSceneByName(sceneName).isLoaded)
            {
                loadLevel();
            }
        }
        if (!GameManager.isRewinding())
        {
            if (isLoaded && !overlaps)
            {
                isLoaded = false;
                unloadLevel();
            }
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