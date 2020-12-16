using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerBattery : PowerConduit, IPowerer
{
    public override bool takesEnergy => false;
    public override bool convertsToEnergy => true;

    public float ThroughPut => maxEnergyPerSecond;

    public GameObject GameObject => gameObject;

    private OnPowerFlowed onPowerFlowed;
    public OnPowerFlowed OnPowerFlowed
    {
        get => onPowerFlowed;
        set => onPowerFlowed = value;
    }

    public float givePower(float requestedPower)
    {
        float maxAmount = maxEnergyPerSecond * Time.fixedDeltaTime;
        float amount = Mathf.Min(requestedPower, maxAmount, Energy);
        adjustEnergy(-amount);
        onPowerFlowed?.Invoke(Energy, maxEnergyLevel);
        return amount;
    }
}
