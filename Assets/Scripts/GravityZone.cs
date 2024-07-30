using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GravityZone : MonoBehaviour
{
    public float gravityScale = 9.81f;
    public bool mainGravityZone = true;//true to change camera angle, false to not
    public bool radialGravity = true;//true to make it gravitate towards the center of the gravity zone

    private Vector2 gravityVector;
    private List<Rigidbody2D> tenants = new List<Rigidbody2D>();//the list of Rigidbody2D in this zone
    private List<GravityAccepter> tenantsGAs = new List<GravityAccepter>();//the list of GravityAccepter in this zone

    private Collider2D coll2d;
    public Collider2D Collider2D
    {
        get
        {
            if (coll2d == null)
            {
                coll2d = GetComponent<Collider2D>();
            }
            return coll2d;
        }
    }

    // Use this for initialization
    void Start()
    {
        coll2d = GetComponent<Collider2D>();
        gravityVector = -transform.up.normalized * gravityScale;
    }

    void OnTriggerEnter2D(Collider2D coll)
    {
        if (coll.isSolid())
        {
            GravityAccepter ga = coll.GetComponent<GravityAccepter>();
            if (ga)
            {
                if (!tenantsGAs.Contains(ga))
                {
                    tenantsGAs.Add(ga);
                }
                ga.Center = transform;
            }
            else
            {
                Rigidbody2D rb2d = coll.GetComponent<Rigidbody2D>();
                if (rb2d)
                {
                    if (!tenants.Contains(rb2d))
                    {
                        tenants.Add(rb2d);
                    }
                }
            }
        }
    }
    void OnTriggerExit2D(Collider2D coll)
    {
        if (coll.isSolid())
        {
            GravityAccepter ga = coll.GetComponent<GravityAccepter>();
            if (ga)
            {
                if (!tenantsGAs.Contains(ga))
                {
                    tenantsGAs.Add(ga);
                }
            }
            Rigidbody2D rb2d = coll.GetComponent<Rigidbody2D>();
            if (rb2d)
            {
                tenants.Remove(rb2d);
            }
        }
    }

    void FixedUpdate()
    {
        tenantsGAs.RemoveAll(ten => !ten || ReferenceEquals(ten, null));
        tenants.RemoveAll(ten => !ten || ReferenceEquals(ten, null));
        tenantsGAs.ForEach(ga =>
        {
            if (ga.AcceptsGravity)
            {
                Rigidbody2D rb2d = ga.Rigidbody2D;
                Vector2 finalGravityVector = (radialGravity)
                    ? (Vector2)(transform.position - rb2d.transform.position).normalized
                        * gravityScale
                    : gravityVector;
                Vector3 vector = finalGravityVector * rb2d.mass;
                vector *= ga.gravityScale;
                rb2d.AddForce(vector);
                //Inform the gravity accepter of the direction
                ga.addGravity(finalGravityVector);
            }
        });
        tenants.ForEach(rb2d =>
        {
            Vector2 finalGravityVector = (radialGravity)
                ? (Vector2)(transform.position - rb2d.transform.position).normalized
                    * gravityScale
                : gravityVector;
            Vector3 vector = finalGravityVector * rb2d.mass;
            rb2d.AddForce(vector);
        });
    }

    public bool Contains(Vector2 pos) => coll2d.OverlapPoint(pos);

    public static Vector2 getUpDirection(Vector2 pos)
    {
        GravityZone gz = getGravityZone(pos);
        if (!gz)
        {
            return Vector2.zero;
        }
        //Check to see if the camera rotation needs updated
        return (gz.radialGravity)
            ? (pos - (Vector2)gz.transform.position)
            : (Vector2)gz.transform.up;

    }

    public static GravityZone getGravityZone(Vector2 pos)
        => FindObjectsByType<GravityZone>(FindObjectsSortMode.None)
            .FirstOrDefault(
                gz => gz.mainGravityZone && gz.Collider2D.OverlapPoint(pos)
            );
}
