using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LongTeleportAbility : PlayerAbility
{
    [Header("Settings")]
    public float maxRangeIncreaseFactor = 2;
    public float maxDragDistance = 6;//how far out to drag the camera to get max range
    public float postShieldKnockbackSpeed = 10;

    [SerializeField]
    private bool shielded = false;
    public bool Shielded
    {
        get => shielded;
        set
        {
            shielded = value;
        }
    }


    protected override void init()
    {
        base.init();
        Managers.Camera.onOffsetChange += adjustRange;
    }
    public override void OnDisable()
    {
        base.OnDisable();
        Managers.Camera.onOffsetChange -= adjustRange;
        playerController.onHazardHitException -= onHitException;
    }
    }

    /// <summary>
    /// Adjusts (increases) Merky's range the further out the camera is dragged.
    /// </summary>
    void adjustRange(Vector3 offset)
    {
        float dragFactor = Mathf.Min(
            ((Vector2)offset).magnitude / maxDragDistance,
            1
            );
        playerController.Teleport.Range = Mathf.Max(
            playerController.Teleport.baseRange,
            playerController.Teleport.baseRange * maxRangeIncreaseFactor * dragFactor
            );
    }

    protected override void processTeleport(Vector2 oldPos, Vector2 newPos)
    {
        if (playerController.Teleport.Range > playerController.Teleport.baseRange)
        {
            //Update Stats
            Managers.Stats.addOne("LongTeleport");
            //Upgrade 1
            if (CanShield)
            {
                applyShield();
            }
            //Effect teleport
            effectTeleport(oldPos, newPos);
        }
    }

    protected override bool isGrounded() => false;

    bool CanShield =>
        FeatureLevel >= 1;

    void applyShield()
    {
        Shielded = true;
    }
    bool onHitException(Vector2 contactPoint)
    {
        bool prevShielded = shielded;
        if (shielded)
        {
            //Remove shield
            Shielded = false;
            //Stop merky
            rb2d.nullifyMovement();
            //Move merky away from hazard
            rb2d.velocity += ((Vector2)transform.position - contactPoint)
                * postShieldKnockbackSpeed;
        }
        return prevShielded;
    }

    protected override void acceptUpgradeLevel(AbilityUpgradeLevel aul)
    {
        maxRangeIncreaseFactor = aul.stat1;
    }
}
