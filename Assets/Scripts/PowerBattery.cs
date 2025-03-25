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
    public float maxEnergyPerSecondIn;
    public float maxEnergyPerSecondOut;

    public float ThroughPut => Mathf.Min(energy, maxEnergyPerSecondOut);

    public GameObject GameObject => gameObject;


    [AutoInitialize, SerializeField, HideInInspector]
    private Collider2D coll2d;
    public Collider2D Collider2D => coll2d;

    public override void init()
    {
    }

    private OnPowerFlowed onPowerFlowed;
    public OnPowerFlowed OnPowerFlowed
    {
        get => onPowerFlowed;
        set => onPowerFlowed = value;
    }

    public float givePower(float requestedPower)
    {
        float maxAmount = maxEnergyPerSecondOut * Time.fixedDeltaTime;
        float amount = Mathf.Min(requestedPower, maxAmount, energy);
        Energy -= amount;
        return amount;
    }

    public float acceptPower(float power)
    {
        float maxAmount = maxEnergyPerSecondIn * Time.fixedDeltaTime;
        float amount = Mathf.Min(power, maxAmount);
        Energy += amount;
        return power - amount;
    }

    static short key_energy = 0;
    public override SavableObject CurrentState
    {
        get => new SavableObject(this,
            key_energy, energy
            );
        set => Energy = value.Float(key_energy);
    }
}
