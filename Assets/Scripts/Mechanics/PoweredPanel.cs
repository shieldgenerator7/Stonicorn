using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoweredPanel : SavableMonoBehaviour, IPowerable
{
    public float maxEnergyPerSecond = 1;

    public float ThroughPut => maxEnergyPerSecond;
    public GameObject GameObject => gameObject;

    private OnPowerFlowed onPowerGiven;
    public OnPowerFlowed OnPowerFlowed
    {
        get => onPowerGiven;
        set => onPowerGiven = value;
    }
    public override SavableObject CurrentState
    {
        get => new SavableObject(this);
        set { }
    }
    public override void init()
    {
    }

    public float acceptPower(float power)
    {
        float maxEnergy = maxEnergyPerSecond * Time.fixedDeltaTime;
        float energyToUse = Mathf.Min(power, maxEnergy);
        onPowerGiven?.Invoke(energyToUse, maxEnergy);
        return power - energyToUse;
    }
}
