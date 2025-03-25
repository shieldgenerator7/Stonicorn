using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FlashlightAbility : PlayerAbility
{
    [Header("Flashlight")]
    public float maxPullBackDistance = 6;
    public float maxBeamDistance = 6;
    [Range(0, 1)]
    public float minAlpha = 0.1f;
    [Range(0, 1)]
    public float maxAlpha = 1f;
    [Range(0, 10)]
    public float minGlowSize = 2f;
    [Range(0, 10)]
    public float maxGlowSize = 3f;
    [Range(0, 1)]
    public float glowAlpha = -1;//override alpha animation if between 0 and 1

    public GameObject flashlight;
    public Transform flashlightBeamTransform;
    public Transform flashlightAuraTransform;
    private bool flashlightOn = false;
    [AutoInitialize(Container = "flashlight", SearchChildren = true), SerializeField, HideInInspector]
    private List<SpriteRenderer> flashlightSRs;
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

    protected override void registerDelegates(bool register = true)
    {
        Managers.Camera.onOffsetChange -= camOffsetChanged;
        if (register)
        {
            Managers.Camera.onOffsetChange += camOffsetChanged;
        }
    }

    #region Input Processing

    protected override bool isGrounded() => false;
    protected override void processTeleport(Vector2 oldPos, Vector2 newPos)
    {
    }

    public void processDrag(Vector2 oldPos, Vector2 newPos, GestureState state)
    {
        FlashlightDirection = (Vector2)playerController.transform.position - newPos;
        switch (state)
        {
            case GestureState.START:
                //TODO: implement? //2025-01-05: i suspect this hasnt been implemented yet
                return;
            case GestureState.ONGOING:
                flashlightOn = true;
                break;
            case GestureState.FINISH:
                if (!Managers.Camera.offsetOffPlayer())
                {
                flashlightOn = false;
                }
                else
                {
                    FlashlightDirection = flashlightDirection.normalized * maxBeamDistance;
                }
                break;
        }
        float percent = (flashlightDirection.magnitude) / maxPullBackDistance;
        updateVisuals();
    }

    private void camOffsetChanged(Vector3 offset)
    {
        if (Managers.Camera.offsetOffPlayer())
        //if ((Vector2)offset != Vector2.zero)
        {
            FlashlightDirection = (Vector2)offset.normalized * maxPullBackDistance;

            flashlightOn = true;
        }
        else
        {
            flashlightOn = false;
        }

        updateVisuals();
    }
    #endregion

    #region Visuals
    void updateVisuals()
    {
        //flashlight & aura
        if (flashlightOn)
        {
            //flashlight
            flashlight.transform.up = flashlightDirection;

                Vector2 size = flashlightBeamTransform.localScale;
                size.y = maxBeamDistance;
                flashlightBeamTransform.localScale = size;

            //aura
            float aurasize = maxGlowSize;
            Vector2 sizeGlow = Vector2.one * aurasize;
            flashlightAuraTransform.localScale = sizeGlow;

            //adjust alpha
            float alpha = (glowAlpha) * (maxAlpha - minAlpha) + minAlpha;
            flashlightSRs.ForEach(flsr =>
                flsr.color = flsr.color.adjustAlpha(alpha)
            );

        }

        //general
        flashlight.SetActive(flashlightOn);
    }


    void turnOff()
    {
        flashlightOn = false;
        updateVisuals();
    }
    #endregion

    protected override void acceptUpgradeLevel(AbilityUpgradeLevel aul)
    {
        maxPullBackDistance = aul.stat1;
    }

    static byte key_flashlightDirection = 0;
    static byte key_flashlightOn = 1;
    public override SavableObject CurrentState
    {
        get => base.CurrentState.more(
            key_flashlightDirection, flashlightDirection,
            key_flashlightOn, flashlightOn
            );
        set
        {
            bool prevlight = flashlightOn;
            Vector2 prevDir = flashlightDirection;
            flashlightOn = value.Bool(key_flashlightOn);
            flashlightDirection = value.Vector2(key_flashlightDirection);
            if (flashlightOn != prevlight || flashlightDirection != prevDir)
            {
                FlashlightDirection = flashlightDirection;
                updateVisuals();
            }
        }
    }
}
