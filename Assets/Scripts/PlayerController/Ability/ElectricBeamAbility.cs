using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ElectricBeamAbility : PlayerAbility
{
    [Header("Settings")]
    public float range = 2.5f;
    public float energyPerSecond = 100;//how much energy it generates each second
    public float staticSpeed = 2;//how fast it converges your velocity into your target's velocity
    public float rangeBuffer = 1;//how much more outside the range a target can be before being disconnected
    public float maxCharge = 50;//how much energy he can store at once
    public float chargePerTeleport = 10;//how much energy he generates per teleport

    [Header("Components")]
    public GameObject wirePrefab;

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

    private Vector2 tapPos;
    private bool tapOnPlayer;
    private bool wiredThisInput = false;//true if it has wired since the last user input

    GameObject targetGO;
    IPowerable targetPowerable;
    Rigidbody2D targetRB2D;
    public IPowerable Target
    {
        get => targetPowerable;
        private set
        {
            IPowerable oldTarget = targetPowerable;
            targetPowerable = value;
            if (targetPowerable != null)
            {
                targetGO = targetPowerable.GameObject;
                targetRB2D = targetGO.GetComponent<Rigidbody2D>();
                targetId = targetGO.getKey();
            }
            else
            {
                targetGO = null;
                targetRB2D = null;
                targetId = -1;
                if (enabled)
                {
                    applyStatic(false);
                }
            }
            onTargetChanged?.Invoke(oldTarget, targetPowerable);
        }
    }
    public delegate void OnTargetChanged(IPowerable oldPowerable, IPowerable newPowerable);
    public event OnTargetChanged onTargetChanged;

    int targetId = -1;
    public int TargetId
    {
        get => targetId;
        set
        {
            if (value < 0)
            {
                Target = null;
            }
            else
            {
                SavableObjectInfo soi = Managers.Object.getObject(value);
                if (soi != null)
                {
                    //TODO: check to make sure all IPowerables also have a SavableObjectInfo. then, we can loop thru soi's smb list instead of using GetComponent()
                    IPowerable powerable = soi.GetComponent<IPowerable>();
                    Target = powerable;
                }
            }
        }
    }

    private float charge = 0;
    public float Charge
    {
        get => charge;
        set
        {
            charge = Mathf.Clamp(value, 0, maxCharge);
            onChargeChanged?.Invoke(charge);
        }
    }
    public event Action<float> onChargeChanged;

    public override void init()
    {
        base.init();
        rangeChanged(playerController.Teleport.Range);
    }

    protected override void registerDelegates(bool register = true)
    {
        if (playerController)
        {
            playerController.Teleport.findTeleportablePositionOverride
                -= findTeleportablePosition;
            playerController.Teleport.onRangeChanged -= rangeChanged;
            if (register)
            {
                playerController.Teleport.findTeleportablePositionOverride
                    += findTeleportablePosition;
                playerController.Teleport.onRangeChanged += rangeChanged;
            }
        }
    }

    void FixedUpdate()
    {
        if (Activated)
        {
            if (targetPowerable != null)
            {
                //Power
                float power = energyPerSecond * Time.fixedDeltaTime;
                float leftOver = targetPowerable.acceptPower(power);
                Charge -= (power - leftOver);

                //Move relative to the target
                if (CanStatic)
                {
                    applyStatic();
                }

                //Make wire
                if (CanWire)
                {
                    applyWire();
                }

                //Make sure target is still in range
                checkTarget();

                //Check to see if it still has power
                if (charge == 0)
                {
                    Activated = false;
                    Target = null;
                }
            }
            else
            {
                selectTarget(transform.position);
            }
        }
    }

    bool CanStatic =>
        FeatureLevel >= 1 && Target != null;

    void applyStatic(bool apply = true)
    {
        playerController.GravityAccepter.AcceptsGravity = !apply;
        if (apply)
        {
            Vector2 targetVelocity = (targetRB2D) ? targetRB2D.linearVelocity : Vector2.zero;
            rb2d.linearVelocity = Vector2.Lerp(rb2d.linearVelocity, targetVelocity, Time.fixedDeltaTime * staticSpeed);
            playerController.GravityAccepter.AcceptsGravity = false;
        }
    }

    bool CanWire =>
        !wiredThisInput && Target != null && FeatureLevel >= 2 && !(targetRB2D && targetRB2D.isMoving()) && CanUseUltimate;

    void applyWire()
    {
        Vector2 startPos = transform.position;
        Vector2 endPos = Target.GameObject.transform.position;
        Vector2 dir = endPos - startPos;
        GameObject newWire = Managers.Object.Instantiate(wirePrefab);
        newWire.transform.right = dir;
        newWire.transform.position = (startPos + endPos) / 2;
        SpriteRenderer sr = newWire.GetComponent<SpriteRenderer>();
        sr.size = new Vector2(dir.magnitude, sr.size.y);
        Managers.Power.generateConnectionMap();
        wiredThisInput = true;
    }

    void selectTarget(Vector2 targetPos)
    {
        List<IPowerable> powerables = Physics2D.OverlapCircleAll(transform.position, range)
            .Where(coll =>
                coll.GetComponent<IPowerable>() != null
                && inRange(coll.gameObject)
            )
            .OrderBy(coll => ((Vector2)coll.transform.position - targetPos).sqrMagnitude).ToList()
            .ConvertAll(coll => coll.GetComponent<IPowerable>());
        if (powerables.Count > 0)
        {
            Target = powerables.First();
            playerController.updateGroundedState();
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
        if (inRange(Target.GameObject, range + rangeBuffer))
        {
            //all good
        }
        else
        {
            //disconnect from target
            Target = null;
        }
    }

    void rangeChanged(float range)
    {
        this.range = range;
        onRangeChanged?.Invoke(this.range);
    }
    public event Action<float> onRangeChanged;

    bool inRange(GameObject go, float range = 0)
    {
        range = (range > 0) ? range : this.range;
        return go.transform.position.inRange(transform.position, range);
    }

    #region Input Handling
    Vector2 findTeleportablePosition(Vector2 rangePos, Vector2 tapPos)
    {
        this.tapPos = tapPos;
        tapOnPlayer = playerController.gestureOnPlayer(tapPos);
        return Vector2.zero;
    }

    protected override void processTeleport(Vector2 oldPos, Vector2 newPos)
    {
        //deactivate
        if (tapOnPlayer)
        {
            Charge = 0;
            Activated = false;
            Target = null;
            return;
        }
        //charge
        Charge += chargePerTeleport * (newPos - oldPos).magnitude / playerController.Teleport.baseRange;
        if (charge > 0)
        {
            Activated = true;
        }
        //select target
        selectTarget(tapPos);
        //
        wiredThisInput = false;
    }
    protected override bool isGrounded() => Activated && CanStatic;

    #endregion

    protected override void acceptUpgradeLevel(AbilityUpgradeLevel aul)
    {
        //range = aul.stat1;
        energyPerSecond = aul.stat2;
        staticSpeed = aul.stat3;
        maxCharge = aul.stat4;
    }

    static byte key_activated = 0;
    static byte key_targetId = 1;
    static byte key_charge = 2;

    public override SavableObject CurrentState
    {
        get => base.CurrentState.more(
            key_activated, activated,
            key_targetId, targetId,
            key_charge, charge
            );
        set
        {
            base.CurrentState = value;
            Activated = value.Bool(key_activated);
            TargetId = value.Int(key_targetId);
            Charge = value.Float(key_charge);
        }
    }
}
