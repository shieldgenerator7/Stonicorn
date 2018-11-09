using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Cycles an object between hidden and shown, using sprite alpha.
/// </summary>
public class SimpleBlinking : MonoBehaviour
{

    public float onTime = 1;//how long to be shown
    public float offTime = 1;//how long to be hidden
    public float onOffTransition = 0;//how long it takes after onTime ends to go all the way to off
    public float offOnTransition = 0;//how long it takes after offTime ends to go all the way to on

    [Tooltip("How far along into the first cycle it starts")]
    public float initialStartTime = 0;//how far along into the first cycle it starts

    public enum BlinkState
    {
        ON,//shown
        OFF//hidden
    }
    public BlinkState blinkState;

    private float lastKeyFrame = 0;//the last time it was switched
    private float currentDuration = 0;//the current span between keyframes

    private SpriteRenderer sr;

    // Use this for initialization
    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        lastKeyFrame = initialStartTime;
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time > lastKeyFrame + currentDuration)
        {
            lastKeyFrame = Time.time;
            switch (blinkState)
            {
                case BlinkState.ON:
                    blinkState = BlinkState.OFF;
                    currentDuration = offTime;
                    if (onOffTransition == 0)
                    {
                        updateAlpha();
                    }
                    break;
                case BlinkState.OFF:
                    blinkState = BlinkState.ON;
                    currentDuration = onTime;
                    if (offOnTransition == 0)
                    {
                        updateAlpha();
                    }
                    break;
            }
        }
	}

    void updateAlpha(float percent = 1)
    {
        Color c = sr.color;
        c.a = (blinkState == BlinkState.ON) ? 0 + percent : 1 - percent;
        sr.color = c;
    }
}
