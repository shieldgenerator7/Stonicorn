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
            lastKeyFrame = Time.time;
            fadeObjectIn(currentSplashImage);
        }
        else if (displayState == 0)
        {
            if (Time.time > lastKeyFrame + fadeInTime)
            {
                displayState = 1;
                lastKeyFrame = Time.time;
            }
        }
        else if (displayState == 1)
        {
            if (Time.time > lastKeyFrame + showTime)
            {
                //Start the hiding process for the current splash image
                if (currentIndex < splashImages.Count)
                {
                    displayState = 2;
                    lastKeyFrame = Time.time;
                    fadeObjectOut(currentSplashImage);
                }
            }
        }
        else if (displayState == 2)
        {
            if (Time.time > lastKeyFrame + fadeOutTime)
            {
                currentIndex++;
                if (currentIndex < splashImages.Count)
                {
                    displayState = -1;
                    lastKeyFrame = Time.time;
                    currentSplashImage = splashImages[currentIndex];
                }
                else
                {
                    displayState = 3;
                    Fader f = gameObject.AddComponent<Fader>();
                    f.duration = fadeOutTime;
                    f.onFadeFinished += onLastFadeFinished;
                }
            }
        }
    }

    void fadeObjectIn(GameObject obj)
    {
        obj.SetActive(true);
        Fader f = currentSplashImage.AddComponent<Fader>();
        f.delayTime = 0;
        f.startfade = 0;
        f.endfade = 1;
        f.destroyObjectOnFinish = false;
        f.destroyScriptOnFinish = true;
        f.duration = fadeInTime;
    }
    void fadeObjectOut(GameObject obj)
    {
        Fader f = currentSplashImage.AddComponent<Fader>();
        f.delayTime = 0;
        f.startfade = 1;
        f.endfade = 0;
        f.destroyObjectOnFinish = true;
        f.destroyScriptOnFinish = true;
        f.duration = fadeOutTime;
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
