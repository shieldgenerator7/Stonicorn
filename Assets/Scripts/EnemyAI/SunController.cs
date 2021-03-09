using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SunController : Hazard
{
    public Vector2 targetPos = Vector2.zero;
    public float travelTimeToTarget = 360;

    private float speed;
    private Vector2 direction;

    private void Start()
    {
        init();
    }

    public override void init()
    {
        direction = targetPos - (Vector2)transform.position;
        speed = direction.magnitude / travelTimeToTarget;
        GetComponent<Rigidbody2D>().velocity = direction.normalized * speed;
    }
}
