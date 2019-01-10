using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuActionSlideDemoLength : MenuActionSlide
{
    public override void valueAdjusted(float value)
    {
       GameManager.setResetTimer(value * 60);
    }

    public override float getCurrentValue()
    {
        return GameManager.getGameDemoLength() / 60;
    }
}
