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
}


