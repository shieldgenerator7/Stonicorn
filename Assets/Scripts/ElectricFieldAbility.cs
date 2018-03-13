using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElectricFieldAbility : PlayerAbility, Blastable
{//2017-11-17: copied from ShieldBubbleAbility

    public GameObject electricFieldPrefab;//prefab
    public float maxRange = 2.5f;
    public float maxEnergy = 100;//not the maximum for the player's electric fields
    public float maxHoldTime = 1;//how long until the max range is reached after it begins charging
    public float maxSlowPercent = 0.10f;//the max percent of slowness applied to objects near the center of the field when the field has maxRange
    public float lastDisruptTime = 0;//the last time that something happened that disrupted the shield
    public float baseActivationDelay = 2.0f;//how long after the last disruption the field can start regenerating
    public float maxForceResistance = 500f;//if it gets this much force, it takes out the field, but it will come right back up

    private GameObject currentElectricField;
    private ElectricFieldController cEFController;//"current Electric Field Controller"

    private float activationDelay = 2.0f;//how long it will wait, usually set to the base delay
    private float playerTeleportRangeDiff;//the difference between the player's max teleport range and this EFA's max field range (if on the player)

    public AudioClip shieldBubbleSound;

    protected override void init()
    {
        base.init();
        if (playerController)
        {
            playerController.onPreTeleport += processTeleport;
            playerTeleportRangeDiff = playerController.Range - maxRange;
        }
        lastDisruptTime = Time.time;
    }
    public override void OnDisable()
    {
        base.OnDisable();
        if (playerController)
        {
            playerController.onPreTeleport -= processTeleport;
        }
    }

    void Update()
    {
        if (!GameManager.isRewinding())
        {
            if (Time.time > lastDisruptTime + activationDelay)
            {
                processWaitGesture(Time.time - (lastDisruptTime + activationDelay));
            }
        }
        else
        {
            dropWaitGesture();
        }
    }

    public void processWaitGesture(float waitTime)
    {
        if (currentElectricField == null)
        {
            //Find one that he's currently in
            Collider2D coll2d = GetComponent<Collider2D>();
            RaycastHit2D[] rch2ds = new RaycastHit2D[100];
            int collCount = coll2d.Cast(Vector2.zero, rch2ds);
            for (int i = 0; i < collCount; i++)
            {
                ElectricFieldController efc = rch2ds[i].collider.gameObject.GetComponent<ElectricFieldController>();
                if (efc)
                {
                    currentElectricField = efc.gameObject;
                    cEFController = efc;
                    break;
                }
            }
            if (currentElectricField == null)
            {
                //Create a new one
                currentElectricField = Utility.Instantiate(electricFieldPrefab);
                currentElectricField.transform.position = transform.position;
                cEFController = currentElectricField.GetComponent<ElectricFieldController>();
                cEFController.energyToRangeRatio = maxRange / maxEnergy;
                cEFController.energyToSlowRatio = maxSlowPercent / maxEnergy;
                cEFController.maxForceResistance = maxForceResistance;
            }
        }
        float energyToAdd = Time.deltaTime * maxEnergy / maxHoldTime;
        cEFController.addEnergy(energyToAdd);
        if (playerController)
        {
            float distance = Vector2.Distance(
                currentElectricField.transform.position,
                transform.position
                );
            float maxAllowedRange = distance
                + Mathf.Max(playerController.Range, playerController.baseRange)
                - playerTeleportRangeDiff;
            if (cEFController.range > maxAllowedRange)
            {
                cEFController.addEnergy(-energyToAdd);
            }
            float minAllowedTeleportRange = cEFController.range + playerTeleportRangeDiff - distance;
            if (playerController.Range < minAllowedTeleportRange)
            {
                playerController.Range = minAllowedTeleportRange;
            }
        }
        else if (cEFController.energy > maxEnergy)
        {
            cEFController.energy = maxEnergy;
        }
    }

    public void dropWaitGesture()
    {
        lastDisruptTime = Time.time;

        currentElectricField = null;
        cEFController = null;
    }

    public bool processTeleport(Vector2 oldPos, Vector2 newPos, Vector2 triedPos)
    {
        dropWaitGesture();
        float distance = Vector3.Distance(oldPos, triedPos);
        activationDelay = baseActivationDelay * distance / playerController.baseRange;
        return true;
    }

    public float checkForce(float force)
    {
        float addedDelay = maxHoldTime * force / maxForceResistance;
        lastDisruptTime = Time.time + addedDelay - maxHoldTime;
        if (force >= maxForceResistance)
        {
            dropWaitGesture();
        }
        return addedDelay;
    }
    public float getDistanceFromExplosion(Vector2 explosionPos)
    {
        return Utility.distanceToObject(explosionPos, gameObject);
    }
}
