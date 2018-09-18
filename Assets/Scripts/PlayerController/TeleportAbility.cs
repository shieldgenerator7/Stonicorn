using UnityEngine;
using System.Collections;

/// <summary>
/// Currently just used for the teleport hold gesture effect,
/// but might actually have teleport capabilities in the future
/// </summary>
public class TeleportAbility : PlayerAbility
{//2017-08-07: copied from ForceTeleportAbility
    public GameObject futureProjection;//the object that is used to show a preview of the landing spot
    public GameObject teleportPreviewPointer;//the object that visually points at the future projection

    public float maxRange = 3;
    public float maxHoldTime = 1;//how long until the max range is reached

    public new bool takesGesture()
    {
        return true;
    }

    public new bool takesHoldGesture()
    {
        return true;
    }

    public override void processHoldGesture(Vector2 pos, float holdTime, bool finished)
    {
        float range = maxRange * holdTime * GestureManager.holdTimeScaleRecip / maxHoldTime;
        if (range > maxRange)
        {
            range = maxRange;
        }
        if (finished)
        {
            futureProjection.SetActive(false);
            teleportPreviewPointer.SetActive(false);
        }
        else
        {
            Vector2 futurePos = playerController.findTeleportablePosition(pos);
            //Future Projection
            futureProjection.SetActive(true);
            futureProjection.transform.rotation = transform.rotation;
            futureProjection.transform.localScale = transform.localScale;
            futureProjection.transform.position = futurePos;
            //Teleport Preview Pointer
            teleportPreviewPointer.SetActive(true);
            teleportPreviewPointer.transform.localScale = transform.localScale;
            teleportPreviewPointer.transform.position = futurePos;
            //Account for teleport-on-player
            if (playerController.gestureOnPlayer(futurePos))
            {
                float newAngle = playerController.getNextRotation(futureProjection.transform.localEulerAngles.z);
                futureProjection.transform.localEulerAngles = new Vector3(0, 0, newAngle);
            }
        }
    }

    public override void dropHoldGesture()
    {
        futureProjection.SetActive(false);
        teleportPreviewPointer.SetActive(false);
    }
}
