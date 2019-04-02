using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZoomOutTriggerTimeRewindTut : ZoomOutTrigger {

    public Vector2 targetPoint;//the point to determine closest past merky

    public override void trigger()
    {
        base.trigger();
        //Highlight past merky closest to the targetPoint
        GameObject targetObject = Managers.Game.getClosestPlayerGhost(targetPoint);
        Managers.Effect.highlightTapArea(targetObject.transform.position);
        Managers.Game.tapProcessed += resetEffects;
    }

    public void resetEffects(Vector2 pos)
    {
        Managers.Effect.highlightTapArea(Vector2.zero, false);
        Managers.Game.tapProcessed -= resetEffects;
    }
}
