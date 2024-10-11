using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SunController : Hazard
{
    public Vector2 targetPos = Vector2.zero;
    public float travelTimeToTarget = 360;

    private float speed;
    private Vector2 direction;

    public override void init()
    {
        direction = targetPos - (Vector2)transform.position;
        speed = direction.magnitude / travelTimeToTarget;
        GetComponent<Rigidbody2D>().linearVelocity = direction.normalized * speed;
    }

    private void Update()
    {
        Managers.ActiveCheckPoints.ForEach(cp =>
        {
            bool inSun = cp.transform.position.y - 2 < transform.position.y;
            if (cp.InWorkingOrder == inSun)
            {
                cp.InWorkingOrder = !inSun;
            }
        });
    }
}
