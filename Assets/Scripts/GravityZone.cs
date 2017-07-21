using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityZone : MonoBehaviour
{

    private PolygonCollider2D coll;
    public float gravityScale = 9.81f;
    public bool mainGravityZone = true;//true to change camera angle, false to not
    private Vector2 gravityVector;
    private List<Rigidbody2D> tenants = new List<Rigidbody2D>();//the list of colliders in this zone

    // Use this for initialization
    void Start()
    {
        coll = GetComponent<PolygonCollider2D>();
        gravityVector = -transform.up.normalized * gravityScale;
    }

    void OnTriggerEnter2D(Collider2D coll)
    {
        if (!coll.isTrigger)
        {
            Rigidbody2D rb2d = coll.gameObject.GetComponent<Rigidbody2D>();
            if (rb2d != null)
            {
                tenants.Add(rb2d);
            }
        }
    }
    void OnTriggerExit2D(Collider2D coll)
    {
        if (!coll.isTrigger)
        {
            Rigidbody2D rb2d = coll.gameObject.GetComponent<Rigidbody2D>();
            if (rb2d != null)
            {
                tenants.Remove(coll.gameObject.GetComponent<Rigidbody2D>());
            }
        }
    }
    void FixedUpdate()
    {
        bool cleanNeeded = false;
        foreach (Rigidbody2D rb2d in tenants)
        {
            if (rb2d == null || ReferenceEquals(rb2d, null))
            {
                cleanNeeded = true;
                continue;
            }
            Vector3 vector = gravityVector * rb2d.mass;
            GravityAccepter ga = rb2d.gameObject.GetComponent<GravityAccepter>();
            if (ga)
            {
                if (ga.AcceptsGravity)
                {
                    rb2d.AddForce(vector);
                    //Inform the gravity accepter of the direction
                    ga.addGravity(this.gravityVector);
                }
            }
            else
            {
                rb2d.AddForce(vector);
            }
        }
        //Check to see if the camera rotation needs updated
        if (mainGravityZone && Camera.main.transform.rotation != transform.rotation)
        {
            //Check to see if Merky is in this GravityZone
            if (GameManager.getPlayerObject().GetComponent<GravityAccepter>().Gravity == gravityVector)
            {
                Camera.main.GetComponent<CameraController>().setRotation(transform.rotation);
            }
        }
        if (cleanNeeded)
        {
            for (int i = tenants.Count - 1; i >= 0; i--)
            {
                if (tenants[i] == null || ReferenceEquals(tenants[i], null))
                {
                    tenants.RemoveAt(i);
                }
            }
        }
    }
}
