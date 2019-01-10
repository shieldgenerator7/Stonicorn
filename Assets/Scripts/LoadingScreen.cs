using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingScreen : MonoBehaviour
{
    public string sceneName;

    private List<AsyncOperation> operations = new List<AsyncOperation>();

    private static LoadingScreen instance;

    private Image image;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        image = GetComponent<Image>();
        //Load start scenes
        StartCoroutine(LoadSceneAsynchronously(sceneName));
    }

    //2019-01-09: copied from https://www.youtube.com/watch?v=YMj2qPq9CP8
    static IEnumerator LoadSceneAsynchronously(string sceneName)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

        if (instance)
        {
            instance.operations.Add(operation);
            float percentDone = 0;
            while (percentDone < 1)
            {
                float sum = 0;
                int count = Mathf.Max(2, instance.operations.Count);
                foreach (AsyncOperation ao in instance.operations)
                {
                    sum += ao.progress;
                }
                percentDone = sum / count;
                instance.image.fillAmount = percentDone;
                yield return null;
            }
        }
        SceneManager.SetActiveScene(SceneManager.GetSceneByName("PlayerScene"));
        SceneManager.UnloadSceneAsync("LoadingScreen");
    }
    public static void LoadScene(string sceneName)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

        if (instance)
        {
            instance.operations.Add(operation);
        }
    }
}
