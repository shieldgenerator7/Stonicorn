using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnailController : MonoBehaviour
{
    [Header("Settings")]
    public string food = "stone";
    public float moveSpeed;
    public float rotateSpeed;
    public float stickForce = 9.81f;//how much force it uses to stick to walls
    public float restickAngleAdjustment = 45;//used to keep it stuck to land around corners

    //Runtime Vars
    private Vector2 lastSeenFoodPosition;//where stone was last seen
    public Vector2 floorDirection;//points away from the floor
    public Vector2 floorRight;//if floorDirection is Vector2.up, floorRight is Vector2.right
    private Dictionary<GameObject, ContactPoint2D[]> touchingObjects = new Dictionary<GameObject, ContactPoint2D[]>();
    public bool isScared = false;
    public float rollDistanceTarget = 0;//how far this snail is willing to roll for the current target
    public float rollDistance = 0;//how far this snail has gone for the current target
    public Vector2 prevPos;

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

        //Own gravity
        rb2d.AddForce(-floorDirection * rb2d.mass * stickForce);
        if (isScared)
        {
            rb2d.angularVelocity = rotateSpeed;
            Debug.DrawLine(transform.position, (Vector2)transform.position + floorDirection, Color.blue);

            Debug.DrawLine(transform.position, lastSeenFoodPosition, Color.red);
            rollDistance += Vector2.Distance(transform.position, prevPos);
            prevPos = transform.position;
            if (rollDistance >= rollDistanceTarget)
            {
                int count = Utility.Cast(bottomDetector, Vector2.zero);
                if (count > 0)
                {
                    isScared = false;
                    rb2d.angularVelocity = 0;
                    checkHuntState();
                }
            }
        }
        animator.SetBool("scared", isScared);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        updateFloorVector(collision, true);
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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        checkHuntState();
    }

    /// <summary>
    /// Looks to see if there's food inside its collider
    /// </summary>
    void checkHuntState()
    {//2019-02-08: copied from DiamondShell.checkHuntState()
        Utility.RaycastAnswer answer = Utility.CastAnswer(stoneDetector, Vector2.zero);
        for (int i = 0; i < answer.count; i++)
        {
            RaycastHit2D rch2d = answer.rch2ds[i];
            if (rch2d.collider.gameObject == this.gameObject)
            {
                continue;
            }
            HardMaterial hm = rch2d.collider.gameObject.GetComponent<HardMaterial>();
            if (hm && hm.material == food)
            {
                //there is food in the collider
                lastSeenFoodPosition = rch2d.point;
                Debug.DrawLine(transform.position, lastSeenFoodPosition, Color.red, 1);
                rollDistanceTarget = Vector2.Distance(rch2d.point, transform.position);
                rollDistance = 0;
                rotateSpeed = Mathf.Abs(rotateSpeed)
                    * ((Vector3.Angle(
                        lastSeenFoodPosition - (Vector2)transform.position,
                        floorRight
                        ) < 90) ? -1 : 1);
                prevPos = transform.position;
                isScared = true;
                return;
            }
        }
    }
}
