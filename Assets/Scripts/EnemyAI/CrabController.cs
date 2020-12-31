using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrabController : Hazard
{
    [Header("Settings")]
    public float moveSpeed = 1;
    public float throwSpeed = 5;
    [Header("Animators")]
    public Animator legAnimator;
    [Header("Components")]
    public Collider2D cliffDetector;
    public Collider2D obstacleDetector;
    public Collider2D clawCollider;
    public Collider2D holdDetector;
    public Transform throwDirection;

    private Rigidbody2D rb2d;

    //
    // Processing Variables
    //
    bool holdingObject = false;//true if holding one or more movable objects

    //
    // Properties
    //

    public override bool Hazardous => false;

    public bool CliffDetected
        => Utility.CastCountSolid(cliffDetector, Vector2.zero) == 0;

    public bool ObstacleDetected
        => Utility.CastCountSolid(obstacleDetector, Vector2.zero) > 0;

    public bool HeldObjectDetected
        => Utility.CastCountSolid(holdDetector, Vector2.zero) > 0;

    // Start is called before the first frame update
    void Start()
    {
        init();
    }
    public override void init()
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
                * rb2d.mass * 2
                * moveSpeed
                );
            if (rb2d.velocity.sqrMagnitude > moveSpeed * moveSpeed)
            {
                rb2d.velocity = rb2d.velocity.normalized * moveSpeed;
            }
        }
        if (holdingObject)
        {
            bool foundValidRB2D = false;
            Utility.RaycastAnswer rca = Utility.CastAnswer(holdDetector, Vector2.zero);
            if (rca.count > 0)
            {
                for (int i = 0; i < rca.count; i++)
                {
                    RaycastHit2D rch2d = rca.rch2ds[i];
                    if (!rch2d.collider.isTrigger)
                    {
                        Rigidbody2D collRB2D = rch2d.collider.GetComponent<Rigidbody2D>();
                        if (collRB2D)
                        {
                            collRB2D.velocity = rb2d.velocity;
                            collRB2D.angularVelocity = 0;
                            foundValidRB2D = true;
                        }
                    }
                }
            }
            if (!foundValidRB2D)
            {
                holdingObject = false;
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (ObstacleDetected)
        {
            if (!holdingObject)
            {
                GameObject collGO = collision.gameObject;
                Rigidbody2D collRB2D = collGO.GetComponent<Rigidbody2D>();
                if (collRB2D)
                {
                    rb2d.nullifyMovement();
                    //Pick up object
                    collGO.transform.position = clawCollider.bounds.center;
                }
            }
            changeDirection();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (HeldObjectDetected)
        {
            GameObject collGO = collision.gameObject;
            Rigidbody2D collRB2D = collGO.GetComponent<Rigidbody2D>();
            if (collRB2D)
            {
                holdingObject = true;
                collRB2D.velocity = rb2d.velocity;
                collRB2D.angularVelocity = 0;
            }
        }
        else
        {
            holdingObject = false;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (CliffDetected)
        {
            //Let go of any objects
            if (holdingObject)
            {
                holdingObject = false;
                throwObjects();
            }
            //Change direction
            changeDirection();
        }
        if (!HeldObjectDetected)
        {
            holdingObject = false;
        }
    }

    void changeDirection()
    {
        rb2d.nullifyMovement();
        Vector3 scale = transform.localScale;
        transform.localScale = scale.setX(scale.x * -1);
    }

    void throwObjects()
    {
        Utility.RaycastAnswer rca = Utility.CastAnswer(holdDetector, Vector2.zero);
        if (rca.count > 0)
        {
            for (int i = 0; i < rca.count; i++)
            {
                RaycastHit2D rch2d = rca.rch2ds[i];
                if (!rch2d.collider.isTrigger)
                {
                    Rigidbody2D collRB2D = rch2d.collider.GetComponent<Rigidbody2D>();
                    if (collRB2D)
                    {
                        //Throw object
                        collRB2D.velocity =
                            (throwDirection.position - transform.position)
                            * throwSpeed;
                    }
                }
            }
        }
    }
}
