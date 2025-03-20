using UnityEngine;

public class MenuActionSlideSkin : MenuActionSlide
{


    public override float getCurrentValue()
    {
        return Managers.Skin.SkinIndex;
    }

    public override void valueAdjusted(float value)
    {
        Managers.Skin.SkinIndex = (int)value;
    }

    public override float getOverriddenMaxValue(float currentMaxValue)
    {
        return Managers.Skin.FoundSkinCount-1;
    }

    public override string getValueLabel(float currentValue)
    {
        return Managers.Skin.getSkin((int)currentValue).name;
    }
}
