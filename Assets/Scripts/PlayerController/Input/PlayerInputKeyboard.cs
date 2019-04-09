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
        curMP = Camera.main.WorldToScreenPoint((Vector2)Managers.Player.transform.position + (inputDirection * Managers.Player.Range));
        if (inputDirection != Vector2.zero)
        {
            if (prevInputDirection == Vector2.zero)
            {
                inputState = InputState.Begin;
                origMP = curMP;
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

        //
        //Zoom Processing
        //Button Press Zoom
        //

        return inputData;
    }

}
