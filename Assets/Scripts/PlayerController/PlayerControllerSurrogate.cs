using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Used on projectiles to allow them to activate things for Merky
/// </summary>
public class PlayerControllerSurrogate : PlayerController
{
    private void Awake()
    {
        init();
    }

    protected override void registerDelegates()
    {
        //do nothing
    }

    protected override void OnCollisionEnter2D(Collision2D collision)
    {
        //do nothing
    }
}


