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

    public override void processHoldGesture(Vector2 pos, float holdTime, bool finished)
    {
        //Show a preview of where Merky will teleport
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

    public override void dropHoldGesture()
    {
        futureProjection.SetActive(false);
        teleportPreviewPointer.SetActive(false);
    }
}
