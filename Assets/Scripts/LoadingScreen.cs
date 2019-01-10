﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingScreen : MonoBehaviour
{
    public string sceneName;
    public float growSpeed = 0.5f;

    //Runtime vars
    private List<AsyncOperation> operations = new List<AsyncOperation>();
    private float targetFillAmount = 0;//the fill amount that Image.fillAmount should get to

    //Singleton
    private static LoadingScreen instance;

    //Components
    private Image image;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        image = GetComponent<Image>();
        image.fillAmount = 0;
        //Load start scenes
        StartCoroutine(LoadSceneAsynchronously(sceneName));
    }

    private void Update()
    {
        if (image.fillAmount != targetFillAmount)
        {
            image.fillAmount = Mathf.MoveTowards(image.fillAmount, targetFillAmount, growSpeed * Time.deltaTime);
        }
        if (image.fillAmount == 1)
        {
            SceneManager.UnloadSceneAsync("LoadingScreen");
        }
    }

    //2019-01-09: copied from https://www.youtube.com/watch?v=YMj2qPq9CP8
    static IEnumerator LoadSceneAsynchronously(string sceneName)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        operation.completed += instance.passActiveScene;
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
            instance.targetFillAmount = percentDone;
            yield return null;
        }
    }
    public static void LoadScene(string sceneName)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

        if (instance)
        {
            instance.operations.Add(operation);
        }
    }
    void passActiveScene(AsyncOperation ao)
    {
        Scene mainScene = SceneManager.GetSceneByName(sceneName);
        if (mainScene.isLoaded)
        {
            SceneManager.SetActiveScene(mainScene);
        }
    }
}