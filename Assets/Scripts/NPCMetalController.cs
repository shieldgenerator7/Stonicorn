using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCMetalController : NPCController, IPowerable
{

    public float powerConsumptionRate = 3.0f;

    private float lastPowerReturned;//the last amount of power it got

    public float ThroughPut => powerConsumptionRate;

    public OnPowerFlowed OnPowerFlowed { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

    public GameObject GameObject => gameObject;

    protected override void Start()
    {
        base.Start();
    }

    protected override bool greetOnlyOnce()
    {
        return false;
    }

    protected override bool canGreet()
    {
        return lastPowerReturned > 0;
    }

    protected override bool shouldStop()
    {
        return lastPowerReturned <= 0;
    }

    public float acceptPower(float power)
    {
        float maxPower = powerConsumptionRate * Time.fixedDeltaTime;
        lastPowerReturned = Mathf.Min(power, maxPower);
        return lastPowerReturned;
    }
}
