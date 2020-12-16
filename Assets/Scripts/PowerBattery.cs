using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerBattery : SavableMonoBehaviour, IPowerer, IPowerable
{

    [SerializeField]
    private float energy = 0;
    public float Energy
    {
        get => energy;
        set
        {
            energy = Mathf.Clamp(value, 0, maxEnergy);
            onPowerFlowed?.Invoke(energy, maxEnergy);
        }
    }

    public float maxEnergy;
    public float maxEnergyPerSecond;
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
        float amount = Mathf.Min(requestedPower, maxAmount, energy);
        Energy -= amount;
        return amount;
    }

    public float acceptPower(float power)
    {
        float maxAmount = maxEnergyPerSecond * Time.fixedDeltaTime;
        float amount = Mathf.Min(power, maxAmount);
        Energy += amount;
        return amount;
    }

    public override SavableObject CurrentState
    {
        get => new SavableObject(this,
            "energy", energy
            );
        set => Energy = value.Float("energy");
    }
}
