using System.Runtime.ConstrainedExecution;
using UnityEngine;

[RequireComponent(typeof(GravityAccepter))]
[RequireComponent (typeof(Rigidbody2D))]
public class CloudMover : MonoBehaviour
{
    public float speed = 0.02f;

    GravityAccepter gravityAccepter;

    Rigidbody2D rb2d;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gravityAccepter = GetComponent<GravityAccepter>();
        rb2d = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        rb2d.velocity = gravityAccepter.SideVector * speed;
    }
}
