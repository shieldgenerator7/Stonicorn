using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInputTouch : PlayerInput
{
    public int touchCount = 0;//how many touches to process, usually only 0 or 1, only 2 if zoom
    public Vector3 origMP2;//second orginal "mouse position" for second touch
    public Vector3 curMP2;//"current mouse position" for second touch

    /// <summary>
    /// Gets the input for a touch screen
    /// 2019-04-08: copied from PlayerInputMouse
    /// </summary>
    /// <returns></returns>
    public override InputData getInput()
    {
        inputState = InputState.None;
        //
        //Input scouting
        //
        if (Input.touchCount > 2)
        {
            touchCount = 0;
        }
        else if (Input.touchCount >= 1)
        {
            {
                if (Input.GetTouch(0).phase == TouchPhase.Began)
                {
                    beginSingleTapGesture();
                }
                else if (Input.GetTouch(0).phase == TouchPhase.Ended)
                {
                    inputState = InputState.End;
                    if (touchCount == 2)
                    {
                        if (Input.GetTouch(1).phase != TouchPhase.Ended)
                        {
                            beginSingleTapGesture(1);
                        }
                    }
                }
                else
                {
                    inputState = InputState.Hold;
                    curMP = Input.GetTouch(0).position;
                }
            }
            if (Input.touchCount == 2)
            {
                if (Input.GetTouch(1).phase == TouchPhase.Began)
                {
                    isPinchGesture = true;
                    isCameraMovementOnly = true;
                    touchCount = 2;
                    inputState = InputState.Begin;
                    origMP2 = Input.GetTouch(1).position;
                    origOrthoSize = Managers.Camera.ZoomLevel;
                    //Update origMP
                    origMP = Input.GetTouch(0).position;
                }
                else if (Input.GetTouch(1).phase == TouchPhase.Ended)
                {
                    if (Input.GetTouch(0).phase != TouchPhase.Ended)
                    {
                        beginSingleTapGesture();
                    }
                }
                else
                {
                    inputState = InputState.Hold;
                    curMP2 = Input.GetTouch(1).position;
                }
            }
        }
        else if (Input.touchCount == 0)
        {
            touchCount = 0;
            inputState = InputState.None;
            //
            isDrag = false;
            isPinchGesture = false;
            isCameraMovementOnly = false;
        }


        //
        //Preliminary Processing
        //Stats are processed here
        //
        switch (inputState)
        {
            case InputState.Begin:
                curMP = origMP;
                maxMouseMovement = 0;
                origTime = Time.time;
                curTime = origTime;
                curMP2 = origMP2;
                origMPWorld = Camera.main.ScreenToWorldPoint(origMP);
                break;
            case InputState.End: //do the same thing you would for "in progress"
            case InputState.Hold:
                float mm = Vector3.Distance(curMP, origMP);
                if (mm > maxMouseMovement)
                {
                    maxMouseMovement = mm;
                }
                curTime = Time.time;
                holdTime = curTime - origTime;
                break;
            case InputState.None: break;
            default:
                throw new System.Exception("Click State of wrong type, or type not processed! (Stat Processing) clickState: " + inputState);
        }
        curMPWorld = Camera.main.ScreenToWorldPoint(curMP);//cast to Vector2 to force z to 0


        //
        //Input Processing
        //
        InputData inputData = new InputData(origMP, curMP, inputState, holdTime, 1);

        if (isPinchGesture)
        {//touchCount == 0 || touchCount >= 2
            if (inputState == InputState.Begin)
            {
            }
            else if (inputState == InputState.Hold)
            {
                //
                //Zoom Processing
                //
                
                //
                //Pinch Touch Zoom
                //2015-12-31 (1:23am): copied from https://unity3d.com/learn/tutorials/modules/beginner/platform-specific/pinch-zoom
                //

                // If there are two touches on the device...
                if (touchCount == 2)
                {
                    // Store both touches.
                    Touch touchZero = Input.GetTouch(0);
                    Touch touchOne = Input.GetTouch(1);

                    // Find the position in the previous frame of each touch.
                    Vector2 touchZeroPrevPos = origMP;
                    Vector2 touchOnePrevPos = origMP2;

                    // Find the magnitude of the vector (the distance) between the touches in each frame.
                    float prevTouchDeltaMag = Managers.Camera.distanceInWorldCoordinates(touchZeroPrevPos, touchOnePrevPos);
                    float touchDeltaMag = Managers.Camera.distanceInWorldCoordinates(touchZero.position, touchOne.position);

                    float newZoomLevel = origOrthoSize * prevTouchDeltaMag / touchDeltaMag;

                    Managers.Camera.ZoomLevel = newZoomLevel;
                }
            }
            else if (inputState == InputState.End)
            {
                //Update Stats
                GameStatistics.addOne("Pinch");
                //Process Pinch Gesture
                origOrthoSize = Managers.Camera.ZoomLevel;
            }
        }

        return inputData;
    }

    /// <summary>
    /// Used in Update() to convey that the Input
    /// indicates the beginning of a new single-tap gesture,
    /// used often to transition between gestures with continuous input
    /// 2019-04-08: moved here from GestureManager
    /// </summary>
    /// <param name="tapIndex">The index of the tap in Input.GetTouch()</param>
    protected void beginSingleTapGesture(int tapIndex = 0)
    {
        touchCount = 1;
        inputState = InputState.Begin;
        origMP = Input.GetTouch(tapIndex).position;
        if (isPinchGesture)
        {
            isDrag = true;
        }
        else
        {
            isCameraMovementOnly = false;
        }
    }
}
