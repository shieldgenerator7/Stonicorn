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
        prevInputDirection = inputDirection;
        inputDirection = new Vector2(horizontal, vertical).normalized;
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
