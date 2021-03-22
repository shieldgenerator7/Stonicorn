using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiscoverableActivator : MonoBehaviour, ISwappable
{
    public MemoryMonoBehaviour mmb;

    private void Start()
    {
        mmb.onDiscovered += () => Destroy(this);
    }

    private void OnCollisionEnter2D(Collision2D coll2D)
    {
        if (coll2D.collider.isPlayerSolid())
        {
            mmb.Discovered = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D coll2D)
    {
        if (coll2D.isPlayerSolid())
        {
            mmb.Discovered = true;
        }
    }

    public void nowSwapped()
    {
        mmb.Discovered = true;
    }
}
