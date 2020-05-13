using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuActionSlideScreenResolution : MenuActionSlide
{
    public override void valueAdjusted(float value)
    {
        Managers.Video.ResolutionIndex = (int)value;
    }
    public override float getCurrentValue()
    {
        return Managers.Video.ResolutionIndex;
    }

    public override float getOverriddenMaxValue(float currentMaxValue)
    {
        return Screen.resolutions.Length - 1;
    }
    public override string getValueLabel(float currentValue)
    {
        Resolution resolution = Screen.resolutions[(int)currentValue];
        return "" + resolution.width + " x " + resolution.height;
    }
}
