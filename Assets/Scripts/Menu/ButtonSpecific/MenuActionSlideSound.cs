using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuActionSlideSound : MenuActionSlide
{
    public override void valueAdjusted(float value)
    {
        FindObjectOfType<SoundManager>().Volume = value;
    }

    public override float getCurrentValue()
    {
        return FindObjectOfType<SoundManager>().Volume;
    }
}
