using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuActionSlideMusic : MenuActionSlide
{
    public override void valueAdjusted(float value)
    {
        FindObjectOfType<MusicManager>().Volume = value;
    }

    public override float getCurrentValue()
    {
        return FindObjectOfType<MusicManager>().Volume;
    }
}
