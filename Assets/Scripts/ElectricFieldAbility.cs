using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElectricFieldAbility : PlayerAbility
{//2017-11-17: copied from ShieldBubbleAbility

    public GameObject shieldRangeIndicator;//prefab
    private TeleportRangeIndicatorUpdater sriu;//"shield range indicator updater"
    private GameObject srii;//"shield range indicator instance"
    private CircleCollider2D aoeCollider;//the collider that is used to determine which objects are in the electric field's area of effect
    public float maxRange = 2.5f;
    public float maxHoldTime = 1;//how long until the max range is reached after it begins charging
    public float maxSlowPercent = 0.10f;//the max percent of slowness applied to objects near the center of the field when the field has maxRange
    public float maxGravityDampening = 0.90f;//the max percent of gravity dampening applied to objects in the area of effect
    public float lastDisruptTime = 0;//the last time that something happened that disrupted the shield
    public float activationDelay = 2.0f;//how long after the last disruption the field can start regenerating

    private float range = 0;//the current range of the field

    public AudioClip shieldBubbleSound;

    private static List<ElectricFieldAbility> activeFields = new List<ElectricFieldAbility>();//active fields get put in this
    private static int activeFieldAmount = 0;//how many fields are currently active (because List.count is unreliable)

    protected override void Start()
    {
        base.Start();
        GetComponent<PlayerController>().onTeleport += processTeleport;
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
                    rb2d.velocity = rb2d.velocity * (1 - maxSlowPercent);
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
        if (srii == null)
        {
            srii = Instantiate(shieldRangeIndicator);
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
    }

    /// <summary>
    /// Returns the gravity dampening factor for the given object
    /// </summary>
    /// <param name="go"></param>
    public static float getGravityDampeningFactor(GameObject go)
    {
        float factor = 1;
        if (activeFields.Count > 0)
        {
            foreach (ElectricFieldAbility efa in activeFields)
            {
                if (efa != null)
                {
                    if (efa.aoeCollider.IsTouching(go.GetComponent<Collider2D>()))
                    {
                        factor *= (1 - efa.maxGravityDampening);
                    }
                }
            }
        }
        return factor;
    }
}
