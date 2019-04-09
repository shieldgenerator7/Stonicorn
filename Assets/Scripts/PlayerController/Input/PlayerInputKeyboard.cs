using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInputKeyboard : PlayerInput
{//2019-04-08: copied from PlayerInputMouse

    private Vector2 inputDirection = Vector2.zero;
    private Vector2 prevInputDirection;

    public override InputData getInput()
    {
        inputState = InputState.None;
        //
        //Input scouting
        //
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        prevInputDirection = inputDirection;
        inputDirection = new Vector2(horizontal, vertical).normalized;
        curMPWorld = (Vector2)Managers.Player.transform.position + (inputDirection * Managers.Player.Range);
        if (inputDirection != Vector2.zero)
        {
            if (prevInputDirection == Vector2.zero)
            {
                inputState = InputState.Begin;
                origMPWorld = curMPWorld;
            }
            else
            {
                inputState = InputState.Hold;
            }
        }
        else
        {
            if (prevInputDirection != Vector2.zero)
            {
                inputState = InputState.End;
                curMPWorld = (Vector2)Managers.Player.transform.position + (prevInputDirection * Managers.Player.Range);
            }
        }


        //
        //Preliminary Processing
        //Stats are processed here
        //
        switch (inputState)
        {
            case InputState.Begin:
                origTime = Time.time;
                curTime = origTime;
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


        //
        //Input Processing
        //
        inputData.setWorldPos(origMPWorld, curMPWorld);
        inputData.setState(inputState, holdTime, 1);

        //
        //Zoom Processing
        //Button Press Zoom
        //

        return inputData;
    }

}
