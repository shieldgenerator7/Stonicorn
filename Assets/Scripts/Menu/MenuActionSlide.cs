using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MenuActionSlide : MonoBehaviour
{
    public abstract void valueAdjusted(float value);

    public abstract float getCurrentValue();

    public virtual float getOverriddenMaxValue(float currentMaxValue)
    {
        return currentMaxValue;
    }

    public virtual string getValueLabel(float currentValue)
    {
        return ""+currentValue;
    }
}
