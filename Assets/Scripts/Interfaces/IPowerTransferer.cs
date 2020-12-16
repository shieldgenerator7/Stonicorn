using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPowerTransferer : IPowerConduit
{
    /// <summary>
    /// The max amount of power that can come through this transferer per second
    /// </summary>
    float ThroughPut
    {
        get;
    }

    /// <summary>
    /// Returns how much power was transferred
    /// </summary>
    /// <param name="power"></param>
    /// <returns></returns>
    float transferPower(float power);
}
