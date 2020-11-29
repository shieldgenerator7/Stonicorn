using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZoomOutTriggerTimeRewindTut : ZoomOutTrigger {

    public Vector2 targetPoint;//the point to determine closest past merky

    protected override void nowDiscovered()
    {
        base.nowDiscovered();
        //Highlight past merky closest to the targetPoint
        GameObject targetObject = Managers.PlayerRewind
            .getClosestPlayerGhost(targetPoint);
        Managers.Effect.highlightTapArea(targetObject.transform.position);
        Managers.PlayerRewind.tapProcessed += resetEffects;
    }

    public void resetEffects(Vector2 pos)
    {
        Managers.Effect.highlightTapArea(Vector2.zero, false);
        Managers.PlayerRewind.tapProcessed -= resetEffects;
    }
}
