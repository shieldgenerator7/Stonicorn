using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LongTeleportAbility : PlayerAbility
{
    [Header("Settings")]
    public float maxRangeIncreaseFactor = 2;
    public float maxDragDistance = 6;//how far out to drag the camera to get max range

    protected override void init()
    {
        base.init();
        Managers.Camera.onOffsetChange += adjustRange;
        Managers.Player.onTeleport += longTeleport;
    }
    public override void OnDisable()
    {
        base.OnDisable();
        Managers.Camera.onOffsetChange -= adjustRange;
        Managers.Player.onTeleport -= longTeleport;
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
        playerController.Range = Mathf.Max(
            playerController.baseRange,
            playerController.baseRange * maxRangeIncreaseFactor * dragFactor
            );
    }

    protected override void showTeleportEffect(Vector2 oldPos, Vector2 newPos)
    {
        if (playerController.Range > playerController.baseRange)
        {
            base.showTeleportEffect(oldPos, newPos);
        }
    }

    void longTeleport(Vector2 oldPos, Vector2 newPos)
    {
        if (playerController.Range > playerController.baseRange)
        {
            //Update Stats
            GameStatistics.addOne("LongTeleport");
        }
    }

    protected override void acceptUpgradeLevel(AbilityUpgradeLevel aul)
    {
        maxRangeIncreaseFactor = aul.stat1;
    }
}
