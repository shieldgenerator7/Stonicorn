using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityZone : MonoBehaviour
{
    public float gravityScale = 9.81f;
    public bool mainGravityZone = true;//true to change camera angle, false to not
    public bool radialGravity = false;//true to make it gravitate towards the center of the gravity zone
    private Vector2 gravityVector;
    private List<Rigidbody2D> tenants = new List<Rigidbody2D>();//the list of colliders in this zone
    private bool playerIsTenant = false;//whether the player is inside this GravityZone

    private static RaycastHit2D[] rch2dStartup = new RaycastHit2D[Utility.MAX_HIT_COUNT];

    // Use this for initialization
    void Start()
    {
        gravityVector = -transform.up.normalized * gravityScale;
    }

    void OnTriggerEnter2D(Collider2D coll)
    {
        if (!coll.isTrigger)
        {
            Rigidbody2D rb2d = coll.gameObject.GetComponent<Rigidbody2D>();
            if (rb2d != null)
            {
                if (!tenants.Contains(rb2d))
                {
                    tenants.Add(rb2d);
                    if (coll.gameObject.isPlayer())
                    {
                        playerIsTenant = true;
                    }
                }
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
                if (coll.gameObject.isPlayer())
                {
                    playerIsTenant = false;
                }
            }
        }
    }
    private void Update()
    {
        //Check to see if the camera rotation needs updated
        if (mainGravityZone
            && playerIsTenant
            && !MenuManager.isMenuOpen())
        {
            Vector3 transformUp = transform.up;
            if (radialGravity)
            {
                transformUp = GameManager.Player.transform.position - transform.position;
            }
            if (Camera.main.transform.up != transformUp)
            {
                Camera.main.GetComponent<CameraController>().setRotation(transformUp);
            }
        }
    }
    void FixedUpdate()
    {
        if (GameManager.Rewinding)
        {
            return;//don't do anything if the time is rewinding
        }
        bool cleanNeeded = false;
        foreach (Rigidbody2D rb2d in tenants)
        {
            if (rb2d == null || ReferenceEquals(rb2d, null))
            {
                cleanNeeded = true;
                continue;
            }
            Vector2 finalGravityVector = gravityVector;
            if (radialGravity)
            {
                finalGravityVector = (transform.position - rb2d.transform.position).normalized * gravityScale;
            }
            Vector3 vector = finalGravityVector * rb2d.mass;
            GravityAccepter ga = rb2d.gameObject.GetComponent<GravityAccepter>();
            if (ga)
            {
                if (ga.AcceptsGravity)
                {
                    rb2d.AddForce(vector);
                    //Inform the gravity accepter of the direction
                    ga.addGravity(finalGravityVector);
                }
            }
            else
            {
                rb2d.AddForce(vector);
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
