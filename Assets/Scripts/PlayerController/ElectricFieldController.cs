using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElectricFieldController : SavableMonoBehaviour, Blastable
{//2018-01-07: copied from ShieldBubbleController
    public float range = 3;//how big the shield is

    public float energy = 0;//how much energy this field has left
    public float energyToRangeRatio;//used to "convert" energy into range, this is equal to (maxRange / maxEnergy);, range = energy * energyToPowerRatio;
    public float energyToSlowRatio;//used to "convert" energy to momentum dampening on all objects in aoe, regardless of how close to the center

    //2018-01-07: copied from ElectricFieldAbility
    private TeleportRangeIndicatorUpdater friu;//"field range indicator updater"
    private CircleCollider2D aoeCollider;//the collider that is used to determine which objects are in the electric field's area of effect
    public float maxForceResistance = 500f;//if it gets this much force, it takes out the field, but it will come right back up

    private ParticleSystemController particleController;
    private Color effectColor;
    /// <summary>
    /// Used to determine which objects this field can power
    /// </summary>
    private RaycastHit2D[] rch2dsPowerable = new RaycastHit2D[Utility.MAX_HIT_COUNT];

    private void Start()
    {
        if (!friu)
        {
            init();
        }
    }

    public void init()
    {
        effectColor = GetComponent<SpriteRenderer>().color;
        effectColor.a = 1;
        friu = GetComponent<TeleportRangeIndicatorUpdater>();
        aoeCollider = GetComponent<CircleCollider2D>();
        particleController = GetComponentInChildren<ParticleSystemController>();
    }

    public override SavableObject getSavableObject()
    {
        return new SavableObject(this,
            "energy", energy,
            "energyToRangeRatio", energyToRangeRatio,
            "energyToSlowRatio", energyToSlowRatio,
            "maxForceResistance", maxForceResistance
            );
    }
    public override void acceptSavableObject(SavableObject savObj)
    {
        energy = (float)savObj.data["energy"];
        energyToRangeRatio = (float)savObj.data["energyToRangeRatio"];
        energyToSlowRatio = (float)savObj.data["energyToSlowRatio"];
        maxForceResistance = (float)savObj.data["maxForceResistance"];
        if (!friu)
        {
            init();
        }
        addEnergy(0);
    }
    public override bool isSpawnedObject()
    {
        return true;
    }
    public override string getPrefabName()
    {
        return "ElectricField";
    }

    void FixedUpdate()
    {
        //2017-01-24: copied from WeightSwitchActivator.FixedUpdate()
        int count = Utility.Cast(aoeCollider,Vector2.zero, rch2dsPowerable, 0);
        for (int i = 0; i < count; i++)
        {
            GameObject hc = rch2dsPowerable[i].collider.gameObject;

            //Power objects
            PowerConduit pc = hc.GetComponent<PowerConduit>();
            if (pc != null && pc.convertsToEnergy)
            {
                //2017-11-17 FUTURE CODE: take out the 100 and put a variable in there, perhaps something to do with HP
                float amountTaken = pc.convertSourceToEnergy(energy, Time.fixedDeltaTime);
                addEnergy(-amountTaken);
            }

            //Slow objects
            Rigidbody2D rb2d = hc.GetComponent<Rigidbody2D>();
            if (rb2d != null)
            {
                if (!Mathf.Approximately(rb2d.velocity.sqrMagnitude, 0))
                {
                    float dampening = energy * energyToSlowRatio;
                    dampening = Mathf.Max(0, dampening);
                    rb2d.velocity = rb2d.velocity * (1 - dampening);
                }
                else
                {
                    rb2d.velocity *= 0;
                }
            }
        }
        //Auto-drain energy
        addEnergy(-Mathf.Max(1, range) * Time.fixedDeltaTime);
    }

    public float checkForce(float force)
    {
        float energyLost = Mathf.Abs(energy * force / maxForceResistance);
        addEnergy(-energyLost);
        if (energy < 1)
        {
            dissipate();
        }
        return energyLost;
    }
    public float getDistanceFromExplosion(Vector2 explosionPos)
    {
        return Mathf.Max(0, Vector2.Distance(explosionPos, transform.position) - range);
    }
    public void addEnergy(float amount)
    {
        energy += amount;
        if (energy <= 0)
        {
            dissipate();
        }
        //Electric Field VC: change its sprite's size based on its energy
        if (!friu)
        {
            init();
        }
        range = energy * energyToRangeRatio;
        friu.setRange(range);
        //Particle effects
        particleController.activateTeleportParticleSystem(true, effectColor, transform.position, range);
    }

    void dissipate()
    {
        GameManager.destroyObject(gameObject);
    }
}