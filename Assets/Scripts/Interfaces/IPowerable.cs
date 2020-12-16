using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Receives power and does something with it
/// </summary>
public interface IPowerable : IPowerConduit
{
    /// <summary>
    /// Uses the power given and returns how much is left over
    /// </summary>
    /// <param name="power"></param>
    /// <returns></returns>
    float acceptPower(float power);
}
