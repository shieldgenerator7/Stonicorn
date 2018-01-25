using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AfterWind : MonoBehaviour
{//2018-01-25: copied from GravityZone

    public Vector2 windVector;//direction and magnitude

    private BoxCollider2D coll;
    private RaycastHit2D[] rch2dStartup = new RaycastHit2D[100];

    // Use this for initialization
    void Start()
    {
        coll = GetComponent<BoxCollider2D>();
    }

    void FixedUpdate()
    {
        if (GameManager.isRewinding())
        {
            return;//don't do anything if it is rewinding
        }
        int count = coll.Cast(Vector2.zero, rch2dStartup);
        for (int i = 0; i < count; i++)
        {
            Rigidbody2D rb2d = rch2dStartup[i].collider.gameObject.GetComponent<Rigidbody2D>();
            if (rb2d)
            {
                Vector3 vector = windVector * rb2d.mass;
                GravityAccepter ga = rb2d.gameObject.GetComponent<GravityAccepter>();
                if (ga)
                {
                    if (ga.AcceptsGravity)
                    {
                        rb2d.AddForce(vector);
                        //Inform the gravity accepter of the direction
                        ga.addGravity(this.windVector);
                    }
                }
                else
                {
                    rb2d.AddForce(vector);
                }
            }
        }
    }
}
