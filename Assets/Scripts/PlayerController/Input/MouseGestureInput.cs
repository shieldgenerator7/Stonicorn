using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseGestureInput : GestureInput
{
    public int mouseButton = 0;
    public float dragThreshold = 50;
    public float holdThreshold = 0.2f;

    private Vector2 origPosScreen;
    private Vector2 OrigPosWorld
    {
        get => Utility.ScreenToWorldPoint(origPosScreen);
    }
    private float origTime;

    private enum MouseEvent
    {
        UNKNOWN,
        CLICK,
        DRAG,
        HOLD,
        SCROLL
    }
    private MouseEvent mouseEvent = MouseEvent.UNKNOWN;

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
        get => Input.GetMouseButton(mouseButton)
            || Input.GetMouseButtonUp(mouseButton)
            || Input.GetAxis("Mouse ScrollWheel") != 0;
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
            if (mouseEvent == MouseEvent.UNKNOWN)
            {
                //Click beginning
                if (Input.GetMouseButtonDown(mouseButton))
                {
                    origPosScreen = Input.mousePosition;
                    origTime = Time.time;
                }
                else if (Input.GetAxis("Mouse ScrollWheel") != 0)
                {
                    mouseEvent = MouseEvent.SCROLL;
                }
                //Click middle
                else
                {
                    //Check Drag
                    if (Vector2.Distance(origPosScreen, Input.mousePosition) >= dragThreshold)
                    {
                        mouseEvent = MouseEvent.DRAG;
                    }
                    //Check Hold
                    else if (Time.time - origTime >= holdThreshold)
                    {
                        mouseEvent = MouseEvent.HOLD;
                    }
                }
            }

            //
            //Main Processing
            //

            switch (mouseEvent)
            {
                case MouseEvent.DRAG:
                    profile.processDragGesture(
                        OrigPosWorld, 
                        Utility.ScreenToWorldPoint(Input.mousePosition)
                        );
                    break;
                case MouseEvent.HOLD:
                    profile.processHoldGesture(
                        Utility.ScreenToWorldPoint(Input.mousePosition),
                        Time.time - origTime,
                        Input.GetMouseButtonUp(mouseButton)
                        );
                    break;
                case MouseEvent.SCROLL:
                    if (Input.GetAxis("Mouse ScrollWheel") < 0)
                    {
                        Managers.Camera.ZoomLevel *= 1.2f;
                    }
                    else if (Input.GetAxis("Mouse ScrollWheel") > 0)
                    {
                        Managers.Camera.ZoomLevel /= 1.2f;
                    }
                    break;
            }

            //
            //Check for click end
            //
            if (Input.GetMouseButtonUp(mouseButton))
            {
                //If it's unknown,
                if (mouseEvent == MouseEvent.UNKNOWN)
                {
                    //Then it's a click.
                    mouseEvent = MouseEvent.CLICK;
                    profile.processTapGesture(Utility.ScreenToWorldPoint(Input.mousePosition));
                }
            }
            return true;
        }
        else
        {
            mouseEvent = MouseEvent.UNKNOWN;
            return false;
        }
    }
}
