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

    public override bool processInput(PlayGestureProfile profile)
    {
        float time = Time.unscaledTime;
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
                    gestureStartTime = time;
                }
                float range = Managers.Player.Teleport.baseRange;
                if (Input.GetButton("Short"))
                {
                    range /= 2;
                }
                profile.processHoldGesture(
                    (Vector2)Managers.Player.transform.position + (dir * range),
                    time - gestureStartTime,
                    !isInputNow
                    );
                return true;
            }
            else if (Input.GetButtonDown("Rotate"))
            {
                profile.processTapGesture(Managers.Player.transform.position);
                return true;
            }
            prevDir = dir;
        }
        return false;
    }
}

