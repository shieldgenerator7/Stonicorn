using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForceDashAbility : PlayerAbility
{
    [Header("Settings")]
    public float maxCharge = 50;
    public float maxSpeed = 20;
    public float chargeIncrement = 2.5f;//how much to increase charge on each tap
    public float chargeIncrementEarly = 0.5f;//how much to increase charge by when there's no charge
    public float chargeEarlyThreshold = 1;//how much charge is needed to get the regular charge increment
    public float chargeDecayDelay = 1;//how may seconds after no tap until it decays
    public float chargeDecayRate = 25;//how much charge is lost per second

    [SerializeField]
    private float charge;
    public float Charge
    {
        get => charge;
        private set
        {
            charge = Mathf.Clamp(value, 0, maxCharge);
        }
    }
    private float lastChargeTime;

    private void Start()
    {
        
    }

    protected override void init()
    {
        base.init();
        playerController.onTeleport += chargeUp;
    }

    public override void OnDisable()
    {
        base.OnDisable();
        playerController.onTeleport -= chargeUp;
    }

    // Update is called once per frame
    void Update()
    {
        if (Charge > 0)
        {
            rb2d.AddForce(rb2d.velocity.normalized * (maxSpeed * Charge / maxCharge));
        }
        if (Time.time > lastChargeTime + chargeDecayDelay)
        {
            Charge -= chargeDecayRate * Time.deltaTime;
        }
    }

    public void chargeUp(Vector2 oldPos, Vector2 newPos)
    {
        Debug.Log("Charge up called!");
        float distance = Vector2.Distance(oldPos, newPos);
        if (Charge < chargeEarlyThreshold)
        {
            Charge += chargeIncrementEarly * distance / playerController.baseRange;
        }
        else
        {
            Charge += chargeIncrement * distance / playerController.baseRange;
        }
        rb2d.AddForce((newPos - oldPos).normalized * (maxSpeed * Charge / maxCharge));
        lastChargeTime = Time.time;
    }
}
