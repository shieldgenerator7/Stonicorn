using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleMovementUI : SimpleMovement
{
    // Use this for initialization
    protected override void Start()
    {
        setMovement(transform.localPosition, this.direction, this.direction.magnitude, this.direction.magnitude, false, true);
    }
    void OnEnable()
    {
        lastKeyFrame = Time.unscaledTime;
        forwards = true;
        paused = false;
    }

    // Update is called once per frame
    protected override void Update()
    {
        //endPosition = startPosition + this.direction;
        //speed = (endPosition - startPosition).magnitude / duration;

        if (paused)
        {
            if (Time.unscaledTime > lastKeyFrame + endDelay)
            {
                paused = false;
                lastKeyFrame = lastKeyFrame + endDelay;
                if (roundTrip)
                {
                    forwards = !forwards;
                }
                else
                {
                    transform.localPosition = startPosition;
                }
            }
        }
        else
        {
            if (forwards)
            {
                transform.localPosition = Vector2.MoveTowards(
                    transform.localPosition,
                    endPosition,
                    speed * Time.unscaledDeltaTime
                    );
                if ((Vector2)transform.localPosition == endPosition)
                {
                    paused = true;
                    lastKeyFrame = lastKeyFrame + duration;
                }
            }
            else
            {
                transform.localPosition = Vector2.MoveTowards(
                    transform.localPosition,
                    startPosition,
                    speed * Time.unscaledDeltaTime
                    );
                if ((Vector2)transform.localPosition == startPosition)
                {
                    paused = true;
                    lastKeyFrame = lastKeyFrame + duration;
                }
            }
        }
    }

    public override void setMovement(Vector2 start, Vector2 dir, float minDist = 0, float maxDist = 1, bool keepPercent = true, bool updateSpeed = false)
    {
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
        if (this.direction.magnitude > maxDist)
        {
            this.direction = this.direction.normalized * maxDist;
        }
        if (this.direction.magnitude < minDist)
        {
            this.direction = this.direction.normalized * minDist;
        }
        float percentThrough = 0;
        if (keepPercent)
        {
            percentThrough =
                (Time.unscaledDeltaTime - lastKeyFrame) / duration;
        }
        startPosition = start;
        endPosition = startPosition + this.direction;
        if (percentThrough == 0)
        {
            transform.localPosition = startPosition;
        }
        else
        {
            transform.localPosition = startPosition + (this.direction * percentThrough);
        }
        if (updateSpeed)
        {
            speed = (endPosition - startPosition).magnitude / duration;
        }
    }
}
