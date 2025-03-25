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
    public float glowAlpha = -1;//override alpha animation if between 0 and 1

    public GameObject flashlight;
    public SpriteMask flashlightBeamMask;
    public SpriteRenderer flashlightPlayerGlowSR;
    private bool flashlightOn = false;
    private bool flashAuraOn = false;
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
        if (playerController)
        {
            playerController.onDragGesture -= processDrag;
            if (register)
            {
                playerController.onDragGesture += processDrag;
            }
        }

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
                flashAuraOn = true;
                break;
            case GestureState.FINISH:
                flashlightOn = false;
                break;
        }
        float percent = (flashlightDirection.magnitude) / maxPullBackDistance;
        updateFlashlightVisuals(1, percent);
        updateFlashAuraVisuals(1 - percent);
    }

    private void camOffsetChanged(Vector3 offset)
    {
        float percent;
        if (Managers.Camera.offsetOffPlayer())
        //if ((Vector2)offset != Vector2.zero)
        {
            FlashlightDirection = (Vector2)offset.normalized * maxPullBackDistance;

            flashlightOn = true;
            flashAuraOn = true;

            percent = 1;
        }
        else
        {
            flashlightOn = false;
            flashAuraOn = false;

            percent = 0;
        }

        updateFlashlightVisuals(1, percent);
        updateFlashAuraVisuals(percent);
    }
    #endregion

    #region Visuals
    void updateFlashlightVisuals(float alpaPercent, float pullpercent = -1)
    {
        flashlightBeamMask.enabled = flashlightOn;

        if (flashlightOn)
        {
            flashlight.SetActive(true);
            flashlight.transform.up = flashlightDirection;

            if (pullpercent >= 0)
            {
                Vector2 size = flashlightBeamMask.transform.localScale;
                size.y = maxBeamDistance * pullpercent;
                flashlightBeamMask.transform.localScale = size;
            }

            //adjust alpha
            float alpha = (alpaPercent) * (maxAlpha - minAlpha) + minAlpha;
            flashlightSRs.ForEach(flsr =>
                flsr.color = flsr.color.adjustAlpha(alpha)
            );

            //enable sprites
            flashlightSRs.ForEach(flsr => flsr.enabled = true);

        }
        else
        {
            flashlightSRs.ForEach(flsr =>
                flsr.enabled = false
            );
        }
    }
    void updateFlashAuraVisuals(float percent, float maxSize = 0)
    {
        flashlightPlayerGlowSR.enabled = percent > 0;
        if (maxSize == 0)
        {
            maxSize = maxGlowSize;
        }
        maxSize = Mathf.Clamp(maxSize, minGlowSize, maxGlowSize);
        Vector2 sizeGlow = Vector2.one * ((maxSize - minGlowSize) * (percent) + minGlowSize);
        flashlightPlayerGlowSR.transform.localScale = sizeGlow;
        if (Utility.between(glowAlpha, 0, 1))
        {
            flashlightPlayerGlowSR.color = flashlightPlayerGlowSR.color.adjustAlpha(glowAlpha);
        }
        else
        {
            flashlightPlayerGlowSR.color = flashlightPlayerGlowSR.color.adjustAlpha(percent * (maxAlpha - minAlpha) + minAlpha);
        }
    }


    void turnOff()
    {
        flashlightOn = false;
        flashAuraOn = false;
        flashlight.SetActive(false);
        flashAuraOn = false;
        updateFlashlightVisuals(0);
        updateFlashAuraVisuals(0);
    }
    #endregion

    protected override void acceptUpgradeLevel(AbilityUpgradeLevel aul)
    {
        maxPullBackDistance = aul.stat1;
    }

    static byte key_flashlightDirection = 0;
    static byte key_flashlightOn = 1;
    static byte key_flashAuraOn = 2;
    public override SavableObject CurrentState
    {
        get => base.CurrentState.more(
            key_flashlightDirection, flashlightDirection,
            key_flashlightOn, flashlightOn,
            key_flashAuraOn, flashAuraOn
            );
        set
        {
            bool prevlight = flashlightOn;
            bool prevaura = flashAuraOn;
            flashlightOn = value.Bool(key_flashlightOn);
            flashAuraOn = value.Bool(key_flashAuraOn);
            flashlightDirection = value.Vector2(key_flashlightDirection);
            if (flashlightOn != prevlight || flashAuraOn != prevaura)
            {
                FlashlightDirection = flashlightDirection;
                updateFlashlightVisuals(1);
            }
        }
    }
}
