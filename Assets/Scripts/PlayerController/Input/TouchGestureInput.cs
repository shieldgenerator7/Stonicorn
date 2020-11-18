using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TouchGestureInput : GestureInput
{
    public float dragThreshold = 50;
    public float holdThreshold = 0.2f;

    private int touchCount = 0;//how many touches to process, usually only 0 or 1, only 2 if zoom
    private int maxTouchCount = 0;//the max amount of touches involved in this gesture at any one time

    //Camera Drag processing variables
    private Vector2 origTouchCenter;
    private Vector2 origTouchCenterWorld
        => Utility.ScreenToWorldPoint(origTouchCenter);
    private List<Vector2> AliveTouchPositions
        => Input.touches
            .Where(t =>
                t.phase != TouchPhase.Ended
                && t.phase != TouchPhase.Canceled
                ).ToList()
            .ConvertAll(t => t.position);
    private Vector2 TouchCenter
        => Utility.average(AliveTouchPositions);

    //Camera Zoom processing variables
    private float origCameraZoom;//camera zoom at the start of pinch gesture
    private float origAvgDistanceFromCenter;//how far the taps are from the tap center at start of pinch gesture
    private float AverageDistanceFromCenter
    {
        get
        {
            Vector2 center = TouchCenter;
            List<Vector2> atps = AliveTouchPositions;
            return (atps.Count > 0)
                ? atps.Average(v => Vector2.Distance(v, center))
                : 0;
        }
    }

    //Touch Data
    private Dictionary<int, TouchData> touchDatas = new Dictionary<int, TouchData>();

    struct TouchData
    {
        public Vector2 origPosScreen;
        public Vector2 origPosWorld
            => Utility.ScreenToWorldPoint(origPosScreen);
        public float origTime;

        public TouchData(Touch touch)
        {
            origPosScreen = touch.position;
            origTime = Time.time;
        }
    }

    private enum TouchEvent
    {
        UNKNOWN,
        TAP,
        DRAG,
        HOLD,
        CAMERA
    }
    private TouchEvent touchEvent = TouchEvent.UNKNOWN;

    public override bool InputSupported
        => Input.touchSupported || Input.stylusTouchSupported;

    public override InputDeviceMethod InputType
        => InputDeviceMethod.TOUCH;

    public override bool InputOngoing
        => Input.touchCount > 0;

    public override bool processInput(GestureProfile profile)
    {
        if (Input.touchCount > 0)
        {
            //
            //Check for adding new touches
            //
            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch touch = Input.touches[i];
                if (touch.phase == TouchPhase.Began)
                {
                    touchDatas.Add(touch.fingerId, new TouchData(touch));
                    origTouchCenter = TouchCenter;
                    origCameraZoom = Managers.Camera.ZoomLevel;
                    origAvgDistanceFromCenter = AverageDistanceFromCenter;
                }
            }
            maxTouchCount = Mathf.Max(maxTouchCount, Input.touchCount);

            //
            // Gesture Identification
            //

            if (touchEvent == TouchEvent.UNKNOWN)
            {
                if (maxTouchCount == 1)
                {
                    Touch touch = Input.touches[0];
                    TouchData data = touchDatas[touch.fingerId];
                    //Drag Gesture
                    if (Vector2.Distance(data.origPosScreen, touch.position) >= dragThreshold)
                    {
                        touchEvent = TouchEvent.DRAG;
                    }
                    //Hold Gesture
                    if (Time.time - data.origTime >= holdThreshold)
                    {
                        touchEvent = TouchEvent.HOLD;
                    }
                }
                //If more than one touch
                else if (maxTouchCount > 1)
                {
                    touchEvent = TouchEvent.CAMERA;
                }
            }
            //If converting from a player gesture to a camera gesture,
            else if (touchEvent != TouchEvent.CAMERA && maxTouchCount > 1)
            {
                //End the current player gesture
                //(No need to process tap gesture,
                //because it requires that all input stops to activate)
                Touch touch = Input.touches[0];
                TouchData data = touchDatas[touch.fingerId];
                switch (touchEvent)
                {
                    //DRAG
                    case TouchEvent.DRAG:
                        profile.processDragGesture(
                            data.origPosWorld,
                            Utility.ScreenToWorldPoint(touch.position),
                            DragType.DRAG_PLAYER,
                            true
                            );
                        break;
                    //HOLD
                    case TouchEvent.HOLD:
                        profile.processHoldGesture(
                            Utility.ScreenToWorldPoint(touch.position),
                            Time.time - data.origTime,
                            true
                            );
                        break;
                }
                //Convert to camera gesture
                touchEvent = TouchEvent.CAMERA;
            }

            //
            //Main Processing
            //

            if (maxTouchCount == 1)
            {
                Touch touch = Input.touches[0];
                TouchData data = touchDatas[touch.fingerId];
                switch (touchEvent)
                {
                    //DRAG
                    case TouchEvent.DRAG:
                        profile.processDragGesture(
                            data.origPosWorld,
                            Utility.ScreenToWorldPoint(touch.position),
                            DragType.DRAG_PLAYER,
                            touch.phase == TouchPhase.Ended
                            );
                        break;
                    //HOLD
                    case TouchEvent.HOLD:
                        profile.processHoldGesture(
                            Utility.ScreenToWorldPoint(touch.position),
                            Time.time - data.origTime,
                            touch.phase == TouchPhase.Ended
                            );
                        break;
                }

                //
                // Check for tap end
                //
                if (touch.phase == TouchPhase.Ended)
                {
                    //If it's unknown,
                    if (touchEvent == TouchEvent.UNKNOWN)
                    {
                        //Then it's a tap
                        profile.processTapGesture(Utility.ScreenToWorldPoint(touch.position));
                    }
                }
            }
            else if (maxTouchCount > 1)
            {
                //Get the center and drag the camera to it
                profile.processDragGesture(
                    origTouchCenterWorld,
                    Utility.ScreenToWorldPoint(TouchCenter),
                    DragType.DRAG_CAMERA,
                    Input.touches
                        .Where(t =>
                            t.phase != TouchPhase.Ended
                            && t.phase != TouchPhase.Canceled
                        ).ToArray()
                        .Length == 0
                    );
                //Get the change in scale and zoom the camera
                float adfc = AverageDistanceFromCenter;
                if (adfc > 0)
                {
                    float scaleFactor = origAvgDistanceFromCenter / adfc;
                    Managers.Camera.ZoomLevel = origCameraZoom * scaleFactor;
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
                    origTouchCenter = TouchCenter;
                    origCameraZoom = Managers.Camera.ZoomLevel;
                    origAvgDistanceFromCenter = AverageDistanceFromCenter;
                }
            }
            return true;
        }
        //If there is no input,
        else
        {
            //Reset gesture variables
            touchEvent = TouchEvent.UNKNOWN;
            maxTouchCount = 0;
            origTouchCenter = Vector2.zero;
            origCameraZoom = Managers.Camera.ZoomLevel;
            origAvgDistanceFromCenter = 0;
            touchDatas.Clear();
            return false;
        }
    }
}
