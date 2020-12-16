using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ElectricRingAbility : PlayerAbility
{
    [Header("Settings")]
    public float maxRange = 2.5f;
    public float maxEnergy = 100;//how much energy it can store
    public float energyPerSecond = 100;//how much energy it generates each second
    public float slowSpeed = 1;//how much to decrease velocity by each second

    //State variables
    private float energy = 0;
    public float Energy
    {
        get => energy;
        private set
        {
            energy = Mathf.Clamp(value, 0, maxEnergy);
            onEnergyChanged?.Invoke(energy);
        }
    }
    public delegate void OnEnergyChanged(float energy);
    public event OnEnergyChanged onEnergyChanged;

    private bool activated = false;
    public bool Activated
    {
        get => activated;
        private set
        {
            if (activated != value)
            {
                activated = value;
                onActivatedChanged?.Invoke(activated);
            }
        }
    }
    public delegate void OnActivatedChanged(bool activated);
    public event OnActivatedChanged onActivatedChanged;

    private bool tapOnPlayer = false;

    public float Range
        => maxRange * energy / maxEnergy;

    HashSet<Rigidbody2D> rb2ds = new HashSet<Rigidbody2D>();
    HashSet<IPowerConduit> conduits = new HashSet<IPowerConduit>();

    protected override void init()
    {
        base.init();
        playerController.Teleport.findTeleportablePositionOverride
            += findTeleportablePosition;
    }
    public override void OnDisable()
    {
        base.OnDisable();
        playerController.Teleport.findTeleportablePositionOverride
            -= findTeleportablePosition;
    }

    void FixedUpdate()
    {
        if (activated)
        {
            Energy += energyPerSecond * Time.fixedDeltaTime;
        }
        else if (energy > 0)
        {
            Energy -= energyPerSecond * Time.fixedDeltaTime;
        }

        //Power
        //2020-12-08: copied from ElectricFieldController.FixedUpdate()
        foreach (IPowerConduit conduit in conduits)
        {
            if (conduit is IPowerable)
            {
                float amountTaken = ((IPowerable)conduit).acceptPower(
                    energy * Time.fixedDeltaTime
                    );
                Energy += -amountTaken;
            }
        }
        //Slow
        if (FeatureLevel >= 1)
        {
            foreach (Rigidbody2D rb2d in rb2ds)
            {
                if (rb2d.isMoving())
                {
                    rb2d.velocity += -rb2d.velocity.normalized
                        * slowSpeed * Time.fixedDeltaTime;
                }
                else
                {
                    rb2d.nullifyMovement();
                }
            }
        }
    }

    public void addObject(GameObject go)
    {
        Rigidbody2D rb2d = go.GetComponent<Rigidbody2D>();
        if (rb2d)
        {
            rb2ds.Add(rb2d);
        }
        IPowerConduit conduit = go.GetComponent<IPowerConduit>();
        if (conduit != null)
        {
            conduits.Add(conduit);
        }
    }
    public void removeObject(GameObject go)
    {
        Rigidbody2D rb2d = go.GetComponent<Rigidbody2D>();
        if (rb2d)
        {
            rb2ds.Remove(rb2d);
        }
        IPowerConduit conduit = go.GetComponent<IPowerConduit>();
        if (conduit != null)
        {
            conduits.Remove(conduit);
        }
    }

    #region Input Handling
    Vector2 findTeleportablePosition(Vector2 rangePos, Vector2 tapPos)
    {
        if (playerController.gestureOnPlayer(tapPos))
        {
            tapOnPlayer = true;
        }
        return Vector2.zero;
    }

    protected override void processTeleport(Vector2 oldPos, Vector2 newPos)
    {
        if (tapOnPlayer)
        {
            Activated = !activated;
        }
        tapOnPlayer = false;
    }
    protected override bool isGrounded() => false;
    #endregion

    protected override void acceptUpgradeLevel(AbilityUpgradeLevel aul)
    {
        maxRange = aul.stat1;
        maxEnergy = aul.stat2;
        energyPerSecond = aul.stat3;
        slowSpeed = aul.stat4;
    }
    public override SavableObject CurrentState
    {
        get => base.CurrentState.more(
            "energy", energy,
            "activated", activated
            );
        set
        {
            base.CurrentState = value;
            Energy = value.Float("energy");
            Activated = value.Bool("activated");
        }
    }


}
