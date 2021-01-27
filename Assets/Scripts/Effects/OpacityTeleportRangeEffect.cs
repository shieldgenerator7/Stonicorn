using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpacityTeleportRangeEffect : TimedTeleportRangeEffect
{
    [Range(0, 1)]
    public float transparency = 1.0f;
    [Range(0, 1)]
    public float timeTransparency = 0.5f;

    public override void updateEffect()
    {
        ttre.fragmentsBurned.ForEach(fragment =>
        {
            SpriteRenderer sr = fragment.GetComponent<SpriteRenderer>();
            float newAlpha = transparency;
            sr.color = sr.color.adjustAlpha(newAlpha);
        });
        ttre.fragmentsFuse.ForEach(fragment =>
        {
            SpriteRenderer sr = fragment.GetComponent<SpriteRenderer>();
            float newAlpha = timeTransparency;
            sr.color = sr.color.adjustAlpha(newAlpha);
        });
    }
}
