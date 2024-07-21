using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SwapAbility : PlayerAbility
{
    public float swapSizeScaleLimit = 1;

    private PolygonCollider2D pc2d;

    private GameObject swapTarget;
    public GameObject SwapTarget => swapTarget;

    private bool swappedSomething = false;

    /// <summary>
    /// Used for determining if the swapped object's landing spot is occupied
    /// </summary>
    private RaycastHit2D[] rh2dsOccupied = new RaycastHit2D[Utility.MAX_HIT_COUNT];
    /// <summary>
    /// Used for determining which objects can be swapped for any given teleport attempt
    /// </summary>
    private RaycastHit2D[] rh2dsSwappable = new RaycastHit2D[Utility.MAX_HIT_COUNT];

    public override void init()
    {
        base.init();
        playerController.Teleport.findTeleportablePositionOverride += findSwapPosition;
        playerController.Teleport.onRangeChanged += onRangeChanged;
        pc2d = GetComponent<PolygonCollider2D>();
    }
    public override void OnDisable()
    {
        base.OnDisable();
        playerController.Teleport.findTeleportablePositionOverride -= findSwapPosition;
        playerController.Teleport.onRangeChanged -= onRangeChanged;
    }

    private void Update()
    {
        List<GameObject> swappables = Physics2D.OverlapCircleAll(
            transform.position,
            playerController.Teleport.Range
            ).ToList()
            .FindAll(coll => isObjectSwappable(coll.gameObject))
            .ConvertAll(coll => coll.gameObject);
        //Hide current effects
        Managers.Effect.hideSwapCircleEffects(swappables);
        //Show which game objects are swappable
        swappables.ForEach(go => Managers.Effect.showSwapCircle(go));
    }

    protected override bool isGrounded()
        => swappedSomething;

    private void onRangeChanged(float range)
    {
        if (swappedSomething)
        {
            if (range < playerController.Teleport.baseRange)
            {
                playerController.Teleport.Range = playerController.Teleport.baseRange;
            }
        }
    }

    bool isObjectSwappable(GameObject go)
        => go != this.gameObject
        && go.GetComponent<Rigidbody2D>()
        && go.getSize().magnitude <= playerController.halfWidth * 2 * swapSizeScaleLimit;

    bool isColliderSwappable(Collider2D coll, Vector3 tapPos)
        => isObjectSwappable(coll.gameObject)
        && coll.OverlapPoint(tapPos)
        && isColliderSwappableImpl(coll, tapPos);

    bool isColliderSwappableImpl(Collider2D coll, Vector3 testPos)
    {
        Vector3 swapPos = transform.position;
        return true;
        //bool occupied = isOccupiedForObject(coll, swapPos);
        //if (occupied)
        //{
        //    Vector2 newPos = adjustForOccupant(coll, swapPos);
        //    occupied = isOccupiedForObject(coll, newPos);
        //}
        //return !occupied;
    }

    /// <summary>
    /// Determines whether the given position is occupied by an object other than Merky or not
    /// 2018-02-12: copied from PlayerController.isOccupied(.)
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    public bool isOccupiedForObject(Collider2D coll, Vector3 pos)
    {
        Vector3 offset = pos - coll.gameObject.transform.position;
        float angle = coll.gameObject.transform.localEulerAngles.z;
        Vector3 rOffset = Quaternion.AngleAxis(-angle, Vector3.forward) * offset;//2017-02-14: copied from an answer by robertbu: http://answers.unity3d.com/questions/620828/how-do-i-rotate-a-vector2d.html

        //Do the test
        Vector3 savedOffset = coll.offset;
        coll.offset = rOffset;
        int count = Utility.Cast(coll, Vector2.zero, rh2dsOccupied, 0, true);
        coll.offset = savedOffset;
        for (int i = 0; i < count; i++)
        {
            RaycastHit2D rch2d = rh2dsOccupied[i];
            GameObject go = rch2d.collider.gameObject;
            //Make sure it's not a trigger
            if (!rch2d.collider.isTrigger)
            {
                //Make sure it's not detecting itself or this gameObject
                if (go != coll.gameObject && go != gameObject)
                {
                    return true;
                }

            }
        }
        return false;
    }

    /// <summary>
    /// Adjusts the given Vector3 to avoid collision with the objects that it collides with
    /// </summary>
    /// <param name="testPos">The Vector3 to adjust</param>
    /// <returns>The Vector3, adjusted to avoid collision with objects it collides with</returns>
    /// 2019-09-02: copied from PlayerController.adjustForOccupant()
    private Vector3 adjustForOccupant(Collider2D coll, Vector3 testPos)
    {
        //Find the objects that it would collide with
        Vector3 testOffset = testPos - coll.transform.position;
        testOffset = transform.InverseTransformDirection(testOffset);
        Vector3 savedOffset = pc2d.offset;
        pc2d.offset = testOffset;
        Utility.RaycastAnswer answer;
        answer = pc2d.CastAnswer(Vector2.zero, 0, true);
        pc2d.offset = savedOffset;
        Vector3 extents = getExtents(coll);
        //Adjust the move direction for each found object that it collides with
        Vector3 moveDir = Vector3.zero;//the direction to move the testPos
        for (int i = 0; i < answer.count; i++)
        {
            RaycastHit2D rh2d = answer.rch2ds[i];
            GameObject go = rh2d.collider.gameObject;
            //If the game object is not this game object,
            if (go != transform.gameObject && go != coll.gameObject)
            {
                //And if the game object is not a trigger,
                if (!rh2d.collider.isTrigger)
                {
                    //Figure out in which direction to move and how far
                    Vector3 outDir = testPos - (Vector3)rh2d.point;
                    //(half width is only an estimate of the dist from the sprite center to its edge)
                    float adjustDistance = extents.x - rh2d.distance;
                    //If the distance to move is invalid,
                    if (adjustDistance < 0)
                    {
                        //Use a different estimate of sprite width
                        adjustDistance = pc2d.bounds.extents.magnitude - rh2d.distance;
                    }
                    //If the sprite is mostly contained within the found object,
                    if (rh2d.collider.OverlapPoint(testPos))
                    {
                        //Reverse the direction and increase the dist
                        outDir *= -1;
                        adjustDistance += extents.x;
                    }
                    //Add the calculated direction and magnitude to the running total
                    moveDir += outDir.normalized * adjustDistance;
                }
            }
        }
        return testPos + moveDir;
    }

    Vector3 getExtents(Collider2D coll)
    {
        Vector3 savedUp = coll.transform.up;
        coll.transform.up = Vector3.up;
        Vector3 extents = coll.bounds.extents;
        coll.transform.up = savedUp;
        return extents;
    }

    protected override void processTeleport(Vector2 oldPos, Vector2 newPos)
    {
        swappedSomething = false;
        if (swapTarget)
        {
            //Clear connections
            Rigidbody2D goRB2D = swapTarget.GetComponent<Rigidbody2D>();
            if (goRB2D)
            {
                //Disconnect attachments, if any
                foreach (FixedJoint2D fj2d in GameObject.FindObjectsByType<FixedJoint2D>(FindObjectsSortMode.None))
                {
                    if (fj2d.connectedBody == goRB2D)
                    {
                        Destroy(fj2d);
                    }
                }
                //Nullify velocity
                goRB2D.nullifyMovement();
            }
            //Nullify Merky's velocity
            rb2d.nullifyMovement();
            //Swap object
            Vector2 swapPos = (Vector2)gameObject.transform.position - newPos + oldPos;
            if (isOccupiedForObject(gameObject.GetComponent<Collider2D>(), swapPos))
            {
                swapPos = adjustForOccupant(gameObject.GetComponent<Collider2D>(), swapPos);
            }
            swapTarget.transform.position = swapPos;
            swappedSomething = true;
            playerController.Teleport.Range = playerController.Teleport.baseRange;//TODO: properly hook this up
            //Swappables
            swapTarget.GetComponents<ISwappable>().ToList()
                .ForEach(swappee => swappee.nowSwapped());
            //Upgrade 1: Rotate
            if (CanRotate)
            {
                applyRotate();
            }
            //Upgrade 2: Stasis
            if (CanStasis)
            {
                applyStasis();
            }
            //Give player time to tap again after swapping
            playerController.MovementPaused = true;
            //Update Stats
            Managers.Stats.addOne(Stat.SWAP_OBJECT);
            //Update Stats
            Managers.Stats.addOne(Stat.SWAP);
            //Delegates
            effectTeleport(oldPos, newPos);
            onSwap?.Invoke(playerController.gameObject, swapTarget);
        }
    }
    public delegate void OnSwap(GameObject go1, GameObject go2);
    public event OnSwap onSwap;

    private Vector2 findSwapPosition(Vector2 targetPos, Vector2 tapPos)
    {
        findSwapTarget(targetPos, tapPos);
        if (swapTarget != null)
        {
            return swapTarget.transform.position;
        }
        return Vector2.zero;
    }

    private void findSwapTarget(Vector2 targetPos, Vector2 tapPos)
    {
        swapTarget = null;
        Utility.RaycastAnswer answer = Utility.RaycastAll(targetPos, Vector2.up, 0);
        for (int i = 0; i < answer.count; i++)
        {
            RaycastHit2D rch2d = answer.rch2ds[i];
            GameObject rch2dGO = rch2d.collider.gameObject;
            if (isColliderSwappable(rch2d.collider, tapPos))
            {
                swapTarget = rch2dGO;
            }
        }
    }

    bool CanRotate =>
        FeatureLevel >= 1;

    void applyRotate()
    {
        Vector2 swapUp = transform.up;
        transform.up = swapTarget.transform.up;
        swapTarget.transform.up = swapUp;
        onRotate?.Invoke(playerController.gameObject, swapTarget);
    }
    public event OnSwap onRotate;

    bool CanStasis =>
        FeatureLevel >= 2 && CanUseUltimate;

    void applyStasis()
    {
        StaticUntilTouched sut = swapTarget.AddComponent<StaticUntilTouched>();
        sut.onRootedChanged += (rooted) => { if (!rooted) { Destroy(sut); } };
        onStasis?.Invoke(playerController.gameObject, swapTarget);
    }
    public event OnSwap onStasis;

    protected override void acceptUpgradeLevel(AbilityUpgradeLevel aul)
    {
        swapSizeScaleLimit = aul.stat1;
    }
}
