using UnityEngine;
using System.Collections;

public class BoulderFriendChecker : MonoBehaviour {

    public GameObject friend;//the friend to search for

    public bool bounce = false;
    public bool grounded = false;
    private Rigidbody2D rb2d;

	// Use this for initialization
	void Start () {
        rb2d = GetComponent<Rigidbody2D>();
	}

    void FixedUpdate()
    {
        if (bounce)
        {
            if (isGrounded())
            {
                rb2d.AddForce(new Vector2(0, 50));
            }
        }
    }

    void OnTriggerEnter2D(Collider2D coll)
    {
        if (coll.gameObject.Equals(friend))
        {
            bounce = true;
        }
    }

    bool isGrounded()
    {//2016-07-29: copied from PlayerController.isGrounded()
        bool isGrounded = false;

        Vector3 pos = transform.position;
        Vector2 pos2 = new Vector2(pos.x, pos.y);
        int numberOfLines = 5;
        Bounds bounds = GetComponent<CircleCollider2D>().bounds;
        float width = bounds.max.x - bounds.min.x;
        float increment = width / (numberOfLines - 1);//-1 because the last one doesn't take up any space
        Vector3 startV = bounds.min;
        float length = 1f;
        for (int i = 0; i < numberOfLines; i++)
        {
            Vector2 start = new Vector2(startV.x + i * increment, pos.y - length);
            Vector2 dir2 = new Vector2(0, length);
            Debug.DrawLine(start, start + dir2, Color.black);
            RaycastHit2D rch2d = Physics2D.Raycast(start, dir2, length);// -1*(start), 1f);
            if (rch2d)
            {
                GameObject ground = rch2d.collider.gameObject;
                if (!ground.Equals(transform.gameObject))
                {
                    isGrounded = true;
                    break;
                }
            }
        }
        this.grounded = isGrounded;
        return isGrounded;
    }
}
