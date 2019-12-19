using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForceDashAbility : PlayerAbility
{
    [Header("Settings")]
    public float maxCharge = 50;
    public float maxSpeed = 20;
    public float maxEffectRange = 5;
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
            if (Mathf.Approximately(charge, 0))
            {
                chargeDirection = Vector2.zero;
            }
            showChargeEffect();
        }
    }

    private Vector2 chargeDirection;
    public Vector2 ChargeDirection
    {
        get => chargeDirection;
        set => chargeDirection = value.normalized;
    }

    public float EffectRange
    {
        get => maxEffectRange * Charge / maxCharge;
    }

    [Header("Components")]
    public GameObject forceRangeIndicator;//prefab
    private TeleportRangeIndicatorUpdater friu;//"force range indicator updater"

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

    private void FixedUpdate()
    {
        if (Charge > 0)
        {
            rb2d.AddForce(chargeDirection * (maxSpeed * Charge / maxCharge));
        }
        if (Time.time > lastChargeTime + chargeDecayDelay)
        {
            Charge -= chargeDecayRate * Time.deltaTime;
        }
    }

    public void chargeUp(Vector2 oldPos, Vector2 newPos)
    {
        Vector2 direction = newPos - oldPos;
        float distance = direction.magnitude;
        if (Charge < chargeEarlyThreshold)
        {
            Charge += chargeIncrementEarly * distance / playerController.baseRange;
        }
        else
        {
            Charge += chargeIncrement * distance / playerController.baseRange;
        }
        Vector2 dirNorm = direction.normalized;
        ChargeDirection += dirNorm;
        rb2d.AddForce(dirNorm * (maxSpeed * Charge / maxCharge));
        lastChargeTime = Time.time;

        //If tapping opposite velocity direction,
        Vector2 velocity = playerController.Velocity;
        float angle = Vector2.Angle(direction, velocity);
        if (angle > 90)
        {
            //reduce charge level.
            float nullifyPercent = (170 - angle) / 90;
            Charge *= nullifyPercent;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!Active)
        {
            return;
        }
        //If collision is head on,
        Vector2 velocity = playerController.Velocity;
        Vector2 surfaceNormal = collision.GetContact(0).normal;
        float angle = Vector2.Angle(-velocity, surfaceNormal);
        if (angle < 45)
        {
            Charge = 0;
        }
        //Else
        else
        {
            //Divert Merky's course
            //If should rotate "left"
            if (Vector2.SignedAngle(-velocity, surfaceNormal) < 0)
            {
                ChargeDirection = surfaceNormal.normalized.PerpendicularRight();
            }
            //Else should rotate "right"
            else
            {
                ChargeDirection = surfaceNormal.normalized.PerpendicularLeft();
            }
        }
    }

    void showChargeEffect()
    {
        float range = EffectRange;
        if (range > 0)
        {
            if (friu == null)
            {
                GameObject frii = Instantiate(forceRangeIndicator);
                friu = frii.GetComponent<TeleportRangeIndicatorUpdater>();
                frii.GetComponent<SpriteRenderer>().enabled = false;
                frii.transform.position = (Vector2)transform.position;
                frii.transform.parent = transform;
            }
            friu.gameObject.SetActive(true);
            friu.setRange(range);
        }
        else
        {
            friu.gameObject.SetActive(false);
        }
    }
}
