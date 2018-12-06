using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DirectedMovement : SimpleMovement
{

    public GameObject target;//the game object to point to
    public string actorTag = "MainCamera";//the tag on the actor to identify it
    public string actorName = "Main Camera";//the name of the actor to identify it
    private GameObject actor;//the game object that this object follows

    private Vector2 prevPos;

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
        Debug.Log("Actor name: " + actor.name);
        //Make sure direction is valid
        if (direction == Vector2.zero)
        {
            direction = Vector2.one;
        }
        //Set initial movement vector
        setMovement((Vector2)actor.transform.position, (Vector2)actor.transform.position - (Vector2)target.transform.position, false, true);
    }

    // Update is called once per frame
    protected override void Update()
    {
        if ((Vector2)actor.transform.position != prevPos)
        {
            prevPos = actor.transform.position;
            setMovement(actor.transform.position, actor.transform.position - target.transform.position);
        }
        base.Update();
    }
}
