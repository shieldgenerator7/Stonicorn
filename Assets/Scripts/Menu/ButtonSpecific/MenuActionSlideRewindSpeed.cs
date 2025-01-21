using UnityEngine;

public class MenuActionSlideRewindSpeed : MenuActionSlide
{
    public int minValue = 25;
    public int maxValue = 300;

    public override float getCurrentValue()
    {
        return (1/(Managers.Rewind.RewindSpeedFactor) * 100) - minValue;
    }

    public override void valueAdjusted(float value)
    {
        Managers.Rewind.RewindSpeedFactor = 1/((value + minValue) / 100);
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
