using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElectricBeamAbility : PlayerAbility
{
    [Header("Settings")]
    public float maxRange = 2.5f;
    public float energyPerSecond = 100;//how much energy it generates each second

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

    IPowerable targetPowerable;
    Rigidbody2D targetRB2D;
    GameObject Target
    {
        get => targetRB2D?.gameObject;
        set
        {
            if (value)
            {
                targetPowerable = value.GetComponent<IPowerable>();
                targetRB2D = value.GetComponent<Rigidbody2D>();
            }
            else
            {
                targetPowerable = null;
                targetRB2D = null;
            }
        }
    }

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
        //Power
        targetPowerable.acceptPower(energyPerSecond * Time.fixedDeltaTime);

        //Move relative to the target
        if (CanStatic)
        {
            applyStatic();
        }
    }

    bool CanStatic =>
        FeatureLevel >= 1 && Target != null;

    void applyStatic()
    {
        rb2d.velocity = targetRB2D.velocity;
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
        Activated = tapOnPlayer;
        tapOnPlayer = false;
    }
    protected override bool isGrounded() => Activated && CanStatic;
    #endregion

    protected override void acceptUpgradeLevel(AbilityUpgradeLevel aul)
    {
        maxRange = aul.stat1;
        energyPerSecond = aul.stat2;
    }
    public override SavableObject CurrentState
    {
        get => base.CurrentState.more(
            "activated", activated
            );
        set
        {
            base.CurrentState = value;
            Activated = value.Bool("activated");
        }
    }
}
