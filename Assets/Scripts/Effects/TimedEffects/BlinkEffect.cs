using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlinkEffect : TimedEffect
{
    public float blinkCount = 1;

    private SpriteRenderer sr;

    public override void processEffect(float time)
    {
        if (sr == null)
        {
            sr = GetComponent<SpriteRenderer>();
        }
        bool on = Mathf.Floor(blinkCount * 2 * time) % 2 == 0;
        sr.enabled = on;
    }
}
