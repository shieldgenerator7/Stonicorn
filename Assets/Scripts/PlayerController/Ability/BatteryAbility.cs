using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// When Merky stands on a wire, he powers it
/// Also the wires slow him down, negate his gravity, and refresh his teleport
/// </summary>
//2026-05-24: copied from ElectricBeamAbility
public class BatteryAbility : PlayerAbility, IPowerer
{
    [Header("Settings")]
    public float range = 2.5f;
    public float energyPerSecond = 100;//how much energy it generates each second
    public float staticSpeed = 2;//how fast it converges your velocity into your target's velocity
    public bool negateGravity = true;
    public bool dampenMomentum = true;


    private Vector2 prevPos;

    struct WireCache
    {
        public GameObject go;
        public IPowerTransferer ipt;
        public Rigidbody2D rb2d;
        public int id;

        public WireCache(IPowerTransferer ipt)
        {
            this.ipt = ipt;
            go = ipt.GameObject;
            rb2d = go.GetComponent<Rigidbody2D>();
            id = go.getKey();
        }
    }
    List<WireCache> wires = new List<WireCache>();
    public List<IPowerTransferer> WireList => wires.ConvertAll(wire => wire.ipt);
    public delegate void OnWiresChanged(List<IPowerTransferer> wireList);
    public event OnWiresChanged onWiresChanged;


    public override void init()
    {
        base.init();
        prevPos = transform.position;
    }

    protected override void registerDelegates(bool register = true)
    {
        if (playerController)
        {
            if (register)
            {
            }
        }
    }

    /// <summary>
    /// Merky powers wires
    /// </summary>
    void FixedUpdate()
    {
        refreshTargets(transform.position);
            if (wires.Count > 0)
            {
                //Power

                //Move relative to the target
                if (CanStatic)
                {
                    applyStatic();
                }
            }
    }

    bool CanStatic =>
        FeatureLevel >= 1 && wires.Count > 0;

    public float ThroughPut => energyPerSecond;

    public GameObject GameObject => gameObject;

    public Collider2D Collider2D => this.playerController.Collider2D;

    private OnPowerFlowed onPowerFlowed;
    public OnPowerFlowed OnPowerFlowed
    {
        get => onPowerFlowed;
        set => onPowerFlowed = value;
    }

    /// <summary>
    /// slow merky down and negate his gravity
    /// </summary>
    /// <param name="apply"></param>
    void applyStatic(bool apply = true)
    {
        playerController.GravityAccepter.AcceptsGravity = !negateGravity || !apply;
        if (apply)
        {
            if (dampenMomentum)
            {
            Vector2 targetVelocity = Vector2.zero;
            rb2d.linearVelocity = Vector2.Lerp(rb2d.linearVelocity, targetVelocity, Time.fixedDeltaTime * staticSpeed);
            }
            playerController.GravityAccepter.AcceptsGravity = !negateGravity || false;
        }
    }

    void refreshTargets(Vector2 targetPos)
    {
        //early exit: same position
        if (prevPos == targetPos) { return; }

        //
        prevPos = targetPos;

        //get list of nearby wires
        List<IPowerTransferer> wires = Physics2D.OverlapCircleAll(transform.position, range)
            .Where(coll => coll.GetComponent<IPowerTransferer>() != null).ToList()
            .ConvertAll(coll => coll.GetComponent<IPowerTransferer>());

        //yes wires found
        if (wires.Count > 0)
        {
            removeAllWires();
            wires.ForEach(wire=>addWire(wire));
            onWiresChanged?.Invoke(WireList);
            playerController.updateGroundedState();
        }
        //no wires found
        else
        {
            if (WireList.Count > 0)
            {
                removeAllWires();
                onWiresChanged?.Invoke(WireList);
            }
        }
    }

    private void addWire(IPowerTransferer ipt)
    {
        if (ipt != null)
        {
            WireCache wire = new WireCache(ipt);
            wires.Add(wire);
            applyStatic(true);
            onWiresChanged?.Invoke(WireList);
        }
    }
    private void removeAllWires()
    {
        wires.Clear();
        if (enabled)
        {
            applyStatic(false);
        }
    }

    public float givePower(float requestedPower)
    {
        return Math.Max(requestedPower, energyPerSecond * Time.fixedDeltaTime);
    }

    #region Input Handling

    protected override void processTeleport(Vector2 oldPos, Vector2 newPos)
    {
        //select target
        refreshTargets(newPos);
    }
    /// <summary>
    /// Refresh teleport while there are wires
    /// </summary>
    /// <returns></returns>
    protected override bool isGrounded() => wires.Count > 0;

    #endregion

    protected override void acceptUpgradeLevel(AbilityUpgradeLevel aul)
    {
        //range = aul.stat1;
        energyPerSecond = aul.stat2;
        staticSpeed = aul.stat3;
    }

    //static byte key_activated = 0;
    //static byte key_targetId = 1;

    //public override SavableObject CurrentState
    //{
    //    get => base.CurrentState.more(
    //        key_activated, activated,
    //        key_targetId, targetId
    //        );
    //    set
    //    {
    //        base.CurrentState = value;
    //        Activated = value.Bool(key_activated);
    //        TargetId = value.Int(key_targetId);
    //    }
    //}
}
