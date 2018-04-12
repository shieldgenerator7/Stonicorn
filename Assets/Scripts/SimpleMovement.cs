using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleMovement : MonoBehaviour
{

    //Settings
    public Vector2 direction;
    public float duration;//in seconds
    public float endDelay;//delay after reaching the end before resetting to the beginning
    
    private Vector2 startPosition;

    //Runtime vars
    private float speed;
    private Vector2 endPosition;
    private float endDelayStartTime = 0;

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
        if (endDelayStartTime != 0)
        {
            if (Time.time > endDelayStartTime + endDelay)
            {
                transform.position = startPosition;
                endDelayStartTime = 0;
            }
        }
        else
        {
            transform.position = Vector2.MoveTowards(
                transform.position,
                endPosition,
                speed * Time.deltaTime
                );
            if (((Vector2)transform.position - endPosition).sqrMagnitude == 0)
            {
                endDelayStartTime = Time.time;
            }
        }
    }
}
