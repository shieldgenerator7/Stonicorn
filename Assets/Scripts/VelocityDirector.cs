using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VelocityDirector : MonoBehaviour
{
    private BoxCollider2D bc2d;
    private EdgeCollider2D ec2d;

    private void Start()
    {
        //BoxCollider2D
        bc2d = GetComponent<BoxCollider2D>();
        if (bc2d && !bc2d.isTrigger)
        {
            throw new UnityException("VelocityDirector requires BoxCollider2D on " + name + " to be a trigger!");
        }
        //EdgeCollider2D
        ec2d = GetComponent<EdgeCollider2D>();
        if (ec2d && !ec2d.isTrigger)
        {
            throw new UnityException("VelocityDirector requires EdgeCollider2D on " + name + " to be a trigger!");
        }
    }

    private void OnTriggerEnter2D(Collider2D coll)
    {
        if (bc2d)
        {
            Rigidbody2D collRB2D = coll.gameObject.GetComponent<Rigidbody2D>();
            if (collRB2D)
            {
                directVelocity(collRB2D, transform.right);
            }
        }
    }

    private void OnTriggerStay2D(Collider2D coll)
    {
        if (ec2d)
        {
            Rigidbody2D collRB2D = coll.gameObject.GetComponent<Rigidbody2D>();
            if (collRB2D)
            {
                Vector2 curpos = coll.gameObject.transform.position;//"current position"
                //Find out which segment the object is in
                float minSqrDistance = float.MaxValue;
                int closestPoint = 0;
                for (int i = 0; i < ec2d.pointCount; i++)
                {
                    Vector2 point = transform.TransformPoint(ec2d.points[i]);
                    float sqrDistance = (point - curpos).sqrMagnitude;
                    if (sqrDistance <= minSqrDistance)
                    {
                        minSqrDistance = sqrDistance;
                        closestPoint = i;
                    }
                }
                //Find out the direction of the segment
                Vector2 vectorBefore = (closestPoint > 0) ? ec2d.points[closestPoint] - ec2d.points[closestPoint - 1] : Vector2.zero;
                Vector2 vectorAfter = (closestPoint < ec2d.pointCount - 1) ? ec2d.points[closestPoint + 1] - ec2d.points[closestPoint] : Vector2.zero;
                if (vectorBefore == Vector2.zero)
                {
                    vectorBefore = vectorAfter;
                }
                if (vectorAfter == Vector2.zero)
                {
                    vectorAfter = vectorBefore;
                }
                Vector2 direction = (vectorBefore + vectorAfter).normalized;
                direction = transform.TransformDirection(direction);
                direction = direction.normalized;
                //Direct the object's velocity
                directVelocity(collRB2D, direction);
            }
        }
    }

    public void directVelocity(Rigidbody2D rb2d, Vector2 dirRight)
    {
        Vector2 velocity = rb2d.velocity;
        float angleRight = Vector2.Angle(velocity, dirRight);
        float angleLeft = Vector2.Angle(velocity, -dirRight);
        float direction = 0;
        if (angleRight < angleLeft)
        {
            direction = 1;
        }
        if (angleLeft < angleRight)
        {
            direction = -1;
        }
        rb2d.velocity = dirRight * direction * velocity.magnitude;
    }
}
