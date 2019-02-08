using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnailController : MonoBehaviour
{
    [Header("Settings")]
    public float moveSpeed;
    public float rotateSpeed;
    public float stickForce = 9.81f;//how much force it uses to stick to walls
    public float acceptNewWallAngleThreshold = 170;

    //Runtime Vars
    private Vector2 lastSeenStonePosition;//where stone was last seen
    public Vector2 floorDirection;//points away from the floor
    public Vector2 floorRight;//if floorDirection is Vector2.up, floorRight is Vector2.right

    [Header("Components")]
    public Animator animator;
    public Collider2D bottomDetector;//used to make sure the snail is at the right orientation before coming out
    public Collider2D stoneDetector;//used to detect stone in its area
    private Rigidbody2D rb2d;

    // Start is called before the first frame update
    void Start()
    {
        animator.SetBool("scared", true);
        rb2d = GetComponentInChildren<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        rb2d.angularVelocity = rotateSpeed;
        //Own gravity
        rb2d.AddForce(-floorDirection * rb2d.mass * stickForce);
        Debug.DrawLine(transform.position, (Vector2)transform.position + floorDirection, Color.blue);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Vector2 newFD = collision.contacts[0].normal;
        float newRot = Utility.RotationZ(rb2d.velocity, newFD);
        float curRot = Utility.RotationZ(rb2d.velocity, floorDirection);
        if (floorDirection == Vector2.zero
            || (rotateSpeed > 0 && newRot < curRot)
            || (rotateSpeed < 0 && newRot > curRot))
        {
            floorDirection = newFD;
            floorRight = Utility.RotateZ(floorDirection, -90);
        }
    }
}
