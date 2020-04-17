using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseGestureInput : GestureInput
{
    public int mouseButton = 0;
    public float dragThreshold = 50;
    public float holdThreshold = 0.2f;

    private Vector2 origPosScreen;
    private Vector2 origPosWorld;
    private float origTime;

    public override bool InputSupported
    {
        get => Input.mousePresent;
    }

    public override InputDeviceMethod InputType
    {
        get => InputDeviceMethod.MOUSE;
    }

    public override bool InputOngoing
    {
        get => Input.GetMouseButton(mouseButton) || Input.GetMouseButtonUp(mouseButton);
    }

    public override bool processInput(GestureProfile profile)
    {
        //Debug.Log("mouse down: " + Input.GetMouseButtonDown(mouseButton) + " " + Time.time);
        //Debug.Log("mouse press: " + Input.GetMouseButton(mouseButton) + " " + Time.time);
        //Debug.Log("mouse up: " + Input.GetMouseButtonUp(mouseButton) + " " + Time.time);
        if (InputOngoing)
        {
            //
            //Check for click start
            //
            if (Input.GetMouseButtonDown(mouseButton))
            {
                origPosScreen = Input.mousePosition;
                origPosWorld = Utility.ScreenToWorldPoint(Input.mousePosition);
                origTime = Time.time;
            }

            //
            //Main Processing
            //

            //Tap
            if (Input.GetMouseButtonUp(mouseButton))
            {
                //If it's not a hold,
                if (Time.time - origTime < holdThreshold
                    //And it's not a drag,
                    && Vector2.Distance(origPosScreen, Input.mousePosition) < dragThreshold)
                {
                    //Then it's a tap.
                    profile.processTapGesture(Utility.ScreenToWorldPoint(Input.mousePosition));
                }
            }

            //
            //Check for removing touches
            //
            return true;
        }
        return false;
    }
}
