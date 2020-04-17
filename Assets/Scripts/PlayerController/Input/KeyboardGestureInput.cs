using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyboardGestureInput : GestureInput
{
    public float holdThreshold = 0.2f;

    private Vector2 origPosScreen;
    private Vector2 OrigPosWorld
    {
        get => Utility.ScreenToWorldPoint(origPosScreen);
    }
    private float origTime;

    public override InputDeviceMethod InputType
    {
        get => InputDeviceMethod.KEYBOARD;
    }

    public override bool InputOngoing
    {
        get => Input.anyKey;
    }

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
            if (horizontal != 0 || vertical != 0)
            {
                Vector2 dir = (
                    (Camera.main.transform.up * vertical)
                    + (Camera.main.transform.right * horizontal)
                    ).normalized;
                float range = Managers.Player.baseRange;
                if (Input.GetButton("Short"))
                {
                    range /= 2;
                }
                profile.processTapGesture(
                    (Vector2)Managers.Player.transform.position + (dir * range)
                    );
                return true;
            }
            else if (Input.GetButtonDown("Rotate"))
            {
                profile.processTapGesture(Managers.Player.transform.position);
                return true;
            }
        }
        return false;
    }
}

