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
    public float afterglowDuration = 0.5f;
    private float afterglowStartSize = 1;

    public GameObject flashlight;
    public SpriteMask flashlightBeamMask;
    public SpriteRenderer flashlightPlayerGlowSR;
    private bool flashlightOn = false;
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
        flashlightBeamMask.enabled = flashlightOn;

        if (flashlightOn)
        {
            flashlight.SetActive(true);
            flashlight.transform.up = flashlightDirection;


            float percent = (flashlightDirection.magnitude - 0.5f) / maxPullBackDistance;
            Vector2 size = flashlightBeamMask.transform.localScale;
            size.y = maxBeamDistance * percent;
            flashlightBeamMask.transform.localScale = size;

            Vector2 sizeGlow = flashlightPlayerGlowSR.transform.localScale;
            sizeGlow = Vector2.one * ((maxGlowSize - minGlowSize) * (1 - percent) + minGlowSize);
            flashlightPlayerGlowSR.transform.localScale = sizeGlow;

            //adjust alpha
            float alpha = (1 - percent) * (maxAlpha - minAlpha) + minAlpha;
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
            flashlightPlayerGlowSR.enabled = true;

            playerController.Teleport.onTeleport += startFade;
        }
    }

    void startFade(Vector2 oldPos, Vector2 newPos)
    {
        playerController.Teleport.onTeleport -= startFade;
        afterglowStartSize = flashlightPlayerGlowSR.transform.localScale.x;//assume x and y are same and the sprite takes a single unit
        Timer timer = Timer.startTimer(afterglowDuration, () =>
        {
            if (!flashlightOn)
            {
                flashlight.SetActive(false);
            }
        });
        timer.onTimeLeftChanged += (timeLeft, duration) =>
        {
            if (!flashlightOn)
            {
                float percent = timeLeft / duration;
                flashlightPlayerGlowSR.color = flashlightPlayerGlowSR.color.adjustAlpha(percent * (maxAlpha - minAlpha) + minAlpha);

                Vector2 sizeGlow = flashlightPlayerGlowSR.transform.localScale;
                sizeGlow = Vector2.one * ((afterglowStartSize) * percent);
                flashlightPlayerGlowSR.transform.localScale = sizeGlow;
            }
            else
            {
                Destroy(timer);
            }
        };
    }
    #endregion

    protected override void acceptUpgradeLevel(AbilityUpgradeLevel aul)
    {
        maxPullBackDistance = aul.stat1;
    }
}
