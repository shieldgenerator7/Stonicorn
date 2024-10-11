using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoweredMover : SavableMonoBehaviour, IPowerable
{
    public float maxEnergyPerSecond = 3;
    public float moveForce = 10;//magnitude
    public Vector2 moveVector;//direction, relative to self

    public Collider2D bumperColl;

    private Rigidbody2D rb2d;

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
        rb2d = GetComponent<Rigidbody2D>();
        //Error checking
        if (!bumperColl || !bumperColl.isTrigger)
        {
            Debug.LogError("PoweredMover.bumperColl requires a collider that is a trigger!", gameObject);
        }
    }

    public float acceptPower(float power)
    {
        float maxEnergy = maxEnergyPerSecond * Time.fixedDeltaTime;
        float energyToUse = Mathf.Min(power, maxEnergy);
        if (energyToUse > 0)
        {
            //Move self
            float speed = (energyToUse / maxEnergy) * moveForce;
            if (rb2d.linearVelocity.magnitude < 0.1f)
            {
                speed *= 2;
            }
            Vector3 forceVector = speed * transform.TransformDirection(moveVector);
            rb2d.AddForce(forceVector * rb2d.mass);
            if (rb2d.linearVelocity.magnitude > speed)
            {
                rb2d.linearVelocity = rb2d.linearVelocity.normalized * speed;
            }
        }
        onPowerGiven?.Invoke(energyToUse, maxEnergy);
        return power - energyToUse;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.isTrigger)
        {
            moveVector *= -1;
        }
    }
}
