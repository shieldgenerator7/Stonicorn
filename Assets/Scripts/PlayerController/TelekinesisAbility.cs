using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TelekinesisAbility : PlayerAbility
{
    [Header("Settings")]
    /// <summary>
    /// Merky can hold an object that is this many times bigger than him or smaller
    /// </summary>
    public float holdSizeScaleLimit = 1;
    /// <summary>
    /// Merky can start holding an object if it is within this distance
    /// </summary>
    public float maxHoldStartRange = 3;
    /// <summary>
    /// If a held object leaves this range, Merky automatically drops it
    /// </summary>
    public float maxHoldKeepRange = 10;
    /// <summary>
    /// How fast objects move to their desired location relative to Merky
    /// </summary>
    public float pullSpeed = 3;
    /// <summary>
    /// How fast objects snap to the target velocity to get them back to their desired location
    /// </summary>
    public float pullAcceleration = 1;

    [System.Serializable]
    private struct HoldContext
    {
        public GameObject go;
        public Rigidbody2D rb2d;
        public Vector2 offset;

        public HoldContext(GameObject go, Vector2 origin)
        {
            this.go = go;
            this.rb2d = go.GetComponent<Rigidbody2D>();
            this.offset = (Vector2)go.transform.position - origin;
        }
    }
    [SerializeField]
    private List<HoldContext> holdTargets;

    public override void init()
    {
        base.init();
        playerController.teleportOverride += checkOverrideTeleport;
    }
    public override void OnDisable()
    {
        base.OnDisable();
        playerController.teleportOverride -= checkOverrideTeleport;
    }

    private void FixedUpdate()
    {
        //For each held object, pull it towards its desired position relative to Merky
        Vector2 playerPosition = (Vector2)transform.position;
        holdTargets.ForEach((hc) =>
        {
            Vector2 currentPosition = hc.go.transform.position;
            Vector2 targetPosition = playerPosition + hc.offset;
            //TODO: auto-drop if object becomes outside of range
            if (currentPosition != targetPosition)
            {
                Vector2 targetVelocity = (targetPosition - currentPosition).normalized * pullSpeed;
                hc.rb2d.velocity = Vector2.Lerp(
                    hc.rb2d.velocity,
                    targetVelocity,
                    Time.fixedDeltaTime * pullAcceleration
                    );
            }
        });
    }

    protected override void acceptUpgradeLevel(AbilityUpgradeLevel aul)
    {
        holdSizeScaleLimit = aul.stat1;
    }

    protected override bool isGrounded() => false;

    protected override void processTeleport(Vector2 oldPos, Vector2 newPos) { }

    //TODO: add delegates for when telekinesis happens (for stats, etc)

    private bool checkOverrideTeleport(Vector2 tapPos)
    {
        GameObject holdTarget = findHoldTarget(tapPos);
        if (holdTarget)
        {
            if (isObjectHeld(holdTarget))
            {
                dropObject(holdTarget);
            }
            else
            {
                pickupObject(holdTarget);
            }
            return true;
        }
        return false;
    }

    private GameObject findHoldTarget(Vector2 pos)
    {
        Utility.RaycastAnswer answer = Utility.RaycastAll(pos, Vector2.up, 0);
        for (int i = 0; i < answer.count; i++)
        {
            RaycastHit2D rch2d = answer.rch2ds[i];
            GameObject rch2dGO = rch2d.collider.gameObject;
            if (isObjectHoldable(rch2dGO))
            {
                return rch2dGO;
            }
        }
        return null;
    }

    private bool isObjectHoldable(GameObject go)
       => go != this.gameObject
       && go.GetComponent<Rigidbody2D>()
       && go.getSize().magnitude <= playerController.halfWidth * 2 * holdSizeScaleLimit;
    //TODO: check to make sure object is within range

    private bool isObjectHeld(GameObject go)
    {
        if (!go)
        {
            Debug.LogError($"GameObject is null!: {go}");
        }
        return holdTargets.Any(hc => hc.go == go);
    }

    private void pickupObject(GameObject go)
    {
        //Make gameobject effected by things
        StaticUntilTouched sut = go.GetComponent<StaticUntilTouched>();
        if (sut)
        {
            sut.Rooted = false;
        }
        //Add gameobject to list
        HoldContext hc = new HoldContext(go, transform.position);
        holdTargets.Add(hc);
        //TEMP: visual effects
        Managers.Effect.showSwapCircle(go, true);
    }

    private void dropObject(GameObject go)
    {
        HoldContext hc = holdTargets.Find(hc => hc.go == go);
        holdTargets.Remove(hc);
        //TEMP: visual effects
        Managers.Effect.showSwapCircle(go, false);
    }

    //TODO: add hold targets to save list
}
