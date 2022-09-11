using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LongTeleportAbility : PlayerAbility
{
    [Header("Settings")]
    public float maxRangeIncreaseFactor = 2;
    public float maxDragDistance = 6;//how far out to drag the camera to get max range
    public float portalRequiredRangeFactor = 3;//how much further than the standard range you have to teleport in order to make a portal

    [Header("Components")]
    public GameObject portalPrefab;

    public override void init()
    {
        base.init();
        Managers.Camera.onOffsetChange += adjustRange;
    }
    public override void OnDisable()
    {
        base.OnDisable();
        Managers.Camera.onOffsetChange -= adjustRange;
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
            Managers.Stats.addOne(Stat.LONG_TELEPORT);
            //Upgrade 1
            //Upgrade 2
            if (canPortal(oldPos, newPos))
            {
                applyPortal(oldPos, newPos);
            }
            //Effect teleport
            effectTeleport(oldPos, newPos);
        }
    }

    protected override bool isGrounded() => false;

    bool canPortal(Vector2 oldPos, Vector2 newPos)
    {
        List<TeleportPortal> portalList = FindObjectsOfType<TeleportPortal>().ToList();
        //Require max upgrade level
        return FeatureLevel >= 2
            //Require certain distance apart
            && !oldPos.inRange(
                newPos,
                playerController.Teleport.baseRange * portalRequiredRangeFactor
                )
            //Don't overlap portals
            && !portalList.Any(portal => portal.containsPoint(oldPos))
            && !portalList.Any(portal => portal.containsPoint(newPos));
    }

    void applyPortal(Vector2 oldPos, Vector2 newPos)
    {
        TeleportPortal portal1 = makePortal(oldPos);
        TeleportPortal portal2 = makePortal(newPos);
        portal1.connectTo(portal2);
    }

    TeleportPortal makePortal(Vector2 pos)
    {
        GameObject portal = Utility.Instantiate(portalPrefab);
        portal.transform.position = pos;
        SpriteRenderer sr = portal.GetComponent<SpriteRenderer>();
        sr.color = EffectColor.adjustAlpha(sr.color.a);
        return portal.GetComponent<TeleportPortal>();
    }

    protected override void acceptUpgradeLevel(AbilityUpgradeLevel aul)
    {
        maxRangeIncreaseFactor = aul.stat1;
    }
}
