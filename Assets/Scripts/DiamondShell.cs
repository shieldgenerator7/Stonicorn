using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(HardMaterial))]
public class DiamondShell : MonoBehaviour
{

    //Settings
    public float maxSpeed = 10.0f;//max speed
    public float accelDuration = 2.0f;//how long it takes to get to max speed
    public float sightRange = 10.0f;//how far it can see from its center
    public string food = "stone";
    [Range(0, 1)]
    public float onDamageHealPercent = 0.2f;//what percent of damage it does to others it heals itself
    public float quickTurnDuration = 1.0f;//how long quick turns last
    public float maxWaitPeriod = 3.0f;//how long it will spin its wheels before switching direction

    //Runtime vars
    private float accelerationPerSecond = 0;//how fast the diamondshell can accelerate each second
    private float speed = 0;//current speed
    private float direction = 0;//-1 for left, 1 for right, 0 for stopped
    private float quickTurnStartTime = 0;
    private float quickTurnDirection = 0;//-1 for left, 1 for right, 0 for no quickTurn
    private float waitStartTime = 0;
    private bool isGrounded = true;

    //Components
    private Rigidbody2D rb2d;
    private HardMaterial hm;
    public Collider2D groundCollider;
    private static RaycastHit2D[] rch2dsGround = new RaycastHit2D[10];

    // Use this for initialization
    void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
        hm = GetComponent<HardMaterial>();
        hm.hardCollision += eatDamage;
        accelerationPerSecond = maxSpeed / accelDuration;
        if (groundCollider == null)
        {
            foreach (Collider2D coll2d in GetComponents<Collider2D>())
            {
                if (coll2d.isTrigger)
                {
                    groundCollider = coll2d;
                    Debug.LogWarning("DiamondShell (" + name + ") had no groundCollider, so a collider of type " + coll2d.GetType() + " was used.");
                    break;
                }
            }
            //if it's still null, there's an error
            if (groundCollider == null)
            {
                throw new UnityException("DiamondShell (" + name + ") has no groundCollider, and no trigger Collider2D!");
            }
        }
    }

    void FixedUpdate()
    {
        //If it's stuck, change direction
        if (direction != 0 && Mathf.Approximately(rb2d.velocity.magnitude, 0) && speed == maxSpeed)
        {
            if (waitStartTime == 0)
            {
                waitStartTime = Time.time;
            }
            else if (Time.time > waitStartTime + maxWaitPeriod)
            {
                waitStartTime = 0;
                quickTurnStartTime = Time.time;
                quickTurnDirection = -direction;
                direction *= -1;
                Debug.Log("DiamondShell (" + name + ") trying new direction: " + quickTurnDirection);
            }
        }
        //Check to see if there's any stones in sight
        float distLeft = checkFoodInDirection(-1.0f);
        float distRight = checkFoodInDirection(1.0f);
        //If any stones in range
        if (distLeft > 0 || distRight > 0 || quickTurnDirection != 0)
        {
            float prevDirection = direction;
            if (quickTurnDirection == 0)
            {
                if (distLeft > distRight)
                {
                    direction = -1;
                }
                else
                {
                    direction = 1;
                }
            }
            else
            {
                direction = quickTurnDirection;
                if (Time.time > quickTurnStartTime + quickTurnDuration)
                {
                    quickTurnDirection = 0;
                }
            }
            if (direction == prevDirection)
            {
                //Increase speed in that direction
                speed += accelerationPerSecond * Time.fixedDeltaTime;
                speed = Mathf.Min(speed, maxSpeed);
            }
            else
            {
                //Decellerate before switching directions
                speed -= accelerationPerSecond * 0.5f * Time.fixedDeltaTime;
                if (speed <= 0)
                {
                    speed = 0;
                }
                else
                {
                    direction = prevDirection;
                }
            }
        }
        else
        {
            //Otherwise slow down
            if (speed > 0)
            {
                speed -= accelerationPerSecond * Time.fixedDeltaTime;
                speed = Mathf.Max(speed, 0);
            }
            else
            {
                direction = 0;
            }
        }
        //If moving, addforce to keep moving
        if (speed > 0)
        {
            rb2d.AddForce(transform.right * rb2d.mass * speed * direction);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        float angle = Vector2.Angle(transform.right * direction, collision.contacts[0].point - (Vector2)transform.position);
        Debug.Log("DiamondShell (" + gameObject.name + ") hit something: " + collision.collider.gameObject.name + ", angle: " + angle);
        //If crashed into something in the direction of travel, 
        if (angle < 40)
        {
            if (quickTurnDirection == 0)
            {
                quickTurnStartTime = Time.time;
                quickTurnDirection = -direction;
                direction *= -1;
            }
        }
        checkGroundedState();
    }
    private void OnCollisionExit2D(Collision2D collision)
    {
        checkGroundedState();
    }

    /// <summary>
    /// Heals the diamond shell when it does damage
    /// Called from HardMaterial.hardCollision
    /// </summary>
    /// <param name="damageToSelf"></param>
    /// <param name="damageToOther"></param>
    void eatDamage(float damageToSelf, float damageToOther)
    {
        hm.addIntegrity(damageToOther * onDamageHealPercent);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="direction">-1 for left, 1 for right</param>
    /// <returns></returns>
    float checkFoodInDirection(float direction)
    {
        RaycastHit2D[] rch2ds = Physics2D.RaycastAll(transform.position, transform.right * direction, sightRange);
        Debug.DrawLine(transform.position, transform.position + transform.right * Mathf.Sign(direction) * sightRange, Color.blue);

        foreach (RaycastHit2D rch2d in rch2ds)
        {
            if (rch2d && rch2d.collider.gameObject != gameObject)
            {
                HardMaterial hm = rch2d.collider.gameObject.GetComponent<HardMaterial>();
                if (hm && hm.material == food)
                {
                    Debug.DrawLine(transform.position, rch2d.point, Color.red);
                    Debug.Log("DiamondShell (" + gameObject.name + ") sees object: " + rch2d.collider.gameObject.name);
                    return
                        (sightRange - rch2d.distance)//the closer the object is, the higher this number will be
                        + hm.getIntegrity();//the healthier this object is, the higher this number will be
                }
                else if (!isGrounded)
                {
                    return
                        (sightRange - rch2d.distance);
                }
            }
        }
        if (!isGrounded)
        {
            return sightRange;
        }
        return 0;
    }

    void checkGroundedState()
    {
        isGrounded = false;
        int count = groundCollider.Cast(Vector2.zero, rch2dsGround, 0, true);
        for (int i = 0; i < count; i++)
        {
            if (!rch2dsGround[i].collider.isTrigger)
            {
                isGrounded = true;
                return;
            }
        }
    }
}
