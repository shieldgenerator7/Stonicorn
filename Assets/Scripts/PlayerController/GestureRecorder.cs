using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GestureRecorder : SavableMonoBehaviour
{
    public List<Gesture> gestures = new List<Gesture>();
    public int currentPlayBackIndex = 0;
    public bool playBack = false;

    private Gesture CurrentGesture => gestures[currentPlayBackIndex];

    public override SavableObject CurrentState
    {
        get
        {
            SavableObject so = new SavableObject(this);
            so.addList("gestures", gestures);
            return so;
        }
        set
        {
            SavableObject so = value;
            gestures = so.List<Gesture>("gestures");
        }
    }

    public override void init()
    {
    }

    public void recordGesture(Gesture gesture)
    {
        currentPlayBackIndex = 0;
        playBack = false;
        gestures.Add(gesture);
    }

    public void playBackGesture(float time)
    {
        playBack = true;
        if (time >= CurrentGesture.time)
        {
            //TODO: process gesture
            currentPlayBackIndex++;
            if (currentPlayBackIndex >= gestures.Count)
            {
                playBack = false;
            }
        }
    }

    private void Update()
    {
        if (playBack)
        {
            playBackGesture(Managers.Time.Time);
        }
    }

}
