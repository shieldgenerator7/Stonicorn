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
    protected virtual void Start()
    {
        setMovement(transform.position, this.direction, false, true);
    }
    void OnEnable()
    {
        lastKeyFrame = Time.time;
        forwards = true;
        paused = false;
    }

    // Update is called once per frame
    protected virtual void Update()
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

    public void setMovement(Vector2 start, Vector2 dir, bool keepPercent = true, bool updateSpeed = false)
    {
        Debug.Log("start: " + start + ", dir: " + dir + ", direction: " + direction);
        if (!updateSpeed)
        {
            //Make sure direction is valid
            if (direction == Vector2.zero)
            {
                direction = Vector2.one;
            }
            //Trim dir to size
            dir = dir.normalized * this.direction.magnitude;
        }
        this.direction = dir;
        float percentThrough = 0;
        if (keepPercent)
        {
            percentThrough =
                Vector3.Distance(transform.position, startPosition)
                / Vector3.Distance(endPosition, startPosition);
        }
        startPosition = start;
        endPosition = startPosition + dir;
        if (percentThrough == 0)
        {
            transform.position = startPosition;
        }
        else
        {
            transform.position = startPosition + (dir.normalized * percentThrough);
        }
        if (updateSpeed)
        {
            speed = (endPosition - startPosition).magnitude / duration;
        }
        Debug.Log("startpos: " + startPosition + ", endpos: " + endPosition+", speed: "+speed+", duration: "+duration);
    }

    public void setMovementEnd(Vector2 end, Vector2 dir)
    {
        dir = dir.normalized * this.direction.magnitude;
        setMovement(end - dir, dir);
    }
}
