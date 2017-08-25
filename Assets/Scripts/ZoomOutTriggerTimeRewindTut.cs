using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZoomOutTriggerTimeRewindTut : ZoomOutTrigger {

    public Vector2 targetPoint;//the point to determine closest past merky

    public override void trigger()
    {
        base.trigger();
        //Highlight past merky closest to the targetPoint
        GameObject targetObject = GameManager.getClosestPlayerGhost(targetPoint);
        EffectManager.highlightTapArea(targetObject.transform.position);
        GameManager.gameManagerTapProcessed += resetEffects;
    }

    public void resetEffects(Vector2 pos)
    {
        EffectManager.highlightTapArea(Vector2.zero, false);
        GameManager.gameManagerTapProcessed -= resetEffects;
    }
}
