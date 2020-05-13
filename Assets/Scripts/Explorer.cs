using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Used to get data useful for loading and unloading scenes
/// </summary>
public class Explorer : MonoBehaviour
{
    public float sightRadius = 30;
    public float hindSightRadius = 50;

    [SerializeField]
    private CircleCollider2D coll2d;

    [SerializeField]
    private CircleCollider2D behindColl2d;

    private void Start()
    {
        coll2d.radius = sightRadius;
        coll2d.isTrigger = true;
        behindColl2d.radius = hindSightRadius;
        behindColl2d.isTrigger = true;
    }

    public bool canSee(Collider2D c2d)
    {
        return coll2d.IsTouching(c2d);
    }

    public bool canSeeBehind(Collider2D c2d)
    {
        return behindColl2d.IsTouching(c2d);
    }
}
