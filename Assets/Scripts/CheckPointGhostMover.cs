﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPointGhostMover : MonoBehaviour
{
    public static float MIN_DISTANCE = 4.5f;//how much distanceOut must be before it can stop moving.

    public float sqrDistanceFromCurrentCP;//how its CheckPoint is from the current CheckPoint, used to determine how far out this ghost must go
    public CheckPointChecker parentCPC;
    private Vector2 startPos;//the position to start from
    private Vector2 moveDir;//the direction to move
    private float distanceOut = 0;//how far from the current CP this ghost is
    private float moveOutSpeed = 0.1f;
    public float moveOutAccel = 0.05f;
    private float spriteRadius = 0;//the radius of the circular sprite
    private Vector2 epicenter;//the center to revolve around

    //Readjust position
    private Vector2 targetPos;
    private bool outOfLine = false;//true when they're not in their lane after Merky moves
    private Vector2 adjustDir;//the direction it moves to adjust position
    private float adjustDistanceOut;
    private float accelDir = 1;//which direction to accelerate in
    private Vector2 relativeSigns;//used to determine when the ghost moved past its target

    //Going home
    private bool goingHome = false;//true when about to be hidden

    private SpriteRenderer sr;

    // Use this for initialization
    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    public void showRelativeTo(GameObject currentCP)
    {
        this.epicenter = Managers.Player.position;
        //Activate Object
        enabled = true;
        gameObject.SetActive(true);
        //Get sprite radius
        transform.localEulerAngles = Vector3.zero;
        spriteRadius = sr.bounds.extents.x;
        //Rotate sprite
        transform.localRotation = currentCP.transform.localRotation;
        //Set sprite to start position
        startPos = epicenter;
        transform.position = startPos;
        //Setup movement variables
        moveDir = ((Vector2)parentCPC.gameObject.transform.position - epicenter).normalized;
        sqrDistanceFromCurrentCP = (epicenter - (Vector2)parentCPC.gameObject.transform.position).sqrMagnitude;
        distanceOut = 0;
        moveOutSpeed = 0.1f;
        outOfLine = false;
        goingHome = false;
    }

    internal void snapToNewPosition(Vector2 epicenter)
    {
        Vector2 offset = (Vector2)transform.position - this.epicenter;
        this.epicenter = epicenter;
        transform.position = epicenter + offset;
        //If it's not on the initial move out step,
        if (moveOutSpeed <= 0)
        {
            //Stop updating the position
            enabled = false;
        }
    }

    internal void readjustPosition(Vector2 epicenter)
    {
        this.epicenter = epicenter;
        //Activate Object
        enabled = true;
        //Setup movement variables
        startPos = epicenter;
        moveDir = ((Vector2)parentCPC.gameObject.transform.position - epicenter).normalized;
        sqrDistanceFromCurrentCP = (epicenter - (Vector2)parentCPC.gameObject.transform.position).sqrMagnitude;
        moveOutSpeed = 0.4f;
        //Set target
        targetPos = (moveDir * distanceOut) + epicenter;
        adjustDir = ((Vector2)transform.position - targetPos).normalized;
        adjustDistanceOut = Vector2.Distance(targetPos, transform.position);
        relativeSigns = targetPos - (Vector2)transform.position;
        accelDir = -1;
        outOfLine = true;
        goingHome = false;
    }

    /// <summary>
    /// Sets the checkpoint ghosts to go to the epicenter and disappear
    /// </summary>
    public void goHome()
    {
        this.epicenter = CheckPointChecker.current.transform.position;
        enabled = true;
        startPos = epicenter;
        moveOutSpeed = 0.1f;
        outOfLine = false;
        goingHome = true;
    }

    //Update is called once per frame
    void Update()
    {
        //If needs to adjust to Merky repositioning
        if (outOfLine)
        {
            adjustDistanceOut += moveOutSpeed;
            moveOutSpeed += moveOutAccel * accelDir;
            transform.position = targetPos + (adjustDir * adjustDistanceOut);
            if (Mathf.Sign(relativeSigns.x) != Mathf.Sign(targetPos.x - transform.position.x)
                || Mathf.Sign(relativeSigns.y) != Mathf.Sign(targetPos.y - transform.position.y))
            {
                outOfLine = false;
            }
            return;
        }
        else if (goingHome)
        {
            moveOutSpeed += moveOutAccel;
            transform.position = Vector3.MoveTowards(transform.position, epicenter, moveOutSpeed);
            if ((Vector2)transform.position == epicenter)
            {
                goingHome = false;
                gameObject.SetActive(false);
            }
            return;
        }
        //Go out until there are no conflicts
        if (moveOutSpeed > 0)
        {
            if (distanceOut < MIN_DISTANCE)
            {
                distanceOut += moveOutSpeed;
                moveOutSpeed += moveOutAccel;
                transform.position = startPos + (moveDir * (distanceOut));
                return;
            }
            //check to make sure its ghost does not intersect other CP ghosts
            if (overlapAny())
            {
                distanceOut += moveOutSpeed;
                moveOutSpeed += moveOutAccel;
                transform.position = startPos + (moveDir * (distanceOut));
            }
            else
            {
                moveOutSpeed = -0.1f;
            }
        }
        //Then come back slowly until you reach a conflict
        else
        {
            distanceOut += moveOutSpeed;
            transform.position = startPos + (moveDir * (distanceOut));
            if (distanceOut <= MIN_DISTANCE || overlapAny())
            {
                //Undo the last movement to land at the "perfect spot"
                distanceOut -= moveOutSpeed;
                transform.position = startPos + (moveDir * (distanceOut));
                enabled = false;
            }
        }
    }

    /// <summary>
    /// Returns true if it overlaps any other CP ghost
    /// And the other one is closer to the current CP than this one is
    /// </summary>
    /// <returns></returns>
    public bool overlapAny()
    {
        foreach (CheckPointChecker cpc in Managers.ActiveCheckPoints)
        {
            if (cpc != parentCPC)
            {
                GameObject go = cpc.gameObject;
                if (cpc.Discovered && cpc.GhostMover.gameObject.activeInHierarchy)
                {
                    if (cpc.GhostMover.overlaps(sr.bounds, spriteRadius))
                    {
                        if (sqrDistanceFromCurrentCP > cpc.GhostMover.sqrDistanceFromCurrentCP)
                        {
                            return true;
                        }
                    }
                }
            }
        }
        return false;
    }
    /// <summary>
    /// Determines if this CP ghost overlaps the given bounds
    /// </summary>
    /// <param name="b"></param>
    /// <returns></returns>
    public bool overlaps(Bounds otherBounds, float otherRadius)
    {
        return sr.bounds.Intersects(otherBounds)
        //because they're circles, using the extents gives us how far apart (at minimum) they're supposed to be 
        && sr.bounds.center.inRange(otherBounds.center, spriteRadius + otherRadius);
    }
}
