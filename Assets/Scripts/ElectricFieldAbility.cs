using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElectricFieldAbility : PlayerAbility
{//2017-11-17: copied from ShieldBubbleAbility

    public GameObject electricFieldRangeIndicator;//prefab
    private TeleportRangeIndicatorUpdater sriu;//"shield range indicator updater"
    private GameObject srii;//"shield range indicator instance"
    private CircleCollider2D aoeCollider;//the collider that is used to determine which objects are in the electric field's area of effect
    public float maxRange = 2.5f;
    public float maxHoldTime = 1;//how long until the max range is reached after it begins charging
    public float maxSlowPercent = 0.10f;//the max percent of slowness applied to objects near the center of the field when the field has maxRange
    public float maxGravityDampening = 0.90f;//the max percent of gravity dampening applied to objects in the area of effect
    public float lastDisruptTime = 0;//the last time that something happened that disrupted the shield
    public float baseActivationDelay = 2.0f;//how long after the last disruption the field can start regenerating
    public float maxForceResistance = 500f;//if it gets this much force, it takes out the field, but it will come right back up

    private float range = 0;//the current range of the field
    private float activationDelay = 2.0f;//how long it will wait, usually set to the base delay

    public AudioClip shieldBubbleSound;
    private PlayerController playerController;//for if this script is on Merky

    private static List<ElectricFieldAbility> activeFields = new List<ElectricFieldAbility>();//active fields get put in this
    private static int activeFieldAmount = 0;//how many fields are currently active (because List.count is unreliable)

    protected override void Start()
    {
        base.Start();
        playerController = GetComponent<PlayerController>();
        if (playerController)
        {
            playerController.onTeleport += processTeleport;
        }
        lastDisruptTime = Time.time;
    }

    void Update()
    {
        if (Time.time > lastDisruptTime + activationDelay)
        {
            processWaitGesture(Time.time - (lastDisruptTime + activationDelay));
        }
    }

    void FixedUpdate()
    {
        if (Time.time > lastDisruptTime + activationDelay && srii != null)
        {
            //2017-01-24: copied from WeightSwitchActivator.FixedUpdate()
            RaycastHit2D[] rch2ds = new RaycastHit2D[10];
            int count = aoeCollider.Cast(Vector2.zero, rch2ds, 0);
            for (int i = 0; i < count; i++)
            {
                GameObject hc = rch2ds[i].collider.gameObject;

                PowerConduit pc = hc.GetComponent<PowerConduit>();
                if (pc != null && pc.convertsToEnergy)
                {
                    //2017-11-17 FUTURE CODE: take out the 100 and put a variable in there, perhaps something to do with HP
                    float amountTaken = pc.convertSourceToEnergy(100, Time.fixedDeltaTime);
                }

                Rigidbody2D rb2d = hc.GetComponent<Rigidbody2D>();
                if (rb2d != null)
                {
                    if (rb2d.velocity.sqrMagnitude > 0.1f)
                    {
                        //Less Expensive Attempt
                        float distance = Vector3.Distance(transform.position, hc.transform.position);
                        //More Expensive Attempt
                        if (distance > range)
                        {
                            distance = Utility.distanceToObject(transform.position, hc);
                        }
                        float dampening = maxSlowPercent * (range - distance) / maxRange;
                        dampening = Mathf.Max(0, dampening);
                        rb2d.velocity = rb2d.velocity * (1 - dampening);
                    }
                    else
                    {
                        rb2d.velocity *= 0;
                    }
                }
            }
        }
    }

    public void processWaitGesture(float waitTime)
    {
        range = maxRange * waitTime * GestureManager.holdTimeScaleRecip / maxHoldTime;
        if (range > maxRange)
        {
            range = maxRange;
        }
        if (playerController)
        {
            if (playerController.range < range + 0.5f)
            {
                playerController.setRange(range + 0.5f);
            }
        }
        if (srii == null)
        {
            srii = Instantiate(electricFieldRangeIndicator);
            sriu = srii.GetComponent<TeleportRangeIndicatorUpdater>();
            aoeCollider = srii.GetComponent<CircleCollider2D>();
            srii.GetComponent<SpriteRenderer>().enabled = false;
            activeFields.Add(this);
            activeFieldAmount++;
        }
        srii.transform.position = transform.position;
        sriu.setRange(range);
        //Particle effects
        particleController.activateTeleportParticleSystem(true, effectColor, transform.position, range);
    }

    public void dropWaitGesture()
    {
        lastDisruptTime = Time.time;
        range = 0;

        if (srii != null)
        {
            Destroy(srii);
            srii = null;
            activeFields.Remove(this);
            activeFieldAmount--;
        }
        particleController.activateTeleportParticleSystem(false);
    }

    public void processTeleport(Vector2 oldPos, Vector2 newPos)
    {
        dropWaitGesture();
        float distance = Vector3.Distance(oldPos, newPos);
        activationDelay = baseActivationDelay * distance / playerController.baseRange;
    }

    /// <summary>
    /// Returns the gravity dampening factor for the given object
    /// </summary>
    /// <param name="go"></param>
    public static float getGravityDampeningFactor(GameObject go)
    {
        float factor = 1;
        if (activeFieldAmount > 0)
        {
            foreach (ElectricFieldAbility efa in activeFields)
            {
                if (efa != null)
                {
                    if (efa.aoeCollider.IsTouching(go.GetComponent<Collider2D>()))
                    {
                        //Less Expensive Attempt
                        float distance = Vector3.Distance(efa.transform.position, go.transform.position);
                        //More Expensive Attempt
                        if (distance > efa.range)
                        {
                            distance = Utility.distanceToObject(efa.transform.position, go);
                        }
                        float dampening = efa.maxGravityDampening * (efa.range - distance) / efa.maxRange;
                        dampening = Mathf.Max(0, dampening);
                        factor *= (1 - dampening);
                    }
                }
            }
        }
        return factor;
    }

    public void checkForce(float force)
    {
        float addedDelay = maxHoldTime * force / maxForceResistance;
        lastDisruptTime = Time.time + addedDelay - maxHoldTime;
        if (force >= maxForceResistance)
        {
            dropWaitGesture();
        }
    }
}
