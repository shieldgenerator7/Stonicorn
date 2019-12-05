using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuActionSlideQuality : MenuActionSlide
{//2019-01-18: copied from MenuActionSlideScreenResolution

    public override void valueAdjusted(float value)
    {
        Managers.Video.QualityLevel = (int)value;
    }
    public override float getCurrentValue()
    {
        return Managers.Video.QualityLevel;
    }

    public override float getOverriddenMaxValue(float currentMaxValue)
    {
        return QualitySettings.names.Length - 1;
    }
    public override string getValueLabel(float currentValue)
    {
        return QualitySettings.names[(int)currentValue];
    }
}
