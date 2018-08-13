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
        RaycastHit2D[] rch2ds = Physics2D.RaycastAll(transform.position, transform.right, sightRange);
        Debug.DrawLine(transform.position, transform.position + transform.right * sightRange, Color.blue);
        bool accelerating = false;
        foreach (RaycastHit2D rch2d in rch2ds)
        {
            if (rch2d && rch2d.collider.gameObject != gameObject)
            {
                HardMaterial hm = rch2d.collider.gameObject.GetComponent<HardMaterial>();
                if (hm)
                {
                    Debug.DrawLine(transform.position, rch2d.point, Color.red);
                    Debug.Log("DiamondShell (" + gameObject.name + ") sees object: " + rch2d.collider.gameObject.name);
                    speed += accelerationPerSecond * Time.deltaTime;
                    accelerating = true;
                }
            }
        }
        //Slow down if cant find anything
        if (!accelerating)
        {
            if (speed > 0)
            {
                speed -= accelerationPerSecond * Time.deltaTime;
                speed = Mathf.Max(speed, 0);
            }
        }
        //If moving, addforce to keep moving
        if (speed > 0)
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
}
