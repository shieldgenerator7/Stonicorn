using Microsoft.Win32.SafeHandles;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyboardGestureInput : GestureInput
{
    public float holdThreshold = 0.2f;

    private Vector2 prevDir = Vector2.zero;
    private float gestureStartTime;

    private Vector2 origPosScreen;
    private Vector2 OrigPosWorld
        => Utility.ScreenToWorldPoint(origPosScreen);

    private float origTime;

    public override InputDeviceMethod InputType
        => InputDeviceMethod.KEYBOARD;

    public override bool InputOngoing
        => Input.anyKey;

    public override bool processInput(GestureProfile profile)
    {
        if (InputOngoing)
        {
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");
            if (Input.GetButton("Jump"))
            {
                vertical = 1;
            }
            Vector2 dir = (horizontal != 0 || vertical != 0)
                ? (
                (Vector2)((Camera.main.transform.up * vertical)
                + (Camera.main.transform.right * horizontal)
                ).normalized)
                : Vector2.zero;
            bool isInputNow = dir != Vector2.zero;
            bool wasInputPrev = prevDir != Vector2.zero;
            if (isInputNow || wasInputPrev)
            {
                if (!wasInputPrev && isInputNow)
                {
                    //Gesture Start
                    gestureStartTime = Time.time;
                }
                float range = Managers.Player.BaseRange;
                if (Input.GetButton("Short"))
                {
                    range /= 2;
                }
                profile.processGesture(new Gesture(
                    Managers.Player.position + (dir * range),
                    Time.time - gestureStartTime,
                    (!isInputNow)
                    ? GestureState.FINISHED
                    : GestureState.ONGOING
                    ));
                return true;
            }
            else if (Input.GetButtonDown("Rotate"))
            {
                profile.processGesture(new Gesture(Managers.Player.position));
                return true;
            }
            prevDir = dir;
        }
        return false;
    }
}

