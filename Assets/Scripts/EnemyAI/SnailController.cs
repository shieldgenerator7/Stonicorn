using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnailController : MonoBehaviour
{
    [Header("Settings")]
    public float rotateSpeed;
    public float maxRollDistance = 5;
    public float stickForce = 9.81f;//how much force it uses to stick to walls
    public float restickAngleAdjustment = 45;//used to keep it stuck to land around corners

    //Runtime Vars
    private Vector2 floorDirection;//points away from the floor
    private Vector2 floorRight;//if floorDirection is Vector2.up, floorRight is Vector2.right
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

    [Header("Components")]
    public Collider2D bottomDetector;//used to make sure the snail is at the right orientation before coming out
    private Animator animator;
    private GroundChecker ground;
    private GravityAccepter gravity;
    private Rigidbody2D rb2d;
    private HardMaterial hm;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponentInChildren<Animator>();
        ground = GetComponent<GroundChecker>();
        gravity = GetComponent<GravityAccepter>();
        rb2d = GetComponentInChildren<Rigidbody2D>();
        hm = GetComponent<HardMaterial>();
        Awake = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (Managers.Game.Rewinding)
        {
            Debug.Log("Snail: not processing snails while rewinding");
            //don't update while game manager is rewinding
            return;
        }
        //Own gravity
        if (ground.Grounded)
        {
            rb2d.AddForce(-floorDirection * rb2d.mass * stickForce);
            gravity.AcceptsGravity = false;
        }
        else
        {
            gravity.AcceptsGravity = true;
        }

        //Hunting
        if (Awake)
        {
            hm.dealsDamage = true;
            rb2d.angularVelocity = rotateSpeed;
            Debug.DrawLine(transform.position, (Vector2)transform.position + floorDirection, Color.blue);

            if (prevPos == Vector2.zero)
            {

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
                hm.dealsDamage = false;
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
                    hm.dealsDamage = false;
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
                updateFloorVector(collision, true);
            }
        }
    }
    private void OnCollisionStay2D(Collision2D collision)
    {
        updateFloorVector(collision, false);
    }
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (touchingObjects.ContainsKey(collision.gameObject))
        {
            touchingObjects.Remove(collision.gameObject);
        }
        if (touchingObjects.Count == 0)
        {
            floorDirection = Utility.RotateZ(floorDirection, restickAngleAdjustment * Mathf.Sign(rotateSpeed));
            floorRight = Utility.RotateZ(floorDirection, -90);
        }
    }
    void updateFloorVector(Collision2D collision, bool updateVelocity)
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
        newFD.Normalize();

        floorDirection = newFD;
        floorRight = Utility.RotateZ(floorDirection, -90);
    }
}
