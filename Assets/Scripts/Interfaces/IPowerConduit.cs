using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void OnPowerFlowed(float power, float maxPower);

/// <summary>
/// Sends or receives power
/// Meant to be processed in FixedUpdate() only
/// </summary>
public interface IPowerConduit
{
    /// <summary>
    /// The max amount of power that can 
    /// come into or out of this conduit per second
    /// </summary>
    float ThroughPut
    {
        get;
    }

    OnPowerFlowed OnPowerFlowed
    {
        get;
        set;
    }

    GameObject GameObject
    {
        get;
    }
}
