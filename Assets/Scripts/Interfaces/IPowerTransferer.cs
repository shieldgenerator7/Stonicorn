using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPowerTransferer : IPowerConduit
{
    /// <summary>
    /// Returns how much power was transferred
    /// </summary>
    /// <param name="power"></param>
    /// <returns></returns>
    float transferPower(float power);
}
