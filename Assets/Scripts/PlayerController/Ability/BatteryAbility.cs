using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//2026-05-24: copied from ElectricBeamAbility
public class BatteryAbility : PlayerAbility
{
    [Header("Settings")]
    public float range = 2.5f;
    public float energyPerSecond = 100;//how much energy it generates each second
    public float staticSpeed = 2;//how fast it converges your velocity into your target's velocity
    public float rangeBuffer = 1;//how much more outside the range a target can be before being disconnected

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
            Activated = false;
            Target = null;
            return;
        }
            Activated = true;
        //select target
        selectTarget(tapPos);
        //
    }
    protected override bool isGrounded() => Activated && CanStatic;

    #endregion

    protected override void acceptUpgradeLevel(AbilityUpgradeLevel aul)
    {
        //range = aul.stat1;
        energyPerSecond = aul.stat2;
        staticSpeed = aul.stat3;
    }

    static byte key_activated = 0;
    static byte key_targetId = 1;
    static byte key_charge = 2;

    public override SavableObject CurrentState
    {
        get => base.CurrentState.more(
            key_activated, activated,
            key_targetId, targetId
            );
        set
        {
            base.CurrentState = value;
            Activated = value.Bool(key_activated);
            TargetId = value.Int(key_targetId);
        }
    }
}
