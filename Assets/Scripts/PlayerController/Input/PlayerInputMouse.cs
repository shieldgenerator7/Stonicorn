using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInputMouse : PlayerInput
{
    public override InputData getInput()
    {
        inputState = InputState.None;
        //
        //Input scouting
        //
        if (Input.GetMouseButton(0))
        {
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
        else if (!Input.GetMouseButton(0))
        {
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

        if (Input.GetAxis("Mouse ScrollWheel") != 0)
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
        }

        return inputData;
    }
}
