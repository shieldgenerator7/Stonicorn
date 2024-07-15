using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FlashlightAbility : PlayerAbility
{
    [Header("Flashlight")]
    public float maxPullBackDistance = 6;
    public float maxBeamDistance = 6;
    public GameObject flashlight;
    public SpriteMask flashlightBeamMask;
    private bool flashlightOn = false;
    //private List<SpriteRenderer> flashlightSRs;
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

    public override void init()
    {
        base.init();
        playerController.onDragGesture += processDrag;
    }
    public override void OnDisable()
    {
        base.OnDisable();
        playerController.onDragGesture -= processDrag;
    }

    #region Input Processing

    protected override bool isGrounded() => false;
    protected override void processTeleport(Vector2 oldPos, Vector2 newPos) { }

    public void processDrag(Vector2 oldPos, Vector2 newPos, bool finished)
    {
        flashlightOn = !finished;
        FlashlightDirection = (Vector2)playerController.transform.position - newPos;
        updateFlashlightVisuals();
    }
    #endregion

    #region Visuals
    void updateFlashlightVisuals()
    {
        if (flashlightOn)
        {
            flashlight.SetActive(true);
            flashlight.transform.up = flashlightDirection;

            Vector2 size = flashlightBeamMask.transform.localScale;
            size.y = maxBeamDistance;
            flashlightBeamMask.transform.localScale = size;
            float percent = (flashlightDirection.magnitude - 0.5f) / maxPullBackDistance;
            //flashlightSRs.ForEach(flsr =>
            //    flsr.color = flsr.color.adjustAlpha(alpha)
            //);
            Vector3 pos = flashlightBeamMask.transform.localPosition;
            pos.y = percent * maxBeamDistance + size.y/2;
            flashlightBeamMask.transform.localPosition = pos;
        }
        else
        {
            flashlight?.SetActive(false);
        }
    }
    #endregion

    protected override void acceptUpgradeLevel(AbilityUpgradeLevel aul)
    {
        maxPullBackDistance = aul.stat1;
    }
}
