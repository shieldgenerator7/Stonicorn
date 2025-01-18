using UnityEngine;

public class MenuActionSlideNPCTalkSpeed : MenuActionSlide
{
    public int minValue = 25;
    public int maxValue = 200;

    public override float getCurrentValue()
    {
        return (Managers.Event.TalkSpeedMultiplier * 100) - minValue;
    }

    public override void valueAdjusted(float value)
    {
        Managers.Event.TalkSpeedMultiplier = (value + minValue) / 100;
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
