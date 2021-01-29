using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaleEffect : TimedEffect
{
    public float startScale = 1;
    public float endScale = 2;

    public override void processEffect(float time)
    {
        float scale = (endScale - startScale) * time + startScale;
        transform.localScale = Vector3.one * scale;
    }
}
