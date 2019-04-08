using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInputMouse : PlayerInput
{
    public override PlayerInput getInput()
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
                    clickState = ClickState.Ended;
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
                    clickState = ClickState.InProgress;
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
                    clickState = ClickState.Began;
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
                    clickState = ClickState.InProgress;
                    curMP2 = Input.GetTouch(1).position;
                }
            }
        }
        else if (Input.GetMouseButton(0))
        {
            touchCount = 1;
            if (Input.GetMouseButtonDown(0))
            {
                clickState = ClickState.Began;
                origMP = Input.mousePosition;
            }
            else
            {
                clickState = ClickState.InProgress;
                curMP = Input.mousePosition;
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            clickState = ClickState.Ended;
        }
        else if (Input.GetAxis("Mouse ScrollWheel") != 0)
        {
            isPinchGesture = true;
            clickState = ClickState.InProgress;
        }
        else if (Input.touchCount == 0 && !Input.GetMouseButton(0))
        {
            touchCount = 0;
            clickState = ClickState.None;
            //
            isDrag = false;
            isPinchGesture = false;
            isCameraMovementOnly = false;
        }


        //
        //Preliminary Processing
        //Stats are processed here
        //
        switch (clickState)
        {
            case ClickState.Began:
                curMP = origMP;
                maxMouseMovement = 0;
                Managers.Camera.originalCameraPosition = Managers.Camera.transform.position - Managers.Player.transform.position;
                origTime = Time.time;
                curTime = origTime;
                curMP2 = origMP2;
                origMPWorld = Camera.main.ScreenToWorldPoint(origMP);
                break;
            case ClickState.Ended: //do the same thing you would for "in progress"
            case ClickState.InProgress:
                float mm = Vector3.Distance(curMP, origMP);
                if (mm > maxMouseMovement)
                {
                    maxMouseMovement = mm;
                }
                curTime = Time.time;
                holdTime = curTime - origTime;
                break;
            case ClickState.None: break;
            default:
                throw new System.Exception("Click State of wrong type, or type not processed! (Stat Processing) clickState: " + clickState);
        }
        curMPWorld = Camera.main.ScreenToWorldPoint(curMP);//cast to Vector2 to force z to 0


        //
        //Input Processing
        //
        if (touchCount == 1)
        {
            if (clickState == ClickState.Began)
            {
                //Set all flags = true
                cameraDragInProgress = false;
                isDrag = false;
                if (!isCameraMovementOnly)
                {
                    isTapGesture = true;
                }
                else
                {
                    isTapGesture = false;
                }
                isHoldGesture = false;
                isPinchGesture = touchCount == 2;
            }
            else if (clickState == ClickState.InProgress)
            {
                if (maxMouseMovement > Managers.Gesture.dragThreshold
                    && Managers.Player.Speed <= Managers.Gesture.playerSpeedThreshold)
                {
                    if (!isHoldGesture && !isPinchGesture)
                    {
                        isTapGesture = false;
                        isDrag = true;
                        cameraDragInProgress = true;
                    }
                }
                if (holdTime > Managers.Gesture.holdThreshold)
                {
                    if (!isDrag && !isPinchGesture && !isCameraMovementOnly)
                    {
                        isTapGesture = false;
                        isHoldGesture = true;
                        Time.timeScale = GestureManager.holdTimeScale;
                    }
                }
                if (isDrag)
                {
                    Managers.Gesture.currentGP.processDragGesture(Camera.main.ScreenToWorldPoint(origMP), curMPWorld);
                }
                else if (isHoldGesture)
                {
                    Managers.Gesture.currentGP.processHoldGesture(curMPWorld, holdTime, false);
                }
            }
            else if (clickState == ClickState.Ended)
            {
                if (isDrag)
                {
                    //Update Stats
                    GameStatistics.addOne("Drag");
                    //Process Drag Gesture
                    Managers.Camera.pinPoint();
                }
                else if (isHoldGesture)
                {
                    Managers.Gesture.currentGP.processHoldGesture(curMPWorld, holdTime, true);
                }
                else if (isTapGesture)
                {
                    //Update Stats
                    GameStatistics.addOne("Tap");
                    //Process Tap Gesture
                    bool checkPointPort = false;//Merky is in a checkpoint teleporting to another checkpoint
                    if (Managers.Player.InCheckPoint)
                    {
                        foreach (CheckPointChecker cpc in GameObject.FindObjectsOfType<CheckPointChecker>())
                        {
                            if (cpc.checkGhostActivation(curMPWorld))
                            {
                                checkPointPort = true;
                                Managers.Gesture.currentGP.processTapGesture(cpc);
                                if (Managers.Gesture.tapGesture != null)
                                {
                                    Managers.Gesture.tapGesture();
                                }
                                break;
                            }
                        }
                    }
                    if (!checkPointPort)
                    {
                        Managers.Gesture.currentGP.processTapGesture(curMPWorld);
                        if (Managers.Gesture.tapGesture != null)
                        {
                            Managers.Gesture.tapGesture();
                        }
                    }
                }

                //Set all flags = false
                cameraDragInProgress = false;
                isDrag = false;
                isTapGesture = false;
                isHoldGesture = false;
                isPinchGesture = false;
                isCameraMovementOnly = false;
                Time.timeScale = 1;
            }
            else
            {
                throw new System.Exception("Click State of wrong type, or type not processed! (Input Processing) clickState: " + clickState);
            }

        }
        if (isPinchGesture)
        {//touchCount == 0 || touchCount >= 2
            if (clickState == ClickState.Began)
            {
            }
            else if (clickState == ClickState.InProgress)
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
            else if (clickState == ClickState.Ended)
            {
                //Update Stats
                GameStatistics.addOne("Pinch");
                //Process Pinch Gesture
                origOrthoSize = Managers.Camera.ZoomLevel;
            }
        }


        return this;
    }
}
