using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ElectricBeamAbility : StonicornAbility
{
    [Header("Settings")]
    public float range = 2.5f;
    public float energyPerSecond = 100;//how much energy it generates each second
    public float staticSpeed = 2;//how fast it converges your velocity into your target's velocity
    public float rangeBuffer = 1;//how much more outside the range a target can be before being disconnected

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

    private bool tapOnPlayer = false;
    private bool wiredThisInput = false;//true if it has wired since the last user input

    GameObject target;
    IPowerable targetPowerable;
    Rigidbody2D targetRB2D;
    public GameObject Target
    {
        get => target;
        private set
        {
            GameObject oldTarget = target;
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
                if (enabled)
                {
                    applyStatic(false);
                }
            }
            onTargetChanged?.Invoke(oldTarget, target);
        }
    }
    public delegate void OnTargetChanged(GameObject oldGO, GameObject newGO);
    public event OnTargetChanged onTargetChanged;

    public override void init()
    {
        base.init();
        stonicorn.Teleport.findTeleportablePositionOverride
            += findTeleportablePosition;
    }
    public override void OnDisable()
    {
        base.OnDisable();
        stonicorn.Teleport.findTeleportablePositionOverride
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

                //Make wire
                if (CanWire)
                {
                    applyWire();
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

    void applyStatic(bool apply = true)
    {
        stonicorn.GravityAccepter.AcceptsGravity = !apply;
        if (apply)
        {
            Vector2 targetVelocity = (targetRB2D) ? targetRB2D.velocity : Vector2.zero;
            rb2d.velocity = Vector2.Lerp(rb2d.velocity, targetVelocity, Time.fixedDeltaTime * staticSpeed);
            stonicorn.GravityAccepter.AcceptsGravity = false;
        }
    }

    bool CanWire =>
        !wiredThisInput && Target != null && FeatureLevel >= 2 && !(targetRB2D && targetRB2D.isMoving());

    void applyWire()
    {
        Vector2 startPos = transform.position;
        Vector2 endPos = Target.transform.position;
        Vector2 dir = endPos - startPos;
        GameObject newWire = Utility.Instantiate(wirePrefab);
        newWire.transform.right = dir;
        newWire.transform.position = (startPos + endPos) / 2;
        SpriteRenderer sr = newWire.GetComponent<SpriteRenderer>();
        sr.size = new Vector2(dir.magnitude, sr.size.y);
        Managers.Power.generateConnectionMap();
        wiredThisInput = true;
    }

    void selectTarget()
    {
        List<GameObject> powerables = Physics2D.OverlapCircleAll(transform.position, range).ToList()
            .FindAll(coll => coll.GetComponent<IPowerable>() != null)
            .FindAll(coll => inRange(coll.gameObject))
            .OrderBy(coll => (coll.transform.position - transform.position).sqrMagnitude).ToList()
            .ConvertAll(coll => coll.gameObject);
        if (powerables.Count > 0)
        {
            int index = (target) ? powerables.IndexOf(target) : -1;
            int newIndex = (index + 1) % powerables.Count;
            Target = powerables[newIndex];
            stonicorn.updateGroundedState();
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
        if (inRange(Target, range + rangeBuffer))
        {
            //all good
        }
        else
        {
            //disconnect from target
            Target = null;
        }
    }

    bool inRange(GameObject go, float range = 0)
    {
        range = (range > 0) ? range : this.range;
        return go.transform.position.inRange(transform.position, range);
    }

    #region Input Handling
    Vector2 findTeleportablePosition(Vector2 rangePos, Vector2 tapPos)
    {
        if (stonicorn.gestureOnSprite(tapPos))
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
        wiredThisInput = false;
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
            "activated", activated,
            "target", target.getKey()
            );
        set
        {
            base.CurrentState = value;
            Activated = value.Bool("activated");
            int targetId = value.Int("target");
            if (targetId >= 0)
            {
                Target = Managers.Object.getObject(targetId);
            }
            else
            {
                Target = null;
            }
        }
    }
}
