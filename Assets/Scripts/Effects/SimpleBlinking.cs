using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Cycles an object between hidden and shown, using sprite alpha.
/// </summary>
public class SimpleBlinking : MonoBehaviour {

    public float onTime = 1;//how long to be shown
    public float offTime = 1;//how long to be hidden

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
	void Start () {
        sr = GetComponent<SpriteRenderer>();
	}
	
	// Update is called once per frame
	void Update () {
        if (Time.time > lastKeyFrame + currentDuration)
        {
            lastKeyFrame = Time.time;
            switch (blinkState)
            {
                case BlinkState.ON:
                    blinkState = BlinkState.OFF;
                    currentDuration = offTime;
                    break;
                case BlinkState.OFF:
                    blinkState = BlinkState.ON;
                    currentDuration = onTime;
                    break;
            }
            updateAlpha();
        }
	}

    void updateAlpha()
    {
        Color c = sr.color;
        c.a = (blinkState == BlinkState.ON) ? 1 : 0;
        sr.color = c;
    }
}
