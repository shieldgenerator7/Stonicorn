﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GravityZone : MonoBehaviour
{
    public float gravityScale = 9.81f;
    public bool mainGravityZone = true;//true to change camera angle, false to not
    public bool radialGravity = true;//true to make it gravitate towards the center of the gravity zone
    
    private Vector2 gravityVector;
    private List<Rigidbody2D> tenants = new List<Rigidbody2D>();//the list of colliders in this zone

    private Collider2D coll2d;

    // Use this for initialization
    void Start()
    {
        gravityVector = -transform.up.normalized * gravityScale;
        coll2d = GetComponent<Collider2D>();
    }

    void OnTriggerEnter2D(Collider2D coll)
    {
        if (coll.isSolid())
        {
            Rigidbody2D rb2d = coll.GetComponent<Rigidbody2D>();
            if (rb2d)
            {
                if (!tenants.Contains(rb2d))
                {
                    tenants.Add(rb2d);
                }
            }
        }
    }
    void OnTriggerExit2D(Collider2D coll)
    {
        if (coll.isSolid())
        {
            Rigidbody2D rb2d = coll.GetComponent<Rigidbody2D>();
            if (rb2d)
            {
                tenants.Remove(rb2d);
            }
        }
    }

    void FixedUpdate()
    {
        tenants.RemoveAll(ten => !ten || ReferenceEquals(ten, null));
        foreach (Rigidbody2D rb2d in tenants)
        {
            Vector2 finalGravityVector = (radialGravity)
                ? (Vector2)(transform.position - rb2d.transform.position).normalized
                    * gravityScale
                : gravityVector;
            Vector3 vector = finalGravityVector * rb2d.mass;
            GravityAccepter ga = rb2d.GetComponent<GravityAccepter>();
            if (ga)
            {
                if (ga.AcceptsGravity)
                {
                    vector *= ga.gravityScale;
                    rb2d.AddForce(vector);
                    //Inform the gravity accepter of the direction
                    ga.addGravity(finalGravityVector);
                }
            }
            else
            {
                rb2d.AddForce(vector);
            }
        }
    }

    {
        {
        }
    }
}
