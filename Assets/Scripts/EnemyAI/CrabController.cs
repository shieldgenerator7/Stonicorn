using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrabController : Hazard
{
    [Header("Settings")]
    public float moveSpeed = 1;
    [Header("Animators")]
    public Animator legAnimator;
    [Header("Components")]
    public Collider2D cliffDetector;
    public Collider2D obstacleDetector;

    private Rigidbody2D rb2d;

    //
    // Properties
    //

    public override bool Hazardous => false;

    public bool CliffDetected
        => Utility.CastCountSolid(cliffDetector, Vector2.zero) == 0;

    public bool ObstacleDetected
        => Utility.CastCountSolid(obstacleDetector, Vector2.zero) > 0;

    // Start is called before the first frame update
    void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        if (rb2d.velocity.sqrMagnitude < moveSpeed * moveSpeed)
        {
            rb2d.AddForce(
                transform.right
                * Mathf.Sign(transform.localScale.x)
                * rb2d.mass
                * moveSpeed
                );
            if (rb2d.velocity.sqrMagnitude > moveSpeed * moveSpeed)
            {
                rb2d.velocity = rb2d.velocity.normalized * moveSpeed;
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (ObstacleDetected)
        {
            changeDirection();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (CliffDetected)
        {
            changeDirection();
        }
    }

    void changeDirection()
    {
        rb2d.velocity = Vector2.zero;
        Vector3 scale = transform.localScale;
        transform.localScale = scale.setX(scale.x * -1);
    }
}
