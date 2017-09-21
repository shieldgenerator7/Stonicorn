using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySimple : MonoBehaviour
{

    public float speed = 1.0f;//units per second
    public float healsPerSecond = 5.0f;
    public bool activeMove = false;//controls whether it can move or not
    public float allowedLeftAndRightVariance = 25.0f;//used to determine if a colliding object is left or right of this enemy

    public ParticleSystem fearParticles;//the particle system that activates when the enemy is frightened

    private Vector2 direction = Vector2.left;
    private bool goingRight = true;//whether the bug is going right relative to its orientation
    private float maxSpeedReached = 0;//highest speed reached since starting in this direction
    private bool healing = false;
    private bool losToPlayer = false;//"Line Of Sight to Player": whether this bug can see the player
    private static RaycastHit2D[] rch2ds = new RaycastHit2D[10];//for processing collider casts

    private Rigidbody2D rb2d;
    private HardMaterial hm;
    private BoxCollider2D groundCollider;//collider used to see if the enemy is touching the ground
    private GravityAccepter gravity;
    private GameObject player;

    // Use this for initialization
    void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
        hm = GetComponent<HardMaterial>();
        groundCollider = GetComponent<BoxCollider2D>();
        gravity = GetComponent<GravityAccepter>();
        direction = Utility.PerpendicularLeft(transform.up).normalized;
        player = GameManager.getPlayerObject();
    }

    private void Update()
    {
        losToPlayer = Utility.lineOfSight(gameObject, player);
        if (losToPlayer)
        {
            if (!fearParticles.isPlaying)
            {
                fearParticles.Play();
            }
        }
        else
        {
            if (fearParticles.isPlaying)
            {
                fearParticles.Stop();
            }
        }
        if (rb2d.velocity.magnitude < 0.1f)
        {
            hm.addIntegrity(healsPerSecond * Time.deltaTime);
            if (hm.getIntegrity() == hm.maxIntegrity)
            {
                healing = false;
            }
        }
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        if (activeMove && isGrounded())
        {
            float tempSpeed = speed;
            if (rb2d.velocity.magnitude < speed / 4)
            {
                tempSpeed *= speed * 2;
            }
            if (hm.getIntegrity() < hm.maxIntegrity / 4 || healing)
            {
                healing = true;
                tempSpeed = 0;
            }
            if (tempSpeed > 0)
            {
                rb2d.AddForce(rb2d.mass * direction * tempSpeed);
            }
            if (rb2d.velocity.magnitude > maxSpeedReached)
            {
                maxSpeedReached = rb2d.velocity.magnitude;
            }
            //Cliff detection
            if (!losToPlayer) //nothing between it and the player
            {
                if (senseFloorInFront() == null) //there's a cliff up ahead
                {
                    Logger.log(this.gameObject, "Switchdir cliff ahead");
                    switchDirection();
                    rb2d.AddForce(rb2d.mass * direction * rb2d.velocity.magnitude * 4);
                }
                GameObject wall = senseWallInFront();
                if (wall != null)
                {
                    if (!wall.GetComponent<Rigidbody>() && !wall.GetComponent<HardMaterial>())
                    {
                        Logger.log(this.gameObject, "Switchdir hitting wall: " + wall.name);
                        switchDirection();
                    }
                }
            }
            if (!healing && rb2d.velocity.magnitude < 0.01f)
            {
                Logger.log(this.gameObject, "Switchdir velocity less than threshold: " + rb2d.velocity.magnitude);
                switchDirection();
            }
        }
    }

    void OnCollisionEnter2D(Collision2D coll)
    {
        //If the object is left or ride of this enemy
        float angle = Vector2.Angle(transform.up, coll.contacts[0].point - (Vector2)transform.position);
        if (angle > 90 - allowedLeftAndRightVariance && angle < 90 + allowedLeftAndRightVariance)
        {
            if (coll.gameObject.GetComponent<HardMaterial>() == null && coll.gameObject.GetComponent<Rigidbody2D>() == null)
            {
                Logger.log(this.gameObject, "Switchdir after collision: " + coll.gameObject.name);
                switchDirection();
            }
        }
    }
    void OnTriggerEnter2D(Collider2D coll)
    {
        if (!coll.isTrigger && coll.gameObject.GetComponent<HardMaterial>() == null && coll.gameObject.GetComponent<Rigidbody2D>() == null)
        {
            Logger.log(this.gameObject, "Switchdir after tigger entered: " + coll.gameObject.name);
            switchDirection();
        }
    }

    void switchDirection()
    {
        maxSpeedReached = 0;
        goingRight = !goingRight;
        if (goingRight)
        {
            direction = Utility.PerpendicularRight(transform.up).normalized;
        }
        else
        {
            direction = Utility.PerpendicularLeft(transform.up).normalized;
        }
    }

    GameObject senseFloorInFront()
    {
        Vector2 ahead = direction * 2;
        Vector2 senseDir = ahead - (Vector2)transform.up.normalized;
        Debug.DrawLine((Vector2)transform.position + ahead, senseDir + (Vector2)transform.position, Color.blue);
        RaycastHit2D rch2d = Physics2D.Raycast((Vector2)transform.position + ahead, -transform.up, 1);
        if (rch2d)
        {
            return rch2d.collider.gameObject;
        }
        return null;
    }
    GameObject senseWallInFront()
    {
        Vector2 ahead = direction;
        Vector2 length = direction * 0.1f;
        Vector2 senseDir = ahead + length;
        Debug.DrawLine((Vector2)transform.position + ahead, (Vector2)transform.position + senseDir, Color.green);
        RaycastHit2D rch2d = Physics2D.Raycast((Vector2)transform.position + ahead, length, 1);
        if (rch2d)
        {
            return rch2d.collider.gameObject;
        }
        return null;
    }

    /// <summary>
    /// Returns true IFF the bottom is touching the ground
    /// </summary>
    /// <returns></returns>
    bool isGrounded()
    {
        int amount = groundCollider.Cast(-transform.up, rch2ds, 0, true);
        for (int i = 0; i < amount; i++)
        {
            RaycastHit2D rch2d = rch2ds[i];
            if (rch2d
                && !rch2d.collider.isTrigger
                && rch2d.collider.gameObject != gameObject)
            {
                return true;
            }
        }
        return false;
    }
}
