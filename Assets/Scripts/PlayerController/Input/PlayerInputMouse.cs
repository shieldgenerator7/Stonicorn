using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInputMouse : PlayerInput
{
    public override InputData getInput()
    {
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
        else if (Input.GetMouseButton(0))
        {
            touchCount = 1;
            if (Input.GetMouseButtonDown(0))
            {
                inputState = InputState.Begin;
                origMP = Input.mousePosition;
            }
            else
            {
                inputState = InputState.Hold;
                curMP = Input.mousePosition;
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            inputState = InputState.End;
        }
        else if (Input.GetAxis("Mouse ScrollWheel") != 0)
        {
            isPinchGesture = true;
            inputState = InputState.Hold;
        }
        else if (Input.touchCount == 0 && !Input.GetMouseButton(0))
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
                Managers.Camera.originalCameraPosition = Managers.Camera.transform.position - Managers.Player.transform.position;
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
                //Mouse Scrolling Zoom
                //
                if (Input.GetAxis("Mouse ScrollWheel") < 0)
                {
                    Managers.Camera.ZoomLevel++;
                }
                else if (Input.GetAxis("Mouse ScrollWheel") > 0)
                {
                    Managers.Camera.ZoomLevel--;
                }
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
}
