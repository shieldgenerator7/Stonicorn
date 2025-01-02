using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrabController : Hazard
{
    [Header("Settings")]
    public float moveSpeed = 1;
    public float throwSpeed = 5;
    public float throwDuration = 1;
    [Header("Animators")]
    public Animator legAnimator;
    [Header("Components")]
    public Collider2D cliffDetector;
    public Collider2D obstacleDetector;
    public Collider2D clawCollider;
    public Collider2D holdDetector;
    public Transform throwDirection;

    private Rigidbody2D rb2d;
    private GravityAccepter gravityAccepter;

    //
    // Processing Variables
    //
    /// <summary>
    /// The movable object it is carrying
    /// </summary>
    Rigidbody2D heldRB2D = null;
    private float throwStartTime = -1;

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
        gravityAccepter = GetComponent<GravityAccepter>();
    }

    void FixedUpdate()
    {
        moveSelf();
    }

    private void moveObject(Rigidbody2D rb2d)
    {
        float speed = moveSpeed;
        if (rb2d.linearVelocity.magnitude < 0.1f)
        {
            speed *= 2;
        }
        Vector3 forceVector = speed * transform.right * Mathf.Sign(transform.localScale.x);
        rb2d.AddForce(forceVector * rb2d.mass);
        if (rb2d.linearVelocity.magnitude > speed)
        {
            rb2d.linearVelocity = rb2d.linearVelocity.normalized * speed;
        }
    }

    private void moveSelf()
    {
        //Move self
        moveObject(rb2d);

        //Move held object
        if (heldRB2D)
        {
            moveHeldObject();
        }
    }

    private void moveHeldObject()
    {
        moveObject(heldRB2D);
    }    

    private bool canPickupObject
        => throwStartTime < 0 || Managers.Time.Time >= throwStartTime + throwDuration;

    private void pickupObject(Rigidbody2D collRB2D, bool setPosition)
    {
        heldRB2D = collRB2D;
        //StaticUntilTouched
        StaticUntilTouched sut = collRB2D.GetComponent<StaticUntilTouched>();
        if (sut)
        {
            sut.Rooted = false;
        }
        //Set Position
        if (setPosition)
        {
            heldRB2D.transform.position = (Vector2)clawCollider.bounds.center - (heldRB2D.worldCenterOfMass - (Vector2)heldRB2D.transform.position);
            setGravityAcceptance(heldRB2D.gameObject, false);
            heldRB2D.angularVelocity = 0;
        }
        //Pick up object
        moveHeldObject();

    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (ObstacleDetected)
        {
            if (!heldRB2D)
            {
                GameObject collGO = collision.gameObject;
                Rigidbody2D collRB2D = collGO.GetComponent<Rigidbody2D>();
                if (collRB2D)
                {
                    rb2d.nullifyMovement();
                    if (canPickupObject)
                    {
                        pickupObject(collRB2D, true);
                    }
                }
            }
            changeDirection();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!heldRB2D && HeldObjectDetected)
        {
            GameObject collGO = collision.gameObject;
            Rigidbody2D collRB2D = collGO.GetComponent<Rigidbody2D>();
            if (collRB2D)
            {
                if (canPickupObject)
                {
                    pickupObject(collRB2D, false);
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (CliffDetected)
        {
            //Let go of any objects
            if (heldRB2D)
            {
                throwObjects();
                heldRB2D = null;
            }
            //Change direction
            changeDirection();
        }
        if (!HeldObjectDetected)
        {
            setGravityAcceptance(collision.gameObject, true);
            heldRB2D = null;
        }
    }

    void changeDirection()
    {
        rb2d.nullifyMovement();
        Vector3 scale = transform.localScale;
        transform.localScale = scale.setX(scale.x * -1);
        Physics2D.SyncTransforms();
    }

    void throwObjects()
    {
        //Throw object
        heldRB2D.linearVelocity =
            (throwDirection.position - transform.position)
            * throwSpeed;
        throwStartTime = Managers.Time.Time;

        setGravityAcceptance(heldRB2D.gameObject, true);
    }

    void setGravityAcceptance(GameObject go, bool accept)
    {
        Rigidbody2D rb2d = go.GetComponent<Rigidbody2D>();
        if (rb2d)
        {
            rb2d.gravityScale=accept ? 1 : 0;
        }
        GravityAccepter ga = go.GetComponent<GravityAccepter>();
        if (ga)
        {
            ga.AcceptsGravity = accept;
        }
    }
}
