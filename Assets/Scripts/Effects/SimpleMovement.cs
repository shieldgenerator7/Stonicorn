using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleMovement : MonoBehaviour
{

    //Settings
    public Vector2 direction;
    public float duration;//in seconds
    public float endDelay;//delay after reaching the end before resetting to the beginning
    public bool roundTrip = false;//true: move backwards instead of jumping to start pos
    
    private Vector2 startPosition;

    //Runtime constants
    private float speed;
    private Vector2 endPosition;
    //Runtime vars
    private float lastKeyFrame;
    private bool forwards = true;//true to return back to start
    private bool paused = false;

    // Use this for initialization
    void Start()
    {
        startPosition = transform.position;
        endPosition = startPosition + direction;
        speed = (endPosition - startPosition).magnitude / duration;
    }

    // Update is called once per frame
    void Update()
    {
        if (paused)
        {
            if (Time.time > lastKeyFrame + endDelay)
            {
                paused = false;
                lastKeyFrame = lastKeyFrame + endDelay;
                if (roundTrip)
                {
                    forwards = !forwards;
                }
                else
                {
                    transform.position = startPosition;
                }
            }
        }
        else
        {
            if (forwards)
            {
                transform.position = Vector2.MoveTowards(
                    transform.position,
                    endPosition,
                    speed * Time.deltaTime
                    );
                if ((Vector2)transform.position == endPosition)
                {
                    paused = true;
                    lastKeyFrame = lastKeyFrame + duration;
                }
            }
            else
            {
                transform.position = Vector2.MoveTowards(
                    transform.position,
                    startPosition,
                    speed * Time.deltaTime
                    );
                if ((Vector2)transform.position == startPosition)
                {
                    paused = true;
                    lastKeyFrame = lastKeyFrame + duration;
                }
            }
        }
    }
}
