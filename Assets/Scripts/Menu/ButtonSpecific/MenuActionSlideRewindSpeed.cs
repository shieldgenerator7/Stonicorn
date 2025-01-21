using UnityEngine;

public class MenuActionSlideRewindSpeed : MenuActionSlide
{
    public int minValue = 25;
    public int maxValue = 300;

    public override float getCurrentValue()
    {
        return (Managers.Rewind.rewindSpeedFactor * 100) - minValue;
    }

    public override void valueAdjusted(float value)
    {
        Managers.Rewind.rewindSpeedFactor = (value + minValue) / 100;
    }



    public override string getValueLabel(float currentValue)
    {
        return $"x{(currentValue + minValue) / 100}";
    }

    public override float getOverriddenMaxValue(float currentMaxValue)
    {
        return maxValue - minValue;
    }
}
