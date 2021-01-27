using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionTeleportRangeEffect : TeleportRangeEffect
{
    public float rangeOffset = 1;
    public float size = 1;

    public override void updateEffect()
    {
        float range = updater.Range;
        updater.fragments.ForEach(fragment =>
        {
            //Position
            fragment.transform.localPosition =
                    fragment.transform.localPosition.normalized
                    * (range + rangeOffset);
            //Size
            Vector3 scale = fragment.transform.localScale;
            scale.y = size;
            fragment.transform.localScale = scale;
        });
    }
}
