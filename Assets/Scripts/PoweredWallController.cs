using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoweredWallController : MonoBehaviour
{
    //2017-01-24: several things copied from FloatCubeController

    public float efficiency = 100;//how much force one unit of energy can generate

    private Rigidbody2D rb;
    private Vector3 upDirection;//used to determine the up direction of the powered door

    private PowerConduit pc;

    // Use this for initialization
    void Start()
    {
        upDirection = transform.up;
        rb = GetComponent<Rigidbody2D>();
        pc = GetComponent<PowerConduit>();
    }

    void FixedUpdate()
    {
        if (pc.Energy > 0)
        {
            float energyToUse = pc.useEnergy(pc.maxEnergyPerSecond, Time.fixedDeltaTime);
            if (energyToUse > 0)
            {
                Vector3 forceVector = energyToUse * efficiency * 9.81f * upDirection;
                Debug.DrawLine(transform.position, transform.position + forceVector, Color.green);
                rb.AddForce(forceVector);
            }
        }
    }
}
