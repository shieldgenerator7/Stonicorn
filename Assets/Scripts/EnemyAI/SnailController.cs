using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class controls the behaviour of the Snail enemy type.
/// It "sleeps" (outside its shell) until it gets hit by something,
/// then it "wakes up" (restracts into its shell) and exposes its spikes,
/// and then rolls in the direction it was hit.
/// The spikes deal damage because this object is a subtype of Hazard
/// and its Hazardous property returns true while the spikes are out.
/// When something that can take Hazard damage collides with this object,
/// that object responds to hitting a Hazard on its own (i.e. Merky hitting the Snail's spikes)
/// SNAIL DEALS DAMAGE TO MERKY, EVEN THO SNAIL CLASS HAS NO CODE DOING SO
/// </summary>
public class SnailController : Hazard
{
    [Header("Settings")]
    public float rotateSpeed;
    public float maxRollDistance = 5;
    public float stickForce = 9.81f;//how much force it uses to stick to walls
    public float restickAngleAdjustment = 45;//used to keep it stuck to land around corners

    //Runtime Vars
    private Vector2 floorRight;//if floorDirection is Vector2.up, floorRight is Vector2.right
    private Vector2 floorDirection;//points away from the floor
    /// <summary>
    /// The direction pointing perpendicularly away from the floor.
    /// </summary>
    private Vector2 FloorDirection
    {
        get => floorDirection;
        set
        {
            floorDirection = value.normalized;
            floorRight = Utility.RotateZ(floorDirection, -90);
        }
    }
    private Dictionary<GameObject, ContactPoint2D[]> touchingObjects = new Dictionary<GameObject, ContactPoint2D[]>();
    private float rollDistance = 0;//how far this snail has gone for the current target
    private Vector2 prevPos;
    private bool Awake
    {
        get
        {
            return animator.GetCurrentAnimatorStateInfo(0).IsName("enemy_snail_awake");
        }
        set
        {
            animator.SetBool("awake", value);
        }
    }

    public override bool Hazardous => Awake;

    [Header("Components")]
    public Collider2D bottomDetector;//used to make sure the snail is at the right orientation before coming out
    private Animator animator;
    private GroundChecker ground;
    private GravityAccepter gravity;
    private Rigidbody2D rb2d;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponentInChildren<Animator>();
        ground = GetComponent<GroundChecker>();
        gravity = GetComponent<GravityAccepter>();
        rb2d = GetComponentInChildren<Rigidbody2D>();
        Awake = false;
        FloorDirection = transform.up;
    }

    void FixedUpdate()
    {
        if (Managers.Game.Rewinding)
        {
            //don't update while game manager is rewinding
            return;
        }
        //Own gravity
        if (ground.isGroundedInDirection(-FloorDirection))
        {
            rb2d.AddForce(-FloorDirection * rb2d.mass * stickForce);
            gravity.AcceptsGravity = false;
        }
        else
        {
            gravity.AcceptsGravity = true;
        }

        //Hunting
        if (Awake)
        {
            rb2d.angularVelocity = rotateSpeed;
#if UNITY_EDITOR
            Debug.DrawLine(transform.position, (Vector2)transform.position + FloorDirection, Color.blue);
#endif
            if (prevPos == Vector2.zero)
            {
                //do nothing, wait for it to update
            }
            //If it's moved since last frame,
            else if ((Vector2)transform.position != prevPos)
            {
                //add to the total count of distance
                rollDistance += Vector2.Distance(transform.position, prevPos);
            }
            else
            {
                //Make it so the player can hit it
                Awake = false;
            }
            prevPos = transform.position;
            //If it has rolled its max distance,
            if (rollDistance >= maxRollDistance)
            {
                //And it is on the ground,
                int count = Utility.Cast(bottomDetector, Vector2.zero);
                if (count > 0)
                {
                    //Make it go to sleep again
                    Awake = false;
                    //Flipping
                    if (rb2d.angularVelocity != 0)
                    {
                        Vector3 flipScale = animator.transform.localScale;
                        flipScale.x = Mathf.Sign(rb2d.angularVelocity);
                        animator.transform.localScale = flipScale;
                    }
                }
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        bool interactableObject = collision.gameObject.isSavable();
        //If sleeping,
        if (!Awake)
        {
            if (interactableObject)
            {
                //Wake up and roll out
                rollDistance = 0;
                rotateSpeed = Mathf.Abs(rotateSpeed)
                    * ((Vector3.Angle(
                        collision.GetContact(0).point - (Vector2)transform.position,
                        floorRight
                        ) < 90) ? -1 : 1);
                prevPos = Vector2.zero;
                Awake = true;
            }
        }
        //Else if awake,
        else
        {
            if (!interactableObject)
            {
                //Update floor variables
                updateFloorVector(collision);
            }
        }
    }
    private void OnCollisionStay2D(Collision2D collision)
    {
        updateFloorVector(collision);
    }
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (touchingObjects.ContainsKey(collision.gameObject))
        {
            touchingObjects.Remove(collision.gameObject);
        }
        if (touchingObjects.Count == 0)
        {
            FloorDirection = Utility.RotateZ(FloorDirection, restickAngleAdjustment * Mathf.Sign(rotateSpeed));
        }
    }
    void updateFloorVector(Collision2D collision)
    {
        touchingObjects[collision.gameObject] = collision.contacts;
        Vector2 newFD = Vector2.zero;
        foreach (ContactPoint2D[] cp2ds in touchingObjects.Values)
        {
            foreach (ContactPoint2D cp2d in cp2ds)
            {
                newFD += cp2d.normal;
            }
        }
        FloorDirection = newFD;
    }

    public override SavableObject getSavableObject()
    {
        SavableObject savObj = base.getSavableObject();
        Dictionary<string, object> data = savObj.data;
        data.Add("flipDir", Mathf.Sign(animator.transform.localScale.x));
        data.Add("awake", Awake);
        data.Add("rollDistance", rollDistance);
        data.Add("prevPos", prevPos);
        return savObj;
    }

    public override void acceptSavableObject(SavableObject savObj)
    {
        base.acceptSavableObject(savObj);
        if (!animator)
        {
            Start();
        }
        Vector3 animScale = animator.transform.localScale;
        animScale.x = Mathf.Abs(animScale.x) * (float)savObj.data["flipDir"];
        animator.transform.localScale = animScale;
        Awake = (bool)savObj.data["awake"];
        rollDistance = (float)savObj.data["rollDistance"];
        prevPos = (Vector2)savObj.data["prevPos"];
        FloorDirection = transform.up;
    }
}
