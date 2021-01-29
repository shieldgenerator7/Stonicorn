using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveEffect : TimedEffect
{
    public Vector2 startPos = Vector2.zero;
    public Vector2 endPos = Vector2.one;

    public override void processEffect(float time)
    {
        Vector2 pos = (endPos - startPos) * time + startPos;
        transform.localPosition = pos;
    }
}
