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
    public float afterglowDuration = 0.5f;
    private float afterglowStartSize = 1;

    public GameObject flashlight;
    public SpriteMask flashlightBeamMask;
    public SpriteRenderer flashlightPlayerGlowSR;
    private bool flashlightOn = false;
    private bool flashAuraOn = false;
    private List<SpriteRenderer> flashlightSRs;
    private Vector2 originalFlashlightDirection;
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

    Timer timer;

    public override void init()
    {
        base.init();
        playerController.onDragGesture += processDrag;

        this.flashlightSRs = this.flashlight.GetComponentsInChildren<SpriteRenderer>().ToList();
    }
    public override void OnDisable()
    {
        base.OnDisable();
        playerController.onDragGesture -= processDrag;
    }

    #region Input Processing

    protected override bool isGrounded() => false;
    protected override void processTeleport(Vector2 oldPos, Vector2 newPos)
    {
        if (flashlightOn || flashAuraOn)
        {
            //FlashlightDirection = originalFlashlightDirection;
            //updateFlashlightVisuals();
            updateFlashAuraVisuals(1, afterglowStartSize);
            startFade();
        }
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
                if (timer)
                {
                    Destroy(timer);
                }
                afterglowStartSize = 0;
                flashlightOn = true;
                flashAuraOn = true;
                originalFlashlightDirection = flashlightDirection;
                break;
            case GestureState.FINISH:
                afterglowStartSize = flashlightPlayerGlowSR.transform.localScale.x;//assume x and y are same and the sprite takes a single unit
                startFade();
                break;
        }
        updateFlashlightVisuals();
    }
    #endregion

    #region Visuals
    void updateFlashlightVisuals()
    {
        flashlightBeamMask.enabled = flashlightOn;

        if (flashlightOn)
        {
            flashlight.SetActive(true);
            flashlight.transform.up = flashlightDirection;


            float percent = (flashlightDirection.magnitude - 0.5f) / maxPullBackDistance;
            Vector2 size = flashlightBeamMask.transform.localScale;
            size.y = maxBeamDistance * percent;
            flashlightBeamMask.transform.localScale = size;

            //adjust alpha
            float alpha = (1 - percent) * (maxAlpha - minAlpha) + minAlpha;
            flashlightSRs.ForEach(flsr =>
                flsr.color = flsr.color.adjustAlpha(alpha)
            );

            //aura
            updateFlashAuraVisuals(1 - percent);

            //enable sprites
            flashlightSRs.ForEach(flsr => flsr.enabled = true);

        }
        else
        {
            flashlightSRs.ForEach(flsr =>
                flsr.enabled = false
            );
            updateFlashAuraVisuals(1, afterglowStartSize);
        }
    }
    void updateFlashAuraVisuals(float percent, float maxSize = 0)
    {
        flashlightPlayerGlowSR.enabled = percent > 0;
        Vector2 sizeGlow = flashlightPlayerGlowSR.transform.localScale;
        if (maxSize > 0)
        {
            sizeGlow = Vector2.one * ((afterglowStartSize) * percent);
        }
        else
        {
            sizeGlow = Vector2.one * ((maxGlowSize - minGlowSize) * (percent) + minGlowSize);
        }
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

    void startFade()
    {
        if (timer)
        {
            Destroy(timer);
        }
        timer = Timer.startTimer(afterglowDuration, () =>
        {
            flashlightOn = false;
            flashAuraOn = false;
                flashlight.SetActive(false);
                flashAuraOn = false;
                afterglowStartSize = 0;
            updateFlashlightVisuals();
            updateFlashAuraVisuals(0);
        });
        timer.onTimeLeftChanged += (timeLeft, duration) =>
        {
                float percent = Mathf.Clamp(timeLeft / duration, 0, 1);
            float min = 0.2f;
            FlashlightDirection = originalFlashlightDirection.normalized * (
                percent * (originalFlashlightDirection.magnitude - min) + min
            );
            updateFlashlightVisuals();
                updateFlashAuraVisuals(percent, afterglowStartSize);

        };
    }
    #endregion

    protected override void acceptUpgradeLevel(AbilityUpgradeLevel aul)
    {
        maxPullBackDistance = aul.stat1;
    }

    public override SavableObject CurrentState
    {
        get => base.CurrentState.more(
            "flashlightDirection", flashlightDirection,
            "flashlightOn", flashlightOn,
            "flashAuraOn", flashAuraOn
            );
        set
        {
            bool prevlight = flashlightOn;
            bool prevaura = flashAuraOn;
            flashlightOn = value.Bool("flashlightOn");
            flashAuraOn = value.Bool("flashAuraOn");
            flashlightDirection = value.Vector2("flashlightDirection");
            if (flashlightOn != prevlight || flashAuraOn != prevaura)
            {
                FlashlightDirection = flashlightDirection;
                updateFlashlightVisuals();
                startFade();
            }
        }
    }
}
