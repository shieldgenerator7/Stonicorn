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
        if (collision.collider.isSolid())
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
