using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnailController : MonoBehaviour
{
    [Header("Settings")]
    public float moveSpeed;
    public float rotateSpeed;
    public float stickForce = 9.81f;//how much force it uses to stick to walls
    public float restickAngleAdjustment = 45;//used to keep it stuck to land around corners

    //Runtime Vars
    private Vector2 lastSeenStonePosition;//where stone was last seen
    public Vector2 floorDirection;//points away from the floor
    public Vector2 floorRight;//if floorDirection is Vector2.up, floorRight is Vector2.right
    private Dictionary<GameObject, ContactPoint2D[]> touchingObjects = new Dictionary<GameObject, ContactPoint2D[]>();

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
    }
}
