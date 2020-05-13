using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuActionSlideMusic : MenuActionSlide
{
    public override void valueAdjusted(float value)
    {
        Managers.Music.Volume = value;
    }

    public override float getCurrentValue()
    {
        return Managers.Music.Volume;
    }
}
