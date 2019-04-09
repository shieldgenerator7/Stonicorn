using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInputMouse : PlayerInput
{
    float scrollWheelAxis;
    float prevScrollWheelAxis;

    public override InputData getInput()
    {
        inputData.inputState = InputData.InputState.None;

        //
        // Mouse Clicking
        //
        if (Input.GetMouseButton(0))
        {
            if (Input.GetMouseButtonDown(0))
            {
                inputData.inputState = InputData.InputState.Begin;
            }
            else
            {
                inputData.inputState = InputData.InputState.Hold;
            }
            inputData.NewScreenPos = Input.mousePosition;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            inputData.inputState = InputData.InputState.End;
        }

        //
        // Scroll Wheel
        //
        if (inputData.inputState == InputData.InputState.None)
        {
            prevScrollWheelAxis = scrollWheelAxis;
            scrollWheelAxis = Input.GetAxis("Mouse ScrollWheel");
            if (scrollWheelAxis != 0)
            {
                if (prevScrollWheelAxis == 0)
                {
                    inputData.inputState = InputData.InputState.Begin;
                }
                else
                {
                    inputData.inputState = InputData.InputState.Hold;
                }
            }
            else
            {
                if (prevScrollWheelAxis != 0)
                {
                    inputData.inputState = InputData.InputState.End;
                }
            }
            if (scrollWheelAxis != 0)
            {
                inputData.zoomMultiplier = Mathf.Pow(2, -Input.mouseScrollDelta.y * 2 / 3);
                if (scrollWheelAxis < 0)
                {
                    inputData.zoomMultiplier = Mathf.Max(inputData.zoomMultiplier, 4 / 3);
                }
                else if (scrollWheelAxis > 0)
                {
                    inputData.zoomMultiplier = Mathf.Min(inputData.zoomMultiplier, 3 / 4);
                }
            }
        }
        else
        {
            prevScrollWheelAxis = 0;
            scrollWheelAxis = 0;
            inputData.zoomMultiplier = 1;
        }

        //
        //Input Processing
        //
        inputData.process();

        return inputData;
    }
}
