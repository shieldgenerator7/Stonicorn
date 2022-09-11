using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TelekinesisAbility : PlayerAbility
{
    protected override void acceptUpgradeLevel(AbilityUpgradeLevel aul)
    {
        holdSizeScaleLimit = aul.stat1;
    }

    protected override bool isGrounded() => false;

    protected override void processTeleport(Vector2 oldPos, Vector2 newPos) { }

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

    private GameObject holdTarget;
    public GameObject HoldTarget => holdTarget;

    private struct HoldContext
    {
        public GameObject go;
        public Vector2 offset;
    }
    private List<HoldContext> holdTargets;

    public override void init()
    {
        base.init();
        playerController.Teleport.findTeleportablePositionOverride += findHoldPos;
    }
    public override void OnDisable()
    {
        base.OnDisable();
        playerController.Teleport.findTeleportablePositionOverride -= findHoldPos;
    }

    private void Update()
    {
        //For each held object, pull it towards its desired position relative to Merky
        //TODO
    }

    bool isObjectHoldable(GameObject go)
        => go != this.gameObject
        && go.GetComponent<Rigidbody2D>()
        && go.getSize().magnitude <= playerController.halfWidth * 2 * holdSizeScaleLimit;

    bool isColliderHoldable(Collider2D coll, Vector3 tapPos)
        => isObjectHoldable(coll.gameObject)
        && coll.OverlapPoint(tapPos);

    //TODO: add delegates for when telekinesis happens

    //TODO: check to see if this is the right delegate to use (should it happen before a targetPos is determined?)
    private Vector2 findHoldPos(Vector2 targetPos, Vector2 tapPos)
    {
        holdTarget = findHoldTarget(targetPos, tapPos);
        //If hold target found, don't teleport
        return (holdTarget) ?Vector2.zero :targetPos;
    }

    private GameObject findHoldTarget(Vector2 targetPos, Vector2 tapPos)
    {
        Utility.RaycastAnswer answer = Utility.RaycastAll(tapPos, Vector2.up, 0);
        for (int i = 0; i < answer.count; i++)
        {
            RaycastHit2D rch2d = answer.rch2ds[i];
            GameObject rch2dGO = rch2d.collider.gameObject;
            if (isColliderHoldable(rch2d.collider, tapPos))
            {
                return rch2dGO;
            }
        }
        return null;
    }
}
