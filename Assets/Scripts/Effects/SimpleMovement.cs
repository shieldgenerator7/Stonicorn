﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleMovement : MonoBehaviour
{

    //Settings
    [Tooltip("Direction in Local Coordinates")]
    public Vector2 direction;
    public float duration;//in seconds
    public float endDelay;//delay after reaching the end before resetting to the beginning
    public bool roundTrip = false;//true: move backwards instead of jumping to start pos

    protected Vector2 startPosition;

    //Runtime constants
    protected float speed;
    protected Vector2 endPosition;
    //Runtime vars
    protected float lastKeyFrame;
    protected bool forwards = true;//true to return back to start
    protected bool paused = false;

    // Use this for initialization
    protected virtual void Start()
    {
        setMovement(
            transform.position,
            transform.TransformDirection(this.direction),
            this.direction.magnitude,
            this.direction.magnitude,
            false,
            true
            );
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
                if (Vector2.Distance(transform.position, startPosition) >= direction.magnitude
                    || (Vector2)transform.position == endPosition)
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
                if (Vector2.Distance(transform.position, endPosition) >= direction.magnitude
                    || (Vector2)transform.position == startPosition)
                {
                    paused = true;
                    lastKeyFrame = lastKeyFrame + duration;
                }
            }
        }
    }

    public virtual void setMovement(Vector2 start, Vector2 dir, float minDist = 0, float maxDist = 1, bool keepPercent = true, bool updateSpeed = false)
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
                (Time.time - lastKeyFrame) / duration;
        }
        startPosition = start;
        endPosition = startPosition + this.direction;
        if (percentThrough == 0)
        {
            transform.position = startPosition;
        }
        else
        {
            transform.position = startPosition + (this.direction * percentThrough);
        }
        if (updateSpeed)
        {
            speed = (endPosition - startPosition).magnitude / duration;
        }
    }

    public void setMovementEnd(Vector2 end, Vector2 dir)
    {
        dir = dir.normalized * this.direction.magnitude;
        setMovement(end - dir, dir);
    }
}
