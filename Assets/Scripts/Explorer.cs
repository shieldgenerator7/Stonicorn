using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Used to get data useful for loading and unloading scenes
/// </summary>
public class Explorer : MonoBehaviour, ISetupable
{
    public float sightRadius = 30;
    public float hindSightRadius = 50;

    [SerializeField]
    private CircleCollider2D coll2d;

    [SerializeField]
    private CircleCollider2D behindColl2d;

    private void Start()
    {
        if (sightRadius > hindSightRadius)
        {
            Debug.LogError(
                $"sightRadius is greater than hindSightRadius! sightRadius: {sightRadius}, hindSightRadius: {hindSightRadius}",
                this
            );
        }
        coll2d.radius = sightRadius;
        coll2d.isTrigger = true;
        behindColl2d.radius = hindSightRadius;
        behindColl2d.isTrigger = true;
    }

    public bool canSee(Collider2D c2d)
        => c2d.OverlapPoint(transform.position) || coll2d.OverlapsCollider(c2d);

    public bool canSeeBehind(Collider2D c2d)
        => c2d.OverlapPoint(transform.position) || behindColl2d.OverlapsCollider(c2d);

    public int setup()
    {
        int changes = 0;

        if (coll2d && coll2d.radius != sightRadius)
        {
            coll2d.radius = sightRadius;
            changes++;
        }
        if (behindColl2d && behindColl2d.radius != hindSightRadius)
        {
            behindColl2d.radius = hindSightRadius;
            changes++;
        }

        if (changes > 0)
        {
            Debug.LogWarning("Set explorer radius", this);
        }

        return changes;
    }
}
