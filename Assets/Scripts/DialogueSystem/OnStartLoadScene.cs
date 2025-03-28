using UnityEngine;
using UnityEngine.SceneManagement;

public class OnStartLoadScene : MonoBehaviour
{
    public string sceneName;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
    }
    private void OnDestroy()
    {
        SceneManager.UnloadSceneAsync(sceneName);
    }
}
