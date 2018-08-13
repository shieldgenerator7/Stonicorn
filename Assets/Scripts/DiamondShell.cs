using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiamondShell : MonoBehaviour
{

    //Settings
    public float accelerationPerSecond = 2.0f;//how fast the diamondshell can accelerate each second
    public float sightRange = 10.0f;//how far it can see from its center

    //Runtime vars
    private float speed = 0;//current speed
    private float direction = 1;//-1 for left, 1 for right

    //Components
    private Rigidbody2D rb2d;

    // Use this for initialization
    void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        //Check to see if there's any stones in sight
        float distLeft = checkFoodInDirection(-1.0f);
        float distRight = checkFoodInDirection(1.0f);
        //If any stones in range
        if (distLeft > 0 || distRight > 0)
        {
            if (distLeft > distRight)
            {
                direction = -1;
            }
            else
            {
                direction = 1;
            }
            //Increase speed in that direction
            speed += accelerationPerSecond * direction * Time.deltaTime;
        }
        else
        {
            //Otherwise slow down
            if (speed != 0)
            {
                speed = Mathf.MoveTowards(speed, 0, accelerationPerSecond * Time.deltaTime);
            }
        }
        //If moving, addforce to keep moving
        if (speed != 0)
        {
            rb2d.AddForce(transform.right * rb2d.mass * speed);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        float angle = Vector2.Angle(transform.right, collision.contacts[0].point - (Vector2)transform.position);
        Debug.Log("DiamondShell (" + gameObject.name + ") hit something: " + collision.collider.gameObject.name + ", angle: " + angle);
        //If crashed into something in the direction of travel, 
        if (angle < 10)
        {
            //Stop
            speed = 0;
        }
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
                if (hm)
                {
                    Debug.DrawLine(transform.position, rch2d.point, Color.red);
                    Debug.Log("DiamondShell (" + gameObject.name + ") sees object: " + rch2d.collider.gameObject.name);
                    return 
                        (sightRange - rch2d.distance)//the closer the object is, the higher this number will be
                        + hm.getIntegrity();//the healthier this object is, the higher this number will be
                }
            }
        }
        return 0;
    }
}
