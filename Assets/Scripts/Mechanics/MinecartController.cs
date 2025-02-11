using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class MinecartController : MonoBehaviour
{
    public float wheelRadius = 1;

    [AutoInitialize(SearchChildren =true), SerializeField,HideInInspector]
    private List<SimpleRotation> rotators;
    [AutoInitialize,SerializeField,HideInInspector]
    private Rigidbody2D rb2d;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    // Update is called once per frame
    void LateUpdate()
    {
        float speed = rb2d.linearVelocity.magnitude * 2 * wheelRadius * wheelRadius * Mathf.PI * 360
            * -Mathf.Sign(transform.InverseTransformDirection(rb2d.linearVelocity).x);
        rotators.ForEach(r=>r.turnSpeed = speed);
    }
}
