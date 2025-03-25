using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElectricFieldController : SavableMonoBehaviour, IBlastable
{//2018-01-07: copied from ShieldBubbleController
    public float range = 3;//how big the shield is

    public float energy = 0;//how much energy this field has left
    public float energyToRangeRatio;//used to "convert" energy into range, this is equal to (maxRange / maxEnergy);, range = energy * energyToPowerRatio;
    public float energyToSlowRatio;//used to "convert" energy to momentum dampening on all objects in aoe, regardless of how close to the center

    //2018-01-07: copied from ElectricFieldAbility
    [AutoInitialize, SerializeField, HideInInspector]
    private TeleportRangeIndicatorUpdater friu;//"field range indicator updater"
    [AutoInitialize, SerializeField, HideInInspector]
    private CircleCollider2D aoeCollider;//the collider that is used to determine which objects are in the electric field's area of effect
    public float maxForceResistance = 500f;//if it gets this much force, it takes out the field, but it will come right back up

    [AutoInitialize(SearchChildren =true), SerializeField, HideInInspector]
    private ParticleSystemController particleController;

    [AutoInitialize, SerializeField, HideInInspector]
    private SpriteRenderer sr;
    [SerializeField]
    private Color effectColor;
    /// <summary>
    /// Used to determine which objects this field can power
    /// </summary>
    private RaycastHit2D[] rch2dsPowerable = new RaycastHit2D[Utility.MAX_HIT_COUNT];

    public override void init(){}

    static byte key_energy = 0;
    static byte key_energyToRangeRatio = 1;
    static byte key_energyToSlowRatio = 2;
    static byte key_maxForceResistance = 3;
    public override SavableObject CurrentState
    {
        get => new SavableObject(this,
            key_energy, energy,
            key_energyToRangeRatio, energyToRangeRatio,
            key_energyToSlowRatio, energyToSlowRatio,
            key_maxForceResistance, maxForceResistance
            );
        set
        {
            energy = value.Float(key_energy);
            energyToRangeRatio = value.Float(key_energyToRangeRatio);
            energyToSlowRatio = value.Float(key_energyToSlowRatio);
            maxForceResistance = value.Float(key_maxForceResistance);
            addEnergy(0);
        }
    }

    void FixedUpdate()
    {
        //2017-01-24: copied from WeightSwitchActivator.FixedUpdate()
        int count = Utility.Cast(aoeCollider, Vector2.zero, rch2dsPowerable, 0);
        for (int i = 0; i < count; i++)
        {
            GameObject hc = rch2dsPowerable[i].collider.gameObject;

            //Power objects
            IPowerable pwr = hc.GetComponent<IPowerable>();
            if (pwr != null)
            {
                float amountTaken = pwr.acceptPower(energy * Time.fixedDeltaTime);
                addEnergy(-amountTaken);
            }

            //Slow objects
            Rigidbody2D rb2d = hc.GetComponent<Rigidbody2D>();
            if (rb2d != null)
            {
                if (rb2d.isMoving())
                {
                    float dampening = energy * energyToSlowRatio;
                    dampening = Mathf.Max(0, dampening);
                    rb2d.linearVelocity = rb2d.linearVelocity * (1 - dampening);
                }
                else
                {
                    rb2d.nullifyMovement();
                }
            }
        }
    }

    public float checkForce(float force, Vector2 direction)
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
    public void addEnergy(float amount, float maxEnergy = 0)
    {
        energy += amount;
        if (maxEnergy > 0)
        {
            energy = Mathf.Min(energy, maxEnergy);
        }
        if (energy <= 0)
        {
            dissipate();
        }
        //Electric Field VC: change its sprite's size based on its energy
        range = energy * energyToRangeRatio;
        friu.setRange(range);
        //Particle effects
        particleController.activateTeleportParticleSystem(true, effectColor, transform.position, range);
    }

    void dissipate()
    {
        Managers.Object.destroyObject(SavableObjectInfo);
    }

    [Initializer]
    private Color init_effectColor => sr.color.adjustAlpha(1);
}