﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoweredWallController : MonoBehaviour, IPowerable
{
    //2017-01-24: several things copied from FloatCubeController

    public float efficiency = 100;//how much force one unit of energy can generate
    public float maxEnergyPerSecond = 3;

    private Rigidbody2D rb;
    private Vector3 upDirection;//used to determine the up direction of the powered door

    public float ThroughPut => maxEnergyPerSecond;
    public GameObject GameObject => gameObject;

    private OnPowerFlowed onPowerGiven;
    public OnPowerFlowed OnPowerFlowed
    {
        get => onPowerGiven;
        set => onPowerGiven = value;
    }

    // Use this for initialization
    void Start()
    {
        upDirection = transform.up;
        rb = GetComponent<Rigidbody2D>();
    }

    public float acceptPower(float power)
    {
        float maxEnergy = maxEnergyPerSecond * Time.fixedDeltaTime;
        float energyToUse = Mathf.Min(power, maxEnergy);
        if (energyToUse > 0)
        {
            Vector3 forceVector = energyToUse * efficiency * 9.81f * upDirection;
            //Debug.DrawLine(transform.position, transform.position + forceVector, Color.green);
            rb.AddForce(forceVector);
        }
        onPowerGiven?.Invoke(energyToUse, maxEnergy);
        return power - energyToUse;
    }
}
