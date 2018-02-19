using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StickyPadChecker : MonoBehaviour
{
    private HashSet<GameObject> connectedObjs = new HashSet<GameObject>();

    // Use this for initialization
    void Start()
    {
    }

    public void init(Vector2 gravityDir)
    {
        transform.up = -gravityDir;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!connectedObjs.Contains(collision.gameObject))
        {
            Rigidbody2D rb2d = collision.gameObject.GetComponent<Rigidbody2D>();
            if (rb2d)
            {
                FixedJoint2D fj2d = gameObject.AddComponent<FixedJoint2D>();
                fj2d.connectedBody = rb2d;
                fj2d.autoConfigureConnectedAnchor = false;
            }
            else
            {
                TargetJoint2D tj2d = gameObject.AddComponent<TargetJoint2D>();
                tj2d.autoConfigureTarget = false;
            }
            connectedObjs.Add(collision.gameObject);
        }
    }
}
