using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElectricFieldAbility : PlayerAbility
{//2017-11-17: copied from ShieldBubbleAbility

    public GameObject shieldRangeIndicator;//prefab
    private TeleportRangeIndicatorUpdater sriu;//"shield range indicator updater"
    private GameObject srii;//"shield range indicator instance"
    public float maxRange = 2.5f;
    public float maxHoldTime = 1;//how long until the max range is reached after it begins charging
    public float maxSlowPercent = 0.90f;//the max percent of slowness applied to objects near the center of the field when the field has maxRange
    public float lastDisruptTime = 0;//the last time that something happened that disrupted the shield
    public float activationDelay = 2.0f;//how long after the last disruption the field can start regenerating

    private float range = 0;//the current range of the field

    public AudioClip shieldBubbleSound;

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
        if (Time.time > lastDisruptTime + activationDelay)
        {
            //2017-01-24: copied from WeightSwitchActivator.FixedUpdate()
            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, range);
            for (int i = 0; i < hitColliders.Length; i++)
            {
                GameObject hc = hitColliders[i].gameObject;

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
            srii.GetComponent<SpriteRenderer>().enabled = false;
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
        }
        particleController.activateTeleportParticleSystem(false);
    }

    public void processTeleport(Vector2 oldPos, Vector2 newPos)
    {
        dropWaitGesture();
    }
}
