using System.Runtime.ConstrainedExecution;
using UnityEngine;

[RequireComponent(typeof(GravityAccepter))]
[RequireComponent(typeof(Rigidbody2D))]
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
        Vector2 center = Vector2.zero;// gravityAccepter.Center.position //TODO: use gravityAccepter center
        Vector2 gravityVector = center - (Vector2)transform.position;
        Vector2 sideVector = new Vector3(-gravityVector.y, gravityVector.x) / Mathf.Sqrt(gravityVector.x * gravityVector.x + gravityVector.y * gravityVector.y);
        rb2d.velocity = sideVector.normalized * speed;
        transform.up = -gravityVector;
    }
}
