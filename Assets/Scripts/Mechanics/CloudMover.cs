using UnityEngine;

[RequireComponent(typeof(GravityAccepter))]
[RequireComponent(typeof(Rigidbody2D))]
public class CloudMover : MonoBehaviour, ISetupable
{
    public bool destroyOnCollisionWithUnmovableSolid = true;
    public GameObject shadow;

    [AutoInitialize, SerializeField, HideInInspector]
    Rigidbody2D rb2d;
    [SerializeField, HideInInspector]
    SpriteRenderer shadowSR;
    [AutoInitialize, SerializeField, HideInInspector]
    Fader fader;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        FindAnyObjectByType<CloudMoverManager>().updateClouds();//TODO: set this up correctly
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!destroyOnCollisionWithUnmovableSolid) { return; }
        //if it collides with a solid piece of terrain,
        Collider2D collider = collision.collider;
        if (collider.isSolid() && !collider.GetComponent<Rigidbody2D>())
        {
            Debug.Log($"cloud {gameObject.name} ran into {collider.name}");
            //stop it
            rb2d.linearVelocity = Vector2.zero;
            this.enabled = false;
            //make it disappear
            fader.enabled = true;
            //make shadow disappear
            if (shadow)
            {
                shadow.SetActive(false);
            }
            this.enabled = false;
            FindAnyObjectByType<CloudMoverManager>().updateClouds();//TODO: set this up correctly
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

    public int setup()
    {
        int changeCount = 0;
        if (shadow)
        {
            SpriteRenderer newShadowSR = shadow.GetComponent<SpriteRenderer>();
            if (newShadowSR != shadowSR) { 
            shadowSR = newShadowSR;
                changeCount++;
            }
        }
        return changeCount;
    }
}
