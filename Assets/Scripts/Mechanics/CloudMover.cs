using UnityEngine;

[RequireComponent(typeof(GravityAccepter))]
[RequireComponent(typeof(Rigidbody2D))]
public class CloudMover : MonoBehaviour
{

    public GameObject shadow;

    Rigidbody2D rb2d;
    SpriteRenderer shadowSR;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
        if (shadow)
        {
            shadowSR = shadow?.GetComponent<SpriteRenderer>();
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

    internal void acceptMoveJobState(Vector2 velocity, Vector2 up)
    {
        rb2d.linearVelocity = velocity;
        transform.up = up;
    }

    internal void acceptShadowJobState(Vector2 pos, float height)
    {
        //shadow position
        shadow.transform.position = pos;
        //shadow height
        Vector2 size = shadowSR.size;
        size.y = height;
        shadowSR.size = size;
    }
}
