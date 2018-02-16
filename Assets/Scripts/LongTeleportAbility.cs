using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LongTeleportAbility : PlayerAbility {

    protected override void Start()
    {
        base.Start();
        playerController.Cam.onOffsetChange += adjustRange;
    }

    /// <summary>
    /// Adjusts (increases) Merky's range the further out the camera is dragged.
    /// </summary>
    void adjustRange()
    {
        playerController.Range = playerController.baseRange + ((Vector2)playerController.Cam.Offset).magnitude;
    }

}
