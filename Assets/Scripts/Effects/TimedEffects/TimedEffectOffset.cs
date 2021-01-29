using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimedEffectOffset : TimedEffect
{
    [Range(0, 1)]
    public float offset = 0;
    public List<TimedEffect> effects;
    [SerializeField]
    [Range(0, 1)]
    private float offsetTime;

    public override void processEffect(float time)
    {
        offsetTime = (time + offset) % 1;
        effects.ForEach(fx => fx.processEffect(offsetTime));
    }
}
