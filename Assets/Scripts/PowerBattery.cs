using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerBattery : PowerConduit
{
    public override bool takesEnergy => false;
    public override bool convertsToEnergy => true;
}
