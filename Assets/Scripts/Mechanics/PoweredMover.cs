using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoweredMover : SavableMonoBehaviour, IPowerable
{
    public float maxEnergyPerSecond = 3;
    public float moveForce = 10;//magnitude
    public Vector2 startMoveVector = Vector2.left;
    private Vector2 moveVector;//direction, relative to self
    [Tooltip("How long it has to be off before flipping direction")]
    public float offDuration = 0.2f;

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
        get => new SavableObject(this,
            "moveVector", moveVector
            );
        set
        {
            moveVector = value.Vector2("moveVector");
        }
    }
    public override void init()
    {
        rb2d = GetComponent<Rigidbody2D>();

        //init
        moveVector = startMoveVector;
    }
    float prevPoweredTime = 0;
    private void Update()
    {
        if (prevPoweredTime > 0 && Managers.Time.Time >= prevPoweredTime + offDuration)
        {
            prevPoweredTime = 0;
            flipDirection();
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

            //
            prevPoweredTime = Managers.Time.Time;
        }
        onPowerGiven?.Invoke(energyToUse, maxEnergy);
        return power - energyToUse;
    }

    void flipDirection()
    {
        moveVector *= -1;
    }
}
