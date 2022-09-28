using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EndingDialogueTrigger : DialogueTrigger
{
    public Image endImage;
    public float onlyImageDuration = 5;
    public float afterImageDuration = 5;

    private float imageStartTime=-1;
    private float afterStartTime = -1;

    protected override void triggerEvent()
    {
        base.triggerEvent();
        FindObjectOfType<DialoguePlayer>().onDialogueEnded += showEndImage;
    }

    public void showEndImage(DialoguePath path)
    {
        endImage.enabled = true;
        imageStartTime = Time.time;
    }

    private void Update()
    {
        if (imageStartTime > 0 && Time.time > imageStartTime + onlyImageDuration)
        {
            //show credits
            SceneManager.LoadScene("Credits", LoadSceneMode.Additive);
            SceneManager.sceneLoaded += hookIntoCredits;
            imageStartTime = -1;
        }
        if (afterStartTime > 0 && Time.time > afterStartTime + afterImageDuration)
        {
            //show credits
            SceneManager.LoadScene("Title");
            afterStartTime = -1;
        }
    }

    private void hookIntoCredits(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Credits")
        {
            SceneManager.sceneLoaded -= hookIntoCredits;
            FindObjectOfType<Credits>().onFinish += () =>
            {
                SceneManager.UnloadSceneAsync("Credits");
                afterStartTime = Time.time;
            };
        }
    }
}
