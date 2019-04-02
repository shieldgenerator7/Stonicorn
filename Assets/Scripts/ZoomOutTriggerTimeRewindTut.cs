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
        EffectManager.highlightTapArea(targetObject.transform.position);
        Managers.Game.tapProcessed += resetEffects;
    }

    public void resetEffects(Vector2 pos)
    {
        EffectManager.highlightTapArea(Vector2.zero, false);
        Managers.Game.tapProcessed -= resetEffects;
    }
}
