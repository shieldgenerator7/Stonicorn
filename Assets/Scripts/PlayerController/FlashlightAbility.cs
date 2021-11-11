using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlashlightAbility : StonicornAbility
{
    [Header("Flashlight")]
    public float maxPullBackDistance = 6;
    public GameObject flashlightPrefab;//prefab
    private GameObject flashlight;
    private bool flashlightOn = false;
    private SpriteRenderer flashlightSR;
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
        //TODO: Refactor
        //stonicorn.onDragGesture += processDrag;
    }
    public override void OnDisable()
    {
        base.OnDisable();
        //TODO: Refactor
        //stonicorn.onDragGesture -= processDrag;
    }

    #region Input Processing

    protected override bool isGrounded() => false;
    protected override void processTeleport(Vector2 oldPos, Vector2 newPos) { }

    public void processDrag(Vector2 oldPos, Vector2 newPos, GestureState state)
    {
        flashlightOn = !(state == GestureState.FINISHED);
        FlashlightDirection = (Vector2)stonicorn.transform.position - newPos;
        updateFlashlightVisuals();
    }
    #endregion

    #region Visuals
    void updateFlashlightVisuals()
    {
        if (flashlightOn)
        {
            if (this.flashlight == null)
            {
                this.flashlight = Instantiate(flashlightPrefab);
                this.flashlight.transform.parent = transform;
                this.flashlight.transform.localPosition = Vector2.zero;
                this.flashlightSR = this.flashlight.GetComponent<SpriteRenderer>();
            }
            flashlight.SetActive(true);
            flashlight.transform.up = flashlightDirection;
            flashlightSR.color = flashlightSR.color.adjustAlpha(
                    (flashlightDirection.magnitude - 0.5f) / maxPullBackDistance
                    );
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
