using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FlashlightAbility : PlayerAbility
{
    [Header("Flashlight")]
    public float baseAuraRadius = 0.5f;
    [Range(0, 20)]
    public float beamLength = 6;
    [Range(0, 20)]
    public float auraRadius = 1.5f;

    [Header("Flashlight Components")]
    public GameObject flashlight;
    public Transform flashlightBeamTransform;
    public Transform flashlightAuraTransform;
    private bool flashlightOn = false;
    private Vector2 flashlightDirection;
    public Vector2 FlashlightDirection
    {
        get => flashlightDirection;
        private set
        {
            flashlightDirection = value.normalized * beamLength;
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

    private void camOffsetChanged(Vector3 offset)
    {
        if (Managers.Camera.offsetOffPlayer())
        //if ((Vector2)offset != Vector2.zero)
        {
            FlashlightDirection = offset;

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
                size.y = beamLength;
                flashlightBeamTransform.localScale = size;

            //aura
            Vector2 sizeGlow = Vector2.one * (baseAuraRadius + auraRadius);
            flashlightAuraTransform.localScale = sizeGlow;

        }

        //general
        flashlight.SetActive(flashlightOn);
    }
    #endregion

    protected override void acceptUpgradeLevel(AbilityUpgradeLevel aul)
    {
        beamLength = aul.stat1;
        auraRadius = aul.stat2;
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
