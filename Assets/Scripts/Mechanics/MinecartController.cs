using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class MinecartController : MonoBehaviour
{
    public float wheelRadius = 1;

    public List<GameObject> wheels;

    private List<SimpleRotation> rotators;
    private Rigidbody2D rb2d;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rotators = wheels.ConvertAll(wheel=>wheel.GetComponent<SimpleRotation>());
        rb2d = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        float speed = rb2d.linearVelocity.magnitude * 2 * wheelRadius * wheelRadius * Mathf.PI * 360
            * -Mathf.Sign(transform.InverseTransformDirection(rb2d.linearVelocity).x);
        rotators.ForEach(r=>r.turnSpeed = speed);
    }
}
