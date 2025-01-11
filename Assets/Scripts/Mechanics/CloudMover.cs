using UnityEngine;

[RequireComponent(typeof(GravityAccepter))]
[RequireComponent(typeof(Rigidbody2D))]
public class CloudMover : MonoBehaviour
{
    public float speed = 0.02f;

    public GameObject shadow;
    [Header("Shadow ground finding")]
    public float MAX_DISTANCE = 200;
    public float EXTRA_DISTANCE = 10;
    public string LayerName = "Ground";

    GravityAccepter gravityAccepter;
    Rigidbody2D rb2d;
    SpriteRenderer shadowSR;
    int layerMask;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gravityAccepter = GetComponent<GravityAccepter>();
        rb2d = GetComponent<Rigidbody2D>();
        if (shadow)
        {
        shadowSR = shadow?.GetComponent<SpriteRenderer>();
        }
        layerMask = LayerMask.NameToLayer(LayerName);
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
        if (shadow)
        {
            Vector3 gravityVector = (Vector2.zero - (Vector2)transform.position).normalized;

            //find ground point
            Vector2 groundPoint = transform.position + (gravityVector * MAX_DISTANCE);
            RaycastHit2D rch2d = Physics2D.Raycast(
                transform.position,
                gravityVector,
                MAX_DISTANCE,
                layerMask
                );
            groundPoint = rch2d.point;

            //extend shadow
            float distance = Vector2.Distance(groundPoint, transform.position) + EXTRA_DISTANCE;
            acceptJobState(transform.position + (gravityVector * distance / 2), distance);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        //if it collides with a solid piece of terrain,
        Collider2D collider = collision.collider;
        if (collider.isSolid() && !collider.GetComponent<Rigidbody2D>())
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

    internal void acceptJobState(Vector2 pos, float height)
    {
        //shadow position
        shadow.transform.position = pos;
        //shadow height
        Vector2 size = shadowSR.size;
        size.y = height;
        shadowSR.size = size;
    }
}
