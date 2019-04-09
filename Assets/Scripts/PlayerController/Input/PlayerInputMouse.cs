using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInputMouse : PlayerInput
{
    float scrollWheelAxis;
    float prevScrollWheelAxis;

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
        }
        prevScrollWheelAxis = scrollWheelAxis;
        scrollWheelAxis = Input.GetAxis("Mouse ScrollWheel");
        if (scrollWheelAxis != 0)
        {
            if (prevScrollWheelAxis == 0)
            {
                inputState = InputState.Begin;
            }
            else
            {
                inputState = InputState.Hold;
            }
        }
        else
        {
            if (prevScrollWheelAxis != 0)
            {
                inputState = InputState.End;
            }
        }


        //
        //Preliminary Processing
        //Stats are processed here
        //
        switch (inputState)
        {
            case InputState.Begin:
                curMP = origMP;
                origTime = Time.time;
                curTime = origTime;
                origMPWorld = Camera.main.ScreenToWorldPoint(origMP);
                break;
            case InputState.End: //do the same thing you would for "in progress"
            case InputState.Hold:
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
        InputData inputData = new InputData(
            Camera.main.ScreenToWorldPoint(origMP),
            Camera.main.ScreenToWorldPoint(curMP),
            inputState,
            holdTime,
            1
            );

        //
        //Zoom Processing
        //Mouse Scrolling Zoom
        //
        if (scrollWheelAxis != 0)
        {
            inputData.zoomMultiplier = Mathf.Pow(2, Input.mouseScrollDelta.y * 2 / 3);
        }

        return inputData;
    }
}
