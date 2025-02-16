using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingScreen : MonoBehaviour, ISetupable
{
    public string mainSceneName;
    public float growSpeed = 0.5f;

    [AutoInitialize(SearchScene =true)]
    public Camera initialCamera;
    [AutoInitialize(SearchScene = true)]
    public SplashScreenUpdater splashScreenUpdater;

    //Runtime vars
    private List<AsyncOperation> operations = new List<AsyncOperation>();
    private AsyncOperationHandle addressableLoadOperation;
    private float targetFillAmount = 0;//the fill amount that Image.fillAmount should get to
    private bool finishedLoading = false;
    private bool finishedSplashScreen = false;

    //Singleton
    private static LoadingScreen instance;

    //Components
    [AutoInitialize(SearchChildren = true), SerializeField, HideInInspector]
    private List<Image> images;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        //Managers.Time.Paused = false;
        Time.timeScale = 0;
        //Set Splash Screen delegate
        splashScreenUpdater.onSplashScreenFinished += splashScreenFinished;
        //Disable this script until splash screen finishes
        this.enabled = false;
        //Allow app to load in background
        Application.runInBackground = true;
    }

    private void OnDestroy()
    {
        Application.runInBackground = false;
    }

    private void Update()
    {
        //calculate targetFillAmount
        float sum = 0;
        int count = Mathf.Max(2, instance.operations.Count);
        sum = operations.Sum(op => op.progress);
        if (addressableLoadOperation.IsValid())
        {
            sum += addressableLoadOperation.PercentComplete;
            count += 1;
        }
        targetFillAmount = sum / (float)count;

        //update loading bars
        foreach (Image image in images)
        {
            if (image.fillAmount != targetFillAmount)
            {
                image.fillAmount = Mathf.MoveTowards(image.fillAmount, targetFillAmount, growSpeed * Time.unscaledDeltaTime);
            }
            if (image.fillAmount == 1)
            {
                Fader f = transform.parent.gameObject.AddComponent<Fader>();
                f.ignorePause = true;
                f.finishAction = Fader.FinishAction.DESTROY_SCRIPT;
                f.onFadeFinished += loadingFinished;
                this.enabled = false;
                //Switch camera to main camera
                Destroy(initialCamera.gameObject);
                break;
            }
        }
    }

    //2019-01-09: copied from https://www.youtube.com/watch?v=YMj2qPq9CP8
    void LoadMainSceneAsynchronously(string mainSceneName)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(mainSceneName, LoadSceneMode.Additive);
        operation.completed += (ao) => passActiveScene();
        operations.Add(operation);        
    }
    public static void LoadScene(int sceneId)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneId, LoadSceneMode.Additive);

        if (instance)
        {
            instance.operations.Add(operation);
        }
    }
    void passActiveScene()
    {
        Scene mainScene = SceneManager.GetSceneByName(mainSceneName);
        if (mainScene.isLoaded)
        {
            SceneManager.SetActiveScene(mainScene);
        }
    }

    void loadingFinished()
    {
        finishedLoading = true;
        if (MenuManager.Open)
        {
            Managers.Time.setPause(this, true);
        }
        checkUnload();
    }
    void splashScreenFinished()
    {
        if (!finishedSplashScreen)
        {
            finishedSplashScreen = true;
            checkUnload();

            //Load start scenes
            LoadMainSceneAsynchronously(mainSceneName);

            //Load addressable
            addressableLoadOperation = Addressables.InitializeAsync();

            //enable
            this.enabled = true;
        }
    }
    void checkUnload()
    {
        if (finishedLoading && finishedSplashScreen)
        {
            instance = null;
            Managers.Time.setPause(this, false);
            SceneManager.UnloadSceneAsync("LoadingScreen");
        }
    }

    public int setup()
    {
        int changeCount = 0;

        foreach (Image image in images)
        {
            if (image.fillAmount != 0)
            {
                image.fillAmount = 0;
                changeCount++;
            }
        }

        return changeCount;
    }

    public static bool FinishedLoading
    {
        get => !instance || instance.finishedLoading;
    }
}
