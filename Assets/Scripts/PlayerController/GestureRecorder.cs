using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GestureRecorder : SavableMonoBehaviour
{
    public List<Gesture> gestures = new List<Gesture>();
    public int currentPlayBackIndex = 0;
    public bool playBack = false;
    private Stonicorn stonicorn;

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
        stonicorn = GetComponent<Stonicorn>();
    }

    public void recordGesture(Gesture gesture)
    {
        currentPlayBackIndex = 0;
        playBack = false;
        //Convert to relative to stonicorn
        gesture.startPosition = gesture.startPosition - (Vector2)stonicorn.transform.position;
        gesture.position = gesture.position - (Vector2)stonicorn.transform.position;
        //Add gesture to list
        gestures.Add(gesture);
    }

    public void playBackGesture(float time)
    {
        playBack = true;
        Gesture gesture = CurrentGesture;
        if (time >= gesture.time)
        {
            //Convert to relative to world
            gesture.startPosition = (Vector2)stonicorn.transform.position + gesture.startPosition;
            gesture.position = (Vector2)stonicorn.transform.position + gesture.position;
            //Process gesture
            stonicorn.processGesture(gesture);
            //Move to next gesture
            currentPlayBackIndex++;
            if (currentPlayBackIndex >= gestures.Count)
            {
                playBack = false;
            }
        }
    }

    public void clear()
    {
        currentPlayBackIndex = 0;
        gestures.Clear();
    }

    private void Update()
    {
        if (playBack)
        {
            playBackGesture(Managers.Time.Time);
        }
    }

}
