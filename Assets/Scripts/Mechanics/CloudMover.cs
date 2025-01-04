using System.Runtime.ConstrainedExecution;
using UnityEngine;
using static Utility;

[RequireComponent(typeof(GravityAccepter))]
[RequireComponent(typeof(Rigidbody2D))]
public class CloudMover : MonoBehaviour
{
    public float speed = 0.02f;

    public GameObject shadow;

    GravityAccepter gravityAccepter;

    Rigidbody2D rb2d;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gravityAccepter = GetComponent<GravityAccepter>();
        rb2d = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector2 center = Vector2.zero;// gravityAccepter.Center.position //TODO: use gravityAccepter center
        Vector2 gravityVector = center - (Vector2)transform.position;
        Vector2 sideVector = new Vector3(-gravityVector.y, gravityVector.x) / Mathf.Sqrt(gravityVector.x * gravityVector.x + gravityVector.y * gravityVector.y);
        rb2d.linearVelocity = sideVector.normalized * speed;
        transform.up = -gravityVector;
    }

    private void LateUpdate()
    {
        float MAX_DISTANCE = 200;
        float EXTRA_DISTANCE = 10;
        if (shadow)
        {
            Vector3 gravityVector = (Vector2.zero - (Vector2)transform.position).normalized;
            //find ground point
            Vector2 groundPoint = transform.position + (gravityVector * MAX_DISTANCE);
            RaycastAnswer rca = Utility.RaycastAll(transform.position, gravityVector, MAX_DISTANCE);
            for (int i = 0; i < rca.count; i++)
            {
                RaycastHit2D rch2d = rca.rch2ds[i];
                if (!rch2d.collider.isTrigger && !rch2d.collider.GetComponent<Rigidbody2D>())
                {
                    groundPoint = rch2d.point;
                    break;
                }
            }

            //extend shadow
            float distance = Vector2.Distance(groundPoint, transform.position) + EXTRA_DISTANCE;
            shadow.transform.position = transform.position + (gravityVector * distance/2);
            SpriteRenderer shadowSR = shadow.GetComponent<SpriteRenderer>();
            Vector2 size = shadowSR.size;
            size.y = distance;
            shadowSR.size = size;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        //if it collides with a solid piece of terrain,
        Collider2D collider = collision.collider;
        if (collider.isSolid() && !collision.collider.GetComponent<Rigidbody2D>())
        {
            Debug.Log($"cloud {gameObject.name} ran into {collider.name}");
            //stop it
            rb2d.linearVelocity = Vector2.zero;
            this.enabled = false;
            //make it disappear
            Fader fader = GetComponent<Fader>();
            fader.enabled = true;
            //make shadow disappear
            if (shadow)
            {
                shadow.SetActive(false);
            }
        }
    }
}
