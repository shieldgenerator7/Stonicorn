using UnityEngine;

public class MenuActionSlideNPCTalkWaitDuration : MenuActionSlide
{
    public float maxWaitDuration = 10;

    public override float getCurrentValue()
    {
        return Managers.Event.talkWaitDuration * 10;
    }

    public override void valueAdjusted(float value)
    {
        Managers.Event.talkWaitDuration = value / 10;
    }

    public override float getOverriddenMaxValue(float currentMaxValue)
    {
        return maxWaitDuration * 10;
    }

    public override string getValueLabel(float currentValue)
    {
        return $"{currentValue/10}sec";
    }
}
