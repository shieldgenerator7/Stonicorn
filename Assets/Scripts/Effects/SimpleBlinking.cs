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
    public float timeOffset = 0;//offset the timing of the cycle (sec)

    public enum BlinkState
    {
        ON,//shown
        OFF//hidden
    }
    public BlinkState blinkState;

    private float lastKeyFrame = 0;//the last time it was switched
    private float currentDuration = 0;//the current span between keyframes
    private float currentTransitionDuration = 0;//the current time it takes to transition to current state

    void OnEnable()
    {
        lastKeyFrame = Time.time + timeOffset;
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time > lastKeyFrame + currentDuration)
        {
            switch (blinkState)
            {
                case BlinkState.ON:
                    blinkState = BlinkState.OFF;
                    currentDuration = offTime;
                    currentTransitionDuration = onOffTransition;
                    if (currentTransitionDuration == 0)
                    {
                        updateAlpha();
                    }
                    break;
                case BlinkState.OFF:
                    blinkState = BlinkState.ON;
                    currentDuration = onTime;
                    currentTransitionDuration = offOnTransition;
                    if (currentTransitionDuration == 0)
                    {
                        updateAlpha();
                    }
                    break;
            }
            lastKeyFrame = lastKeyFrame + currentDuration;
        }
        //Transitions
        if (currentTransitionDuration > 0)
        {
            updateAlpha(
                Mathf.Min(
                    1,
                    (Time.time - lastKeyFrame) / (currentTransitionDuration)
                )
            );
        }
    }

    /// <summary>
    /// Updates the sprite alpha to be off or on, 
    /// or somewhere in between
    /// </summary>
    /// <param name="percent">The percent towards the current state. 1 for all the way, 0 for none of the way. NOTE: 1 does not always mean opaque, 0 does not always mean transparent.</param>
    void updateAlpha(float percent = 1)
    {
        float newAlpha = (blinkState == BlinkState.ON) ? 0 + percent : 1 - percent;
        foreach (SpriteRenderer tsr in GetComponentsInChildren<SpriteRenderer>())
        {
            Color c = tsr.color;
            c.a = newAlpha;
            tsr.color = c;
        }
    }
}
