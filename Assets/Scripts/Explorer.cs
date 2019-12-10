using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Used to get data useful for loading and unloading scenes
/// </summary>
public class Explorer : MonoBehaviour
{
    public float sightRadius = 60;
    public float hindSightRadius = 80;

    private CircleCollider2D coll2d;
    private Collider2D Collider
    {
        get
        {
            if (coll2d == null)
            {
                coll2d = gameObject.AddComponent<CircleCollider2D>();
                coll2d.radius = sightRadius;
            }
            return coll2d;
        }
    }

    private CircleCollider2D behindColl2d;
    private Collider2D BehindCollider
    {
        get
        {
            if (behindColl2d == null)
            {
                behindColl2d = gameObject.AddComponent<CircleCollider2D>();
                behindColl2d.radius = hindSightRadius;
            }
            return behindColl2d;
        }
    }

    public bool canSee(Collider2D c2d)
    {
        return Collider.IsTouching(c2d);
    }

    public bool canSeeBehind(Collider2D c2d)
    {
        return BehindCollider.IsTouching(c2d);
    }
}
