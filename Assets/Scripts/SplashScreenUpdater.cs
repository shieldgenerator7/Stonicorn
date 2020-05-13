using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplashScreenUpdater : MonoBehaviour
{

    public List<GameObject> splashImages;//the images to be displayed in sequence

    //Settings
    public float showTime = 3;//how many seconds to show each image
    public float fadeInTime = 1;//how long it will take to fade the image in
    public float fadeOutTime = 1;//how long it will take to fade the image out

    //Runtime Vars
    private GameObject currentSplashImage;
    private int currentIndex = 0;
    private float lastKeyFrame = 0;
    private float displayState = -1;//0 = fade in, 1 = showing, 2 = fade out

    // Start is called before the first frame update
    void Start()
    {
        currentSplashImage = splashImages[currentIndex];
    }

    // Update is called once per frame
    void Update()
    {
        if (displayState == -1)
        {
            displayState = 0;
            lastKeyFrame = Time.unscaledTime;
            fadeObjectIn(currentSplashImage);
        }
        else if (displayState == 0)
        {
            if (Time.unscaledTime > lastKeyFrame + fadeInTime)
            {
                displayState = 1;
                lastKeyFrame = Time.unscaledTime;
            }
        }
        else if (displayState == 1)
        {
            if (Time.unscaledTime > lastKeyFrame + showTime)
            {
                //Start the hiding process for the current splash image
                if (currentIndex < splashImages.Count)
                {
                    displayState = 2;
                    lastKeyFrame = Time.unscaledTime;
                    fadeObjectOut(currentSplashImage);
                }
            }
        }
        else if (displayState == 2)
        {
            if (Time.unscaledTime > lastKeyFrame + fadeOutTime)
            {
                advanceToNextScreen();
            }
        }
        if (Input.GetKeyUp(KeyCode.Escape) || Input.GetMouseButtonUp(0)
            || (Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Ended))
        {
            Destroy(currentSplashImage);
            advanceToNextScreen();
        }
    }

    void fadeObjectIn(GameObject obj)
    {
        obj.SetActive(true);
        Fader f = currentSplashImage.AddComponent<Fader>();
        f.ignorePause = true;
        f.delayTime = 0;
        f.startfade = 0;
        f.endfade = 1;
        f.destroyObjectOnFinish = false;
        f.destroyScriptOnFinish = true;
        f.isEffectOnly = true;
        f.duration = fadeInTime;
    }
    void fadeObjectOut(GameObject obj)
    {
        Fader f = currentSplashImage.AddComponent<Fader>();
        f.ignorePause = true;
        f.delayTime = 0;
        f.startfade = 1;
        f.endfade = 0;
        f.destroyObjectOnFinish = true;
        f.destroyScriptOnFinish = true;
        f.isEffectOnly = true;
        f.duration = fadeOutTime;
    }
    void advanceToNextScreen()
    {
        currentIndex++;
        if (currentIndex < splashImages.Count)
        {
            displayState = -1;
            lastKeyFrame = Time.unscaledTime;
            currentSplashImage = splashImages[currentIndex];
        }
        else
        {
            displayState = 3;
            Fader f = gameObject.AddComponent<Fader>();
            f.ignorePause = true;
            f.duration = fadeOutTime;
            f.isEffectOnly = true;
            f.onFadeFinished += onLastFadeFinished;
        }
    }

    public delegate void OnSplashScreenFinished();
    public OnSplashScreenFinished onSplashScreenFinished;

    void onLastFadeFinished()
    {
        if (onSplashScreenFinished != null)
        {
            onSplashScreenFinished();
        }
    }
}
