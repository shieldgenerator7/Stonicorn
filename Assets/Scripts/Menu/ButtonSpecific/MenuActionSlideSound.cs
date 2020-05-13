using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuActionSlideSound : MenuActionSlide
{
    public override void valueAdjusted(float value)
    {
        Managers.Sound.Volume = value;
    }

    public override float getCurrentValue()
    {
        return Managers.Sound.Volume;
    }
}
