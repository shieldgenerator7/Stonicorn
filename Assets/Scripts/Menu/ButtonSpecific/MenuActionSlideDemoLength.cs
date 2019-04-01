using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuActionSlideDemoLength : MenuActionSlide
{
    public override void valueAdjusted(float value)
    {
        GameManager.GameDemoLength = value * 60;
    }

    public override float getCurrentValue()
    {
        return GameManager.GameDemoLength / 60;
    }
}
