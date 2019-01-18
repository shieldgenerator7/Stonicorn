using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuActionSlideQuality : MenuActionSlide
{//2019-01-18: copied from MenuActionSlideScreenResolution

    public override void valueAdjusted(float value)
    {
        QualitySettings.SetQualityLevel((int)value);
    }
    public override float getCurrentValue()
    {
        return QualitySettings.GetQualityLevel();
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
