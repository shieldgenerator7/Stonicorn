using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElectricRingAbility : PlayerAbility
{
    [Header("Settings")]
    public float maxRange = 2.5f;
    public float maxEnergy = 100;//how much energy it can store
    public float energyPerSecond = 100;//how much energy it generates each second
    public float slowSpeed = 1;//how much to decrease velocity by each second

    //State variables
    [Header("State Variables")]
    [SerializeField]
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

    [SerializeField]
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

    [SerializeField]
    private float range = 0;
    public float Range
        => maxRange * energy / maxEnergy;

    protected override void init()
    {
        base.init();
        playerController.Teleport.findTeleportablePositionOverride
            += findTeleportablePosition;
        playerController.Teleport.onTeleport += onTeleport;
    }
    public override void OnDisable()
    {
        base.OnDisable();
        playerController.Teleport.findTeleportablePositionOverride
            -= findTeleportablePosition;
        playerController.Teleport.onTeleport -= onTeleport;
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
        range = Range;
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

    void onTeleport(Vector2 oldPos, Vector2 newPos)
    {
        if (tapOnPlayer)
        {
            Activated = !activated;
        }
        tapOnPlayer = false;
    }
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
