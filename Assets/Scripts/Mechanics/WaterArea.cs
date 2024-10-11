using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterArea : MonoBehaviour
{
    public float minSpeed;//the minimum speed in order to apply dampen
    public float maxSpeed;//the maximum speed allowed underwater
    public float slowDuration;//how long it takes the water to slow something from maxSpeed to minSpeed

    private Collider2D coll2d;

    private void Start()
    {
        coll2d = GetComponent<Collider2D>();
    }

    private void FixedUpdate()
    {
        Utility.RaycastAnswer rca = coll2d.CastAnswer(Vector2.zero, 0, true);
        for (int i = 0; i < rca.count; i++)
        {
            RaycastHit2D rch2d = rca.rch2ds[i];
            if (!rch2d.collider.isTrigger)
            {
                Rigidbody2D rb2d = rch2d.collider.GetComponent<Rigidbody2D>();
                if (rb2d)
                {
                    float speed = rb2d.linearVelocity.magnitude;
                    if (speed >= minSpeed)
                    {
                        Vector2 dir = rb2d.linearVelocity.normalized;
                        if (speed > maxSpeed)
                        {
                            rb2d.linearVelocity = dir * maxSpeed;
                        }
                        float durationLeft = (speed - minSpeed) / (maxSpeed - minSpeed);
                        durationLeft = Mathf.Max(durationLeft, 1);
                        rb2d.linearVelocity = Vector2.Lerp(
                            rb2d.linearVelocity,
                            dir * minSpeed,
                            Time.deltaTime / durationLeft
                            );
                    }
                }
            }
        }
    }
}
