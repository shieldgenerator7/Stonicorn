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
    public float stabilizationPower = 0;//how good it is at keeping itself upright
    public bool stopOnPowerLost = false;//if true, itll reset all momentum when it loses power
    public bool flipDirectionOnPowerLost = true;//if true, itll switch movement direction when it loses power

    private Vector2 gravityCenter = Vector2.zero;//TODO: make this rely on a gravity zone to find

    [AutoInitialize, SerializeField, HideInInspector]
    private Rigidbody2D rb2d;
    [AutoInitialize, SerializeField, HideInInspector]
    private Collider2D coll2d;
    public Collider2D Collider2D => coll2d;


    public float ThroughPut => maxEnergyPerSecond;
    public GameObject GameObject => gameObject;

    private OnPowerFlowed onPowerGiven;
    public OnPowerFlowed OnPowerFlowed
    {
        get => onPowerGiven;
        set => onPowerGiven = value;
    }
    static byte key_moveVector = 0;
    public override SavableObject CurrentState
    {
        get => new SavableObject(this,
            key_moveVector, moveVector
            );
        set
        {
            moveVector = value.Vector2(key_moveVector);
        }
    }
    public override void init()
    {

        //init
        moveVector = startMoveVector;
    }
    float prevPoweredTime = 0;
    private void Update()
    {
        if (prevPoweredTime > 0 && Managers.Time.Time >= prevPoweredTime + offDuration)
        {
            prevPoweredTime = 0;
            if (flipDirectionOnPowerLost)
            {
            flipDirection();
            }
            if (stopOnPowerLost)
            {
                rb2d.linearVelocity = Vector2.zero;
                rb2d.angularVelocity = 0;
            }
        }
    }

    public float acceptPower(float power)
    {
        float maxEnergy = maxEnergyPerSecond * Time.fixedDeltaTime;
        float energyToUse = Mathf.Min(power, maxEnergy);
        if (energyToUse > 0)
        {
            float energyPercent = (energyToUse / maxEnergy);

            //Stabilize self
            if (stabilizationPower > 0)
            {
                Vector2 up = ((Vector2)transform.position - gravityCenter).normalized;
                transform.up = Vector2.Lerp(transform.up, up, stabilizationPower * Time.fixedDeltaTime * energyPercent);
            }

            //Move self
            float speed = energyPercent * moveForce;
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
