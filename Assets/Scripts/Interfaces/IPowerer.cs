using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gives power to the system
/// </summary>
public interface IPowerer : IPowerConduit
{
    /// <summary>
    /// Returns how much power it gave
    /// </summary>
    /// <param name="requestedPower">How much power it should give</param>
    /// <returns></returns>
    float givePower(float requestedPower);
}
