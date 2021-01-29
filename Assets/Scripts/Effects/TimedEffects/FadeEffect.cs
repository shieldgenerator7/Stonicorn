using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeEffect : TimedEffect
{
    public float startFade = 1;
    public float endFade = 0;

    private SpriteRenderer sr;

    public override void processEffect(float time)
    {
        if (sr == null)
        {
            sr = GetComponent<SpriteRenderer>();
        }
        float fade = (endFade - startFade) * time + startFade;
        sr.color = sr.color.adjustAlpha(fade);
    }
}
