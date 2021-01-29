using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateEffect : TimedEffect
{
    public float startAngle = 0;
    public float endAngle = 360;

    public override void processEffect(float time)
    {
        float angle = (endAngle - startAngle) * time + startAngle;
        transform.eulerAngles = new Vector3(0, 0, angle);
    }
}
