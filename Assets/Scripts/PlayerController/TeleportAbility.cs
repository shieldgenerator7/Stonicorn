using UnityEngine;
using System.Collections;

/// <summary>
/// Currently just used for the teleport hold gesture effect,
/// but might actually have teleport capabilities in the future
/// </summary>
public class TeleportAbility : PlayerAbility
{//2017-08-07: copied from ForceTeleportAbility
    [Header("Future Projection")]
    public GameObject futureProjection;//the object that is used to show a preview of the landing spot
    public GameObject teleportPreviewPointer;//the object that visually points at the future projection
    [Header("Flashlight")]
    public float maxPullBackDistance = 6;
    public GameObject flashlightPrefab;//prefab
    private GameObject flashlight;
    private bool flashlightOn = false;
    private Vector2 flashlightDirection;
    public Vector2 FlashlightDirection
    {
        get => flashlightDirection;
        private set
        {
            flashlightDirection = value;
            if (flashlightDirection.magnitude > maxPullBackDistance)
            {
                flashlightDirection = flashlightDirection.normalized * maxPullBackDistance;
            }
        }
    }

    protected override void init()
    {
        base.init();
        playerController.onDragGesture += processDrag;
    }

    public override void OnDisable()
    {
        base.OnDisable();
        playerController.onDragGesture -= processDrag;
    }

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

    public void processDrag(Vector2 oldPos, Vector2 newPos, bool finished)
    {
        flashlightOn = !finished;
        FlashlightDirection = (Vector2)playerController.transform.position - newPos;
        updateFlashlightVisuals();
    }

    void updateFlashlightVisuals()
    {
        if (flashlightOn)
        {
            if (this.flashlight == null)
            {
                this.flashlight = Instantiate(flashlightPrefab);
                this.flashlight.transform.parent = transform;
                this.flashlight.transform.localPosition = Vector2.zero;
            }
            flashlight.SetActive(true);
            flashlight.transform.up = flashlightDirection;
            flashlight.transform.localScale = new Vector3(
                1,
                flashlightDirection.magnitude,
                1
                );
        }
        else
        {
            flashlight?.SetActive(false);
        }
    }
}
