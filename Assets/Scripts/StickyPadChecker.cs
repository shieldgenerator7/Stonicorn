using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StickyPadChecker : MonoBehaviour
{
    private HashSet<GameObject> connectedObjs = new HashSet<GameObject>();

    private Rigidbody2D rb2d;

    // Use this for initialization
    void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
    }

    public void init(Vector2 gravityDir)
    {
        transform.up = -gravityDir;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        stickToObject(collision.gameObject);
    }
    private void OnTriggerEnter2D(Collider2D coll)
    {
        stickToObject(coll.gameObject);
    }
    void stickToObject(GameObject go)
    {
        Rigidbody2D goRB2D = go.GetComponent<Rigidbody2D>();
        if (goRB2D)
        {
            bool foundObj = false;
            foreach (FixedJoint2D fj2d in GetComponents<FixedJoint2D>())
            {
                if (fj2d.connectedBody == goRB2D)
                {
                    foundObj = true;
                    break;
                }
            }
            if (!foundObj)
            {
                FixedJoint2D fj2d = gameObject.AddComponent<FixedJoint2D>();
                fj2d.connectedBody = goRB2D;
                fj2d.autoConfigureConnectedAnchor = false;
            }
        }
        else
        {
            if (!connectedObjs.Contains(go))
            {
                TargetJoint2D tj2d = gameObject.AddComponent<TargetJoint2D>();
                tj2d.autoConfigureTarget = false;
                rb2d.constraints = RigidbodyConstraints2D.FreezeAll;
                connectedObjs.Add(go);
            }
        }
    }
}
