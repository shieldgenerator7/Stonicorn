using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CactusController : Hazard, Blastable
{
    [SerializeField]
    private bool rooted = true;
    public bool Rooted
    {
        get => rooted;
        set
        {
            rooted = value;
            GetComponent<Rigidbody2D>().isKinematic = rooted;
        }
    }

    private void Start()
    {
        //Initialize state
        Rooted = rooted;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.GetComponent<Rigidbody2D>())
        {
            Rooted = false;
        }
    }

    public float checkForce(float force, Vector2 direction)
    {
        Rooted = false;
        return 0;
    }

    public float getDistanceFromExplosion(Vector2 explosionPos)
        => ((Vector2)transform.position - explosionPos).magnitude;

    public override SavableObject CurrentState
    {
        get => base.CurrentState.more(
            "rooted", rooted
            );
        set
        {
            base.CurrentState = value;
            Rooted = value.Bool("rooted");
        }
    }
}
