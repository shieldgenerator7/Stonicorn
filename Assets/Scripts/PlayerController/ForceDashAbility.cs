using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForceDashAbility : PlayerAbility
{
    [Header("Settings")]
    public float maxCharge = 100;
    public float maxSpeed = 25;//this script may not cause Merky to exceed this speed
    public float maxEffectRange = 3;
    public float maxAirTime = 0.5f;//how long it can add force in the air
    public float chargeIncrement = 10;//how much to increase charge on each tap
    public float chargeIncrementEarly = 1;//how much to increase charge by when there's no charge
    public float chargeEarlyThreshold = 4;//how much charge is needed to get the regular charge increment
    public float chargeDecayDelay = 2;//how may seconds after no tap until it decays
    public float chargeDecayRate = 10;//how much charge is lost per second
    //Level 1
    public float wakeLength = 5;//objects this far behind Merky get sped up too

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
        playerController.Ground.isGroundedCheck += dashGroundedCheck;
    }

    public override void OnDisable()
    {
        base.OnDisable();
        playerController.onTeleport -= chargeUp;
        playerController.Ground.isGroundedCheck -= dashGroundedCheck;
    }

    private void FixedUpdate()
    {
        if (!Active)
        {
            return;
        }
        if (Charge >= chargeEarlyThreshold
            && (playerController.Ground.Grounded || Managers.Time.Time - playerController.Ground.LastGroundedTime < maxAirTime)
            )
        {
            //Speed up Merky
            speedUp(rb2d);
            //Speed up objects caught in Merky's wake
            if (true)//getLevel(1))
            {
                Utility.RaycastAnswer answer;
                //Front
                answer = Utility.RaycastAll(transform.position, ChargeDirection, wakeLength);
                for (int i = 0; i < answer.count; i++)
                {
                    RaycastHit2D rch2d = answer.rch2ds[i];
                    if (rch2d.rigidbody && rch2d.rigidbody != rb2d)
                    {
                        speedUp(rch2d.rigidbody);
                    }
                }
                //Back
                answer = Utility.RaycastAll(transform.position, -ChargeDirection, wakeLength);
                for (int i = 0; i < answer.count; i++)
                {
                    RaycastHit2D rch2d = answer.rch2ds[i];
                    if (rch2d.rigidbody && rch2d.rigidbody != rb2d)
                    {
                        speedUp(rch2d.rigidbody);
                    }
                }
            }
            //Adjust charge direction to align with velocity over time
            ChargeDirection = ChargeDirection * (maxSpeed * Charge / maxCharge) + rb2d.velocity;
        }
        if (Time.time > lastChargeTime + chargeDecayDelay)
        {
            Charge -= chargeDecayRate * Time.deltaTime;
        }
    }

    public void chargeUp(Vector2 oldPos, Vector2 newPos)
    {
        Vector2 direction = newPos - oldPos;
        Vector2 dirNorm = direction.normalized;
        if (shouldNegateCharge(dirNorm))
        {
            negateCharge(dirNorm);
        }
        else
        {
            //If the player was grounded before teleporting,
            if (playerController.Ground.GroundedPrev)
            {
                //Update the charge direction
                ChargeDirection += dirNorm;
                float distance = direction.magnitude;
                //If charge is just starting out,
                if (Charge < chargeEarlyThreshold)
                {
                    //Use the lower charge increment
                    Charge += chargeIncrementEarly * Mathf.Min(1, distance / playerController.baseRange);
                }
                else
                {
                    //Else, use the higher charge increment and
                    Charge += chargeIncrement * distance / playerController.baseRange;
                }
                //Reset decay delay
                lastChargeTime = Time.time;
            }
        }
    }

    /// <summary>
    /// Speeds up the given Rigidbody2D based on this ability's current state
    /// </summary>
    /// <param name="rb2d"></param>
    public void speedUp(Rigidbody2D rb2d)
    {
        float oldSpeed = rb2d.velocity.magnitude;
        //Add force in the charge direction
        rb2d.AddForce(chargeDirection * (rb2d.mass * maxSpeed * Charge / maxCharge));
        //Reduce speed if too high
        float newSpeed = rb2d.velocity.magnitude;
        if (newSpeed > maxSpeed)
        {
            rb2d.velocity = rb2d.velocity.normalized * Mathf.Max(oldSpeed, maxSpeed);
        }
    }

    private bool shouldNegateCharge(Vector2 direction)
    {
        float angle = Vector2.Angle(direction, ChargeDirection);
        return (angle > 135);
    }

    private void negateCharge(Vector2 direction)
    {
        //If tapping opposite velocity direction,
        float angle = Vector2.Angle(direction, chargeDirection);
        if (angle > 90)
        {
            //reduce charge level.
            float nullifyPercent = Mathf.Clamp(
                (170 - angle) / 90,
                0,
                1
                );
            Charge *= nullifyPercent;
        }
    }

    bool dashGroundedCheck()
    {
        return Charge > 0 && playerController.Ground.isGroundedInDirection(ChargeDirection);
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
        //If it cant bounce and the hit object is not movable,
        if (angle < 45 && !collision.gameObject.GetComponent<Rigidbody2D>())
        {
            //If explode isn't active,
            if (!true)//getLevel(2))
            {
                //don't do anything
                return;
            }
            //Explode     
            Vector2 explodePos;
            //If object is breakable and not movable,
            if (collision.gameObject.isSavable())
            {
                //explode behind Merky
                explodePos = (Vector2)transform.position - (velocity.normalized * 0.01f);
            }
            //Else
            else
            {
                //explode in front of Merky  
                explodePos = (Vector2)transform.position - (Vector2.Reflect(velocity, surfaceNormal).normalized * 0.01f);
            }
            Vector2 dir = ((Vector2)transform.position - explodePos).normalized;
            float charge = Charge;
            //doExplosionEffect(explodePos, Mathf.Max(charge, chargeIncrement), true);
            //dropHoldGesture();
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
            //Particle effects
            //effectParticleController.activateTeleportParticleSystem(true, effectColor, transform.position, range);
        }
        else
        {
            friu?.gameObject.SetActive(false);
            //effectParticleController.activateTeleportParticleSystem(false);
        }
    }

    public override SavableObject getSavableObject()
    {
        SavableObject so = base.getSavableObject();
        so.data.Add("charge", Charge);
        so.data.Add("chargeDirection", ChargeDirection);
        return so;
    }

    public override void acceptSavableObject(SavableObject savObj)
    {
        base.acceptSavableObject(savObj);
        Charge = (float)savObj.data["charge"];
        ChargeDirection = (Vector2)savObj.data["chargeDirection"];
    }

    protected override void acceptUpgradeLevel(AbilityUpgradeLevel aul)
    {
        throw new System.NotImplementedException();
    }
}
