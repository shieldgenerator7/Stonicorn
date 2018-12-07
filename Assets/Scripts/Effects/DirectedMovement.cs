using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DirectedMovement : SimpleMovement
{
    public float minDistance = 0;//the min distance the pointer should move
    public float maxDistance = 1;//the max distance the pointer should move
    public GameObject target;//the game object to point to
    public string actorTag = "MainCamera";//the tag on the actor to identify it
    public string actorName = "Main Camera";//the name of the actor to identify it
    private GameObject actor;//the game object that this object follows
    public bool startFromActor = false;//true to start from the actor, false to start from the target
    public bool pointAway = false;//true to point from anchor away from the goal, false to point towards the goal

    private Vector2 prevPosAnchor;
    private Vector2 prevPosGoal;
    private GameObject anchorObject;
    private GameObject goalObject;

    // Use this for initialization
    protected override void Start()
    {
        //Find actor (done this way to allow it to work across scenes)
        foreach (GameObject go in GameObject.FindGameObjectsWithTag(actorTag))
        {
            if (go.name == actorName)
            {
                actor = go;
                break;
            }
        }
        //Set anchor object
        anchorObject = (startFromActor) ? actor : target;
        //Set goal object
        goalObject = (!startFromActor) ? actor : target;
        //Make sure direction is valid
        if (direction == Vector2.zero)
        {
            direction = Vector2.one;
        }
        //Set initial movement vector
        setMovement((Vector2)anchorObject.transform.position, getDirection(), minDistance, maxDistance, false, true);
    }

    // Update is called once per frame
    protected override void Update()
    {
        if ((Vector2)anchorObject.transform.position != prevPosAnchor
            || (Vector2)goalObject.transform.position != prevPosGoal)
        {
            prevPosAnchor = anchorObject.transform.position;
            prevPosGoal = goalObject.transform.position;
            setMovement(anchorObject.transform.position, getDirection(), minDistance, maxDistance);
        }
        base.Update();
    }

    Vector2 getDirection()
    {
        return (
            (Vector2)goalObject.transform.position - (Vector2)anchorObject.transform.position
            )
            * ((pointAway) ? -1 : 1)
            ;
    }
}
