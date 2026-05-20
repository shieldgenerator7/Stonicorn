using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

    //TODO: burst this script so its more efficient
[NonSolid]
public class WaterArea : MonoBehaviour
{
    public float minSpeed;//the minimum speed in order to apply dampen
    public float maxSpeed;//the maximum speed allowed underwater
    public float slowDuration;//how long it takes the water to slow something from maxSpeed to minSpeed

    [AutoInitialize, SerializeField, HideInInspector]
    private Collider2D coll2d;

    [SerializeField, HideInInspector]
    private List<Rigidbody2D> tenants = new List<Rigidbody2D>();

    [SerializeField, HideInInspector]
    private float minSquared;

    private void FixedUpdate()
    {
                tenants.ForEach(rb2d =>
                {
                    float speedSquared = rb2d.linearVelocity.sqrMagnitude;
                    if (speedSquared >= minSquared)
                    {
                        Vector2 dir = rb2d.linearVelocity.normalized;
                        float speed = Mathf.Sqrt(speedSquared);
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
                });
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Rigidbody2D rb2d = collision.GetComponent<Rigidbody2D>();
        if (rb2d)
        {
            if (!tenants.Contains(rb2d))
            {
                tenants.Add(rb2d);
            }
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        Rigidbody2D rb2d = collision.GetComponent<Rigidbody2D>();
        if (rb2d)
        {
            tenants.Remove(rb2d);
        }
    }

    [Initializer]
    private float init_minSquared => minSpeed * minSpeed;

    [Initializer]
    private List<Rigidbody2D> init_tenants()
    {
        List<Rigidbody2D> list = new List<Rigidbody2D>();
        Utility.RaycastAnswer rca = coll2d.CastAnswer(Vector2.zero, 0, true);
        for (int i = 0; i < rca.count; i++)
        {
            RaycastHit2D rch2d = rca.rch2ds[i];
            if (!rch2d.collider.isTrigger)
            {
                Rigidbody2D rb2d = rch2d.collider.GetComponent<Rigidbody2D>();
                if (rb2d)
                {
                    if (!list.Contains(rb2d))
                    {
                        list.Add(rb2d);
                    }
                }
            }
        }
        return list.OrderBy(rb2d=>rb2d.GetInstanceID()).ToList();
    }
}
