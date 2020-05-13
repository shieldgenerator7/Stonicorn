using UnityEngine;

public class ElectricFieldAbility : PlayerAbility
{//2017-11-17: copied from ShieldBubbleAbility

    public GameObject electricFieldPrefab;//prefab
    public float maxRange = 2.5f;
    public float maxEnergy = 100;//not the maximum for the player's electric fields
    public float maxChargeTime = 1;//how long until the max range is reached after it begins charging
    public float maxSlowPercent = 0.10f;//the percent of slowness applied to objects in the field when the field has maxRange
    public float maxForceResistance = 500f;//how much force is required to deal 100% damage to the field at max range

    private GameObject currentElectricField;
    private ElectricFieldController cEFController;//"current Electric Field Controller"

    private bool activated = false;//true if Merky is currently using this ability
    private float playerTeleportRangeDiff;//the difference between the player's max teleport range and this EFA's max field range (if on the player)
    private bool newlyCreatedEF = false;//true if the current electric field is one that was just created by Merky

    public AudioClip shieldBubbleSound;
    /// <summary>
    /// Used to find the current electric field
    /// </summary>
    private RaycastHit2D[] rch2dsWait = new RaycastHit2D[Utility.MAX_HIT_COUNT];

    protected override void init()
    {
        base.init();
        if (playerController)
        {
            playerController.onPreTeleport += processTeleport;
            playerTeleportRangeDiff = playerController.baseRange - maxRange;
        }
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
        if (!Managers.Game.Rewinding)
        {
            //If activation delay has ended,
            if (activated)
            {
                //Make a field
                chargeField();
            }
        }
        else
        {
            dropField();
        }
    }

    public void chargeField()
    {
        //If he's not charging one yet,
        if (currentElectricField == null)
        {
            //Find one that he's currently in
            Collider2D coll2d = GetComponent<Collider2D>();
            int collCount = Utility.Cast(coll2d, Vector2.zero, rch2dsWait);
            for (int i = 0; i < collCount; i++)
            {
                ElectricFieldController efc = rch2dsWait[i].collider.gameObject.GetComponent<ElectricFieldController>();
                if (efc)
                {
                    currentElectricField = efc.gameObject;
                    cEFController = efc;
                    break;
                }
            }
            //If he's not in one already,
            if (currentElectricField == null)
            {
                //Create a new one
                currentElectricField = Utility.Instantiate(electricFieldPrefab);
                cEFController = currentElectricField.GetComponent<ElectricFieldController>();
                cEFController.energyToRangeRatio = maxRange / maxEnergy;
                cEFController.energyToSlowRatio = maxSlowPercent / maxEnergy;
                cEFController.maxForceResistance = maxForceResistance;
                newlyCreatedEF = true;
                //Update Stats
                GameStatistics.addOne("ElectricFieldField");
            }
        }
        //If the one he's charging was created by him (and not been dropped yet)
        if (newlyCreatedEF)
        {
            //Make it follow him
            currentElectricField.transform.position = transform.position;
        }
        //Charge the field
        float energyToAdd = Time.deltaTime * maxEnergy / maxChargeTime;
        cEFController.addEnergy(energyToAdd);
        //Keep the field from growing past Merky's range size
        if (playerController)
        {
            float maxAllowedRange = playerController.Range - playerTeleportRangeDiff;
            if (cEFController.range > maxAllowedRange)
            {
                cEFController.addEnergy(-energyToAdd);
            }
        }
        else if (cEFController.energy > maxEnergy)
        {
            cEFController.energy = maxEnergy;
        }
    }

    public void dropField()
    {
        currentElectricField = null;
        cEFController = null;
        newlyCreatedEF = false;
    }

    public void processTeleport(Vector2 oldPos, Vector2 newPos, Vector2 triedPos)
    {
        //If player tapped on Merky,
        if (playerController.gestureOnPlayer(triedPos))
        {
            //If not activated,
            if (!activated)
            {
                //Activate
                activated = true;
                //Update Stats
                GameStatistics.addOne("ElectricField");
            }
            //Else,
            else
            {
                //Deactivate or detach
                activated = false;
                dropField();
            }
        }
    }
}
