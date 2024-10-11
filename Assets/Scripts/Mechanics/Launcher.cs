using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Launcher : SavableMonoBehaviour, IPowerable
{
    public float maxEnergyPerSecond = 3;
    public float maxEnergyStore = 100;
    public float moveForce = 10;//magnitude
    public float energyEntropy = 1;
    public Vector2 moveVector;//direction, relative to self

    private float energyStored = 0;
    public float EnergyStored
    {
        get => energyStored;
        set
        {
            energyStored = Mathf.Clamp(value, 0, maxEnergyStore);
            onPowerGiven?.Invoke(energyStored, maxEnergyStore);
        }
    }

    public Collider2D launchColl;

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
        get => new SavableObject(this,
            "energyStored", energyStored
            );
        set
        {
            EnergyStored = value.Float("energyStored");
        }
    }
    public override void init()
    {
        //Error checking
        if (!launchColl || !launchColl.isTrigger)
        {
            Debug.LogError("Launcher.launchColl requires a collider that is a trigger!", gameObject);
        }
    }

    public float acceptPower(float power)
    {
        float maxEnergy = maxEnergyPerSecond * Time.fixedDeltaTime;
        float energyToUse = Mathf.Min(power, maxEnergy);
        if (energyToUse > 0)
        {
            //Store energy
            EnergyStored += energyToUse;
        }
        return power - energyToUse;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.isSolid() && energyStored > 0)
        {
            launch();
        }
    }

    private void launch()
    {
        //Move self
        float speed = (energyStored / maxEnergyStore) * moveForce;
        Vector2 direction = transform.TransformDirection(moveVector).normalized;
        Utility.RaycastAnswer answer = Utility.CastAnswer(launchColl, Vector2.zero, 0, true);
        for (int i = 0; i < answer.count; i++)
        {
            Rigidbody2D rb2d = answer.rch2ds[i].rigidbody;
            if (rb2d && answer.rch2ds[i].collider.isSolid())
            {
                Vector3 forceVector = speed * direction;
                rb2d.linearVelocity = forceVector;
            }
        }
        EnergyStored = 0;
    }

    private void FixedUpdate()
    {
        //Discharge energy based on how much energy is already stored
        EnergyStored -= energyEntropy * Time.deltaTime * energyStored / maxEnergyStore;
    }
}
