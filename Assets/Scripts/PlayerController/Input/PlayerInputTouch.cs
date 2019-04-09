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
        inputData.inputState = InputData.InputState.None;
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
                    inputData.inputState = InputData.InputState.End;
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
                    inputData.inputState = InputData.InputState.Hold;
                    inputData.NewScreenPos = Input.GetTouch(0).position;
                }
            }
            if (Input.touchCount == 2)
            {
                if (Input.GetTouch(1).phase == TouchPhase.Began)
                {
                    touchCount = 2;
                    inputData.inputState = InputData.InputState.Begin;
                    origMP2 = Input.GetTouch(1).position;
                    curMP2 = origMP2;
                    //Update origMP
                    inputData.NewScreenPos = Input.GetTouch(0).position;
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
                    inputData.inputState = InputData.InputState.Hold;
                    curMP2 = Input.GetTouch(1).position;
                }
            }
        }
        else if (Input.touchCount == 0)
        {
            touchCount = 0;
            inputData.inputState = InputData.InputState.None;
        }

        //
        //Zoom Processing
        //Pinch Touch Zoom
        //2015-12-31 (1:23am): copied from https://unity3d.com/learn/tutorials/modules/beginner/platform-specific/pinch-zoom
        //

        // If there are two touches on the device...
        if (Input.touchCount == 2)
        {
            // Store both touches.
            Touch touchZero = Input.GetTouch(0);
            Touch touchOne = Input.GetTouch(1);

            // Find the position in the previous frame of each touch.
            Vector2 touchZeroPrevPos = inputData.OldScreenPos;
            Vector2 touchOnePrevPos = origMP2;

            // Find the magnitude of the vector (the distance) between the touches in each frame.
            float prevTouchDeltaMag = Managers.Camera.distanceInWorldCoordinates(touchZeroPrevPos, touchOnePrevPos);
            float touchDeltaMag = Managers.Camera.distanceInWorldCoordinates(touchZero.position, touchOne.position);

            inputData.zoomMultiplier = prevTouchDeltaMag / touchDeltaMag;
        }

        //
        //Input Processing
        //
        inputData.process();

        return inputData;
    }

    /// <summary>
    /// Used in Update() to convey that the Input
    /// indicates the beginning of a new single-tap gesture,
    /// used often to transition between gestures with continuous input
    /// 2019-04-08: moved here from GestureManager
    /// </summary>
    /// <param name="tapIndex">The index of the tap in Input.GetTouch()</param>
    private void beginSingleTapGesture(int tapIndex = 0)
    {
        touchCount = 1;
        inputData.inputState = InputData.InputState.Begin;
        inputData.NewScreenPos = Input.GetTouch(tapIndex).position;
    }
}
