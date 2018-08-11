using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleScaling : MonoBehaviour
{//2018-08-11: copied from SimpleMovement
    
    //Settings
    public float scale = 2f;//the scale to transition to
    public float duration = 0.4f;//in seconds
    public float endDelay = 0.2f;//delay after reaching the end before resetting to the beginning

    private Vector3 startScale;

    //Runtime vars
    private float speed;
    private Vector3 endScale;
    private float endDelayStartTime = 0;
    private bool forwards = true;//true to return back to start

    // Use this for initialization
    void Start()
    {
        startScale = transform.localScale;
        endScale = startScale * scale;
        speed = (endScale - startScale).magnitude / duration;
    }

    // Update is called once per frame
    void Update()
    {
        if (endDelayStartTime != 0)
        {
            if (Time.time > endDelayStartTime + endDelay)
            {
                endDelayStartTime = 0;
                forwards = !forwards;
            }
        }
        else
        {
            if (forwards)
            {
                transform.localScale = Vector3.MoveTowards(
                    transform.localScale,
                    endScale,
                    speed * Time.deltaTime
                    );
                if (transform.localScale == endScale)
                {
                    endDelayStartTime = Time.time;
                }
            }
            else
            {
                transform.localScale = Vector3.MoveTowards(
                    transform.localScale,
                    startScale,
                    speed * Time.deltaTime
                    );
                if (transform.localScale == startScale)
                {
                    endDelayStartTime = Time.time;
                }
            }
        }
    }
}
