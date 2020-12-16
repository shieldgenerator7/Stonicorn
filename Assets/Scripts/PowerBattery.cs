using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerBattery : SavableMonoBehaviour, IPowerer
{

    [SerializeField]
    private float energy = 0;

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
        energy -= amount;
        onPowerFlowed?.Invoke(energy, maxEnergy);
        return amount;
    }

    public override SavableObject CurrentState
    {
        get => new SavableObject(this,
            "energy", energy
            );
        set => energy = value.Float("energy");
    }
}
