using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuActionSlideScreenResolution : MenuActionSlide
{
    public override void valueAdjusted(float value)
    {
        Resolution resolution = Screen.resolutions[(int)value];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }
    public override float getCurrentValue()
    {
        for (int i = 0; i < Screen.resolutions.Length; i++)
        {
            if (Screen.resolutions[i].width >= Screen.width
                && Screen.resolutions[i].height >= Screen.height)
            {
                return i;
            }
        }
        return Screen.resolutions.Length - 1;
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
