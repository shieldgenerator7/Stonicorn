using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchGestureInput : GestureInput
{
    public float dragThreshold = 50;
    public float holdThreshold = 0.2f;

    private int touchCount = 0;//how many touches to process, usually only 0 or 1, only 2 if zoom
    private int maxTouchCount = 0;//the max amount of touches involved in this gesture at any one time

    private Dictionary<int, TouchData> touchDatas = new Dictionary<int, TouchData>();

    struct TouchData
    {
        public Vector2 origPosScreen;
        public Vector2 origPosWorld;
        public float origTime;

        public TouchData(Touch touch)
        {
            origPosScreen = touch.position;
            origPosWorld = Utility.ScreenToWorldPoint(touch.position);
            origTime = Time.time;
        }
    }

    public override bool InputSupported
    {
        get => Input.touchSupported || Input.stylusTouchSupported;
    }

    public override InputDeviceMethod InputType
    {
        get => InputDeviceMethod.TOUCH;
    }

    public override bool InputOngoing
    {
        get => Input.touchCount > 0;
    }

    public override bool processInput(GestureProfile profile)
    {
        if (Input.touchCount > 0)
        {
            //
            //Check for adding new touches
            //
            for (int i = 0; i < Input.touchCount; i++)
            {                
                //Managers.Camera.ZoomLevel = 5;
                Touch touch = Input.touches[i];
                if (touch.phase == TouchPhase.Began)
                {
                    touchDatas.Add(touch.fingerId, new TouchData(touch));
                }
            }
            maxTouchCount = Mathf.Max(maxTouchCount, Input.touchCount);

            //
            //Main Processing
            //

            //Tap
            if (maxTouchCount == 1 && Input.touches[0].phase == TouchPhase.Ended)
            {
                Touch touch = Input.touches[0];
                TouchData data = touchDatas[touch.fingerId];
                //If it's not a hold,
                if (Time.time - data.origTime < holdThreshold
                    //And it's not a drag,
                    && Vector2.Distance(data.origPosScreen, touch.position) < dragThreshold)
                {
                    //Then it's a tap.
                    profile.processTapGesture(Utility.ScreenToWorldPoint(touch.position));
                }
            }

            //
            //Check for removing touches
            //
            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch touch = Input.touches[i];
                if (touch.phase == TouchPhase.Ended)
                {
                    touchDatas.Remove(touch.fingerId);
                }
            }
            if (Input.touchCount == 0)
            {
                maxTouchCount = 0;
            }
            return true;
        }
        else
        {
            maxTouchCount = 0;
            return false;
        }
    }
}
