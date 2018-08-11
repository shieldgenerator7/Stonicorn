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
    public float forcedMoveOutDuration = 1.0f;//how many seconds it has to move out before moving back in
    private float forcedMoveOutStartTime = 0;//the time at which it began

    private SpriteRenderer sr;

    // Use this for initialization
    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    public void showRelativeTo(GameObject currentCP)
    {
        this.epicenter = GameManager.getPlayerObject().transform.position;
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
    }

    internal void readjustPosition(Vector2 epicenter)
    {
        this.epicenter = epicenter;
        //Activate Object
        enabled = true;
        gameObject.SetActive(true);
        //Setup movement variables
        startPos = epicenter;
        moveDir = ((Vector2)parentCPC.gameObject.transform.position - epicenter).normalized;
        sqrDistanceFromCurrentCP = (epicenter - (Vector2)parentCPC.gameObject.transform.position).sqrMagnitude;
        moveOutSpeed = 0.1f;
        forcedMoveOutStartTime = Time.time;
    }

    //Update is called once per frame
    void Update()
    {
        //Go out until there are no conflicts
        if (moveOutSpeed > 0 || Time.time < forcedMoveOutStartTime + forcedMoveOutDuration)
        {
            if (distanceOut < MIN_DISTANCE)
            {
                distanceOut += moveOutSpeed;
                moveOutSpeed += moveOutAccel;
                transform.position = startPos + (moveDir * (distanceOut));
                return;
            }
            //check to make sure its ghost does not intersect other CP ghosts
            if (overlapAny() || Time.time < forcedMoveOutStartTime + forcedMoveOutDuration)
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
        foreach (CheckPointChecker cpc in GameManager.getActiveCheckPoints())
        {
            if (cpc != parentCPC)
            {
                GameObject go = cpc.gameObject;
                if (cpc.activated && cpc.cpGhostMover.gameObject.activeInHierarchy)
                {
                    if (cpc.cpGhostMover.overlaps(sr.bounds, spriteRadius))
                    {
                        if (sqrDistanceFromCurrentCP > cpc.cpGhostMover.sqrDistanceFromCurrentCP)
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
        && (sr.bounds.center - otherBounds.center).sqrMagnitude <= Mathf.Pow(spriteRadius + otherRadius, 2);
    }
}
