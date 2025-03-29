using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuActionSlideNPCVOlume : MenuActionSlide
{
    public override void valueAdjusted(float value)
    {
        Managers.Event.Volume = value / 10;
    }

    public override float getCurrentValue()
    {
        return Managers.Event.Volume * 10;
    }

    public override string getValueLabel(float currentValue)
    {
        return $"{Managers.Event.Volume * 100}";
    }
}
