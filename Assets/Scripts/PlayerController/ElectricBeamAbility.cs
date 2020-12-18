using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ElectricBeamAbility : PlayerAbility
{
    [Header("Settings")]
    public float range = 2.5f;
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

    GameObject target;
    IPowerable targetPowerable;
    Rigidbody2D targetRB2D;
    public GameObject Target
    {
        get => target;
        private set
        {
            target = value;
            if (target)
            {
                targetPowerable = target.GetComponent<IPowerable>();
                targetRB2D = target.GetComponent<Rigidbody2D>();
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
        if (Activated)
        {
            if (target)
            {
                //Power
                targetPowerable.acceptPower(energyPerSecond * Time.fixedDeltaTime);

                //Move relative to the target
                if (CanStatic)
                {
                    applyStatic();
                }

                //Make sure target is still in range
                checkTarget();
            }
            else
            {
                selectTarget();
            }
        }
    }

    bool CanStatic =>
        FeatureLevel >= 1 && Target != null;

    void applyStatic()
    {
        rb2d.velocity = (targetRB2D) ? targetRB2D.velocity : Vector2.zero;
    }

    void selectTarget()
    {
        List<GameObject> powerables = Physics2D.OverlapCircleAll(transform.position, range).ToList()
            .FindAll(coll => coll.GetComponent<IPowerable>() != null)
            .OrderBy(coll => (coll.transform.position - transform.position).sqrMagnitude).ToList()
            .ConvertAll(coll => coll.gameObject);
        if (powerables.Count > 0)
        {
            int index = (target) ? powerables.IndexOf(target) : -1;
            int newIndex = (index + 1) % powerables.Count;
            Target = powerables[newIndex];
        }
        else
        {
            Target = null;
        }
    }

    /// <summary>
    /// Checks to make sure the target is still valid
    /// The target can be invalid if it moves out of range
    /// Assumes there is a target already
    /// </summary>
    void checkTarget()
    {
        //If it's in range
        if (target.transform.position.inRange(transform.position, range))
        {
            //all good
        }
        else
        {
            //disconnect from target
            Target = null;
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
        Activated = tapOnPlayer;
        if (Activated)
        {
            selectTarget();
        }
        else
        {
            Target = null;
        }
        tapOnPlayer = false;
    }
    protected override bool isGrounded() => Activated && CanStatic;
    #endregion

    protected override void acceptUpgradeLevel(AbilityUpgradeLevel aul)
    {
        range = aul.stat1;
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
