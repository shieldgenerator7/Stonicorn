using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInputKeyboard : PlayerInput
{//2019-04-08: copied from PlayerInputMouse

    private Vector2 inputDirection = Vector2.zero;
    private Vector2 prevInputDirection;

    public override InputData getInput()
    {
        inputData.inputState = InputData.InputState.None;
        //
        //Input scouting
        //
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        //Get the input direction
        prevInputDirection = inputDirection;
        inputDirection = new Vector2(horizontal, vertical);
        //Convert it to the player's gravity space
        inputDirection = Managers.Camera.transform.TransformDirection(inputDirection);
        //Normalize it
        inputDirection.Normalize();
        //Process it
        if (inputDirection != Vector2.zero)
        {
            if (prevInputDirection == Vector2.zero)
            {
                inputData.inputState = InputData.InputState.Begin;
                inputData.gestureType = GestureManager.GestureType.TAP;
            }
            else
            {
                inputData.inputState = InputData.InputState.Hold;
            }
            inputData.NewWorldPos = (Vector2)Managers.Player.transform.position + (inputDirection * Managers.Player.Range);
        }
        else
        {
            if (prevInputDirection != Vector2.zero)
            {
                inputData.inputState = InputData.InputState.End;
                inputData.NewWorldPos = (Vector2)Managers.Player.transform.position + (prevInputDirection * Managers.Player.Range);
            }
        }

        //
        // Rotation
        //
        if (inputData.inputState == InputData.InputState.None)
        {
            bool rotate = Input.GetKeyDown(KeyCode.R);
            if (rotate)
            {
                inputData.inputState = InputData.InputState.Begin;
                inputData.NewWorldPos = Managers.Player.transform.position;
                inputData.gestureType = GestureManager.GestureType.TAP;
                inputData.inputState = InputData.InputState.End;
            }
        }

        //
        //Zoom Processing
        //Button Press Zoom
        //

        //
        //Input Processing
        //
        inputData.process();

        return inputData;
    }

}
