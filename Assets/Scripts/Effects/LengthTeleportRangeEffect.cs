using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LengthTeleportRangeEffect : TimedTeleportRangeEffect
{
    [Range(0, 1)]
    public float normalLength = 0.5f;
    [Range(0, 1)]
    public float timeLength = 0.8f;

    public override void updateEffect()
    {
        ttre.fragmentsBurned.ForEach(fragment =>
        {
            Vector3 scale = fragment.transform.localScale;
            scale.y = normalLength;
            fragment.transform.localScale = scale;
        });
        ttre.fragmentsFuse.ForEach(fragment =>
        {
            Vector3 scale = fragment.transform.localScale;
            scale.y = timeLength;
            fragment.transform.localScale = scale;
        });
    }
}
