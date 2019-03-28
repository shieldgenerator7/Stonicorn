using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LongTeleportAbility : PlayerAbility {

    protected override void init()
    {
        base.init();
        playerController.Cam.onOffsetChange += adjustRange;
    }
    public override void OnDisable()
    {
        base.OnDisable();
        playerController.Cam.onOffsetChange -= adjustRange;
    }

    /// <summary>
    /// Adjusts (increases) Merky's range the further out the camera is dragged.
    /// </summary>
    void adjustRange()
    {
        playerController.Range = playerController.baseRange + ((Vector2)playerController.Cam.Offset).magnitude;
    }

    protected override void showTeleportEffect(Vector2 oldPos, Vector2 newPos)
    {
        if (playerController.Range > playerController.baseRange)
        {
            base.showTeleportEffect(oldPos, newPos);
        }
    }

}
