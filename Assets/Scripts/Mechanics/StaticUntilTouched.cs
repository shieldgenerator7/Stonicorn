using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticUntilTouched : SavableMonoBehaviour, IBlastable
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
            onRootedChanged?.Invoke(rooted);
        }
    }
    public delegate void OnRootedChanged(bool rooted);
    public event OnRootedChanged onRootedChanged;

#if UNITY_EDITOR
    [Header("Dev Tools")]
    [Tooltip("Set this and unset Rooted to allow the object to drop to its natural resting point.")]
    public bool reverse = false;//used to help determine an object's starting point
#endif

    private void Start()
    {
        init();
    }
    public override void init()
    {
        //Initialize state
        Rooted = rooted;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
#if UNITY_EDITOR
        if (reverse)
        {
            if (collision.collider.isSolid())
            {
                Rooted = true;
                Rigidbody2D rb2d = GetComponent<Rigidbody2D>();
                rb2d.velocity = Vector2.zero;
                rb2d.angularVelocity = 0;
                return;
            }
        }
#endif
        if (rooted)
        {
            if (collision.collider.isSolid())
            {
                Rooted = false;
                GetComponent<Rigidbody2D>().velocity = collision.relativeVelocity;
            }
        }
    }

    public float checkForce(float force, Vector2 direction)
    {
        Rooted = false;
        return 0;
    }

    public float getDistanceFromExplosion(Vector2 explosionPos)
        => ((Vector2)transform.position - explosionPos).magnitude;

    public override bool IsSpawnedScript => true;

    public override SavableObject CurrentState
    {
        get => new SavableObject(this,
            "rooted", rooted
            );
        set
        {
            Rooted = value.Bool("rooted");
        }
    }
}
