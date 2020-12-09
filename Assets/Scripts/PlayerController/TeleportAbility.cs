using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Currently just used for the teleport hold gesture effect,
/// but might actually have teleport capabilities in the future
/// </summary>
public class TeleportAbility : PlayerAbility
{//2017-08-07: copied from ForceTeleportAbility
    [Header("Teleport")]
    [Range(0, 10)]
    public float baseRange = 3;//the range after touching the ground
    [Range(0, 10)]
    public float exhaustRange = 1;//the range after teleporting into the air (and being exhausted)
    [Range(0, 1)]
    public float coolDownTime = 0.1f;//the minimum time between teleports
    [Range(0, 1)]
    public float baseExhaustCoolDownTime = 0.5f;//the base cool down time (sec) for teleporting while exhausted
    public float exhaustCoolDownTime { get; set; }//the current cool down time (sec) for teleporting while exhausted
    private float teleportTime;//the earliest time that Merky can teleport. To be set only in TeleportReady

    [Header("Components")]
    public BoxCollider2D scoutColliderMin;//small collider (inside Merky) used to scout the level for teleportable spots
    public BoxCollider2D scoutColliderMax;//big collider (outside Merky) used to scout the level for teleportable spots

    [Header("Sound Effects")]
    public AudioClip teleportUnavailableSound;

    [Header("Future Projection")]
    public GameObject futureProjection;//the object that is used to show a preview of the landing spot
    public GameObject teleportPreviewPointer;//the object that visually points at the future projection

    /// <summary>
    /// Returns whether the teleport ability is ready
    /// True: teleport is able to be used
    /// False: teleport is still on cooldown and can't be used
    /// </summary>
    public bool TeleportReady
    {
        get => Time.unscaledTime >= teleportTime;
        set
        {
            if (value)
            {
                teleportTime = 0;
            }
            else
            {
                teleportTime = Time.unscaledTime + Mathf.Max(
                    coolDownTime,
                    exhaustCoolDownTime
                    );
            }
        }
    }

    //
    // State vars
    //
    private float range = 3;//how far Merky can currently teleport
    public float Range
    {
        get => range;
        set
        {
            //Range cannot be zero
            //But it can be greater than base range
            range = Mathf.Max(value, 0);
            //Call range changed delegates
            onRangeChanged?.Invoke(range);
        }
    }
    public delegate void OnRangeChanged(float range);
    public event OnRangeChanged onRangeChanged;

    private PolygonCollider2D pc2d;

    protected override void init()
    {
        base.init();
        pc2d = GetComponent<PolygonCollider2D>();
        playerController.onGroundedStateUpdated += onGroundedChanged;
        //Initialize the range
        Range = baseRange;
    }

    public override void OnDisable()
    {
        base.OnDisable();
        playerController.onGroundedStateUpdated -= onGroundedChanged;
    }

    #region Teleport
    /// <summary>
    /// Teleports, without any checking
    /// </summary>
    /// <param name="targetPos">Position to teleport to in world coordinates</param>
    private void teleport(Vector3 targetPos)
    {
        //If Merky is teleporting from the air,
        if (!playerController.Ground.Grounded)
        {
            //Put the teleport ability on cooldown, longer if teleporting up
            //2017-03-06: copied from https://docs.unity3d.com/Manual/AmountVectorMagnitudeInAnotherDirection.html
            float upAmount = Vector3.Dot(
                (targetPos - transform.position).normalized,
                -playerController.Gravity.Gravity.normalized
                );
            exhaustCoolDownTime = baseExhaustCoolDownTime * upAmount;
        }
        //Put teleport on cooldown
        TeleportReady = false;

        //Store old and new positions
        Vector3 oldPos = transform.position;
        Vector3 newPos = targetPos;

        //Actually Teleport
        transform.position = newPos;

        //Update Stats
        Managers.Stats.addOne("Teleport");

        //Momentum Dampening
        //If Merky is moving,
        if (rb2d.isMoving())
        {
            //Reduce momentum that is going in opposite direction
            Vector3 direction = newPos - oldPos;
            float newX = rb2d.velocity.x;//the new velocity x
            float newY = rb2d.velocity.y;
            //If velocity x is moving opposite of the teleport direction x,
            if (Mathf.Sign(rb2d.velocity.x) != Mathf.Sign(direction.x))
            {
                //Add teleport direction x to velocity x
                newX = rb2d.velocity.x + direction.x;
                //If the addition brought velocity x past 0,
                if (Mathf.Sign(rb2d.velocity.x) != Mathf.Sign(newX))
                {
                    //Keep from exploiting boost in opposite direction
                    newX = 0;
                }
            }
            //If velocity y is moving opposite of the teleport direction y,
            if (Mathf.Sign(rb2d.velocity.y) != Mathf.Sign(direction.y))
            {
                //Add teleport direction y to velocity y
                newY = rb2d.velocity.y + direction.y;
                //If the addition brought velocity y past 0,
                if (Mathf.Sign(rb2d.velocity.y) != Mathf.Sign(newY))
                {
                    //Keep from exploiting boost in opposite direction
                    newY = 0;
                }
            }
            //Update velocity
            rb2d.velocity = new Vector2(newX, newY);
        }

        //Check grounded state
        //have to check it again because state has changed
        playerController.updateGroundedState();

        //On Teleport Effects
        onTeleport?.Invoke(oldPos, newPos);

        //Detach Merky from sticky pads stuck to him
        foreach (FixedJoint2D fj2d in GameObject.FindObjectsOfType<FixedJoint2D>())
        {
            if (fj2d.connectedBody == rb2d)
            {
                Destroy(fj2d);
            }
        }
    }
    public delegate void OnTeleport(Vector2 oldPos, Vector2 newPos);
    public event OnTeleport onTeleport;

    /// <summary>
    /// Finds the teleportable position closest to the given targetPos
    /// </summary>
    /// <param name="targetPos">The ideal position to teleport to</param>
    /// <returns>targetPos if it is teleportable, else the closest teleportable position to it</returns>
    public Vector3 findTeleportablePosition(Vector2 oldPos, Vector2 targetPos)
    {
        //TSFS: Teleport Spot Finding System
        Vector2 newPos = targetPos;
        //If new position is not in range,
        if ((newPos - (Vector2)transform.position).sqrMagnitude > range * range)
        {
            //Shorten it to be within range
            newPos = ((newPos - oldPos).normalized * range) + oldPos;
        }

        if (findTeleportablePositionOverride != null)
        {
            Vector2 newPosOverride = Vector2.zero;
            foreach (FindTeleportablePositionOverride ftpo in findTeleportablePositionOverride.GetInvocationList())
            {
                newPosOverride = ftpo.Invoke(newPos, targetPos);
                //The first one that returns a result
                //that's not (0,0) is accepted
                if (newPosOverride != Vector2.zero)
                {
                    return newPosOverride;
                }
            }
        }

        //Determine if you can teleport to the position
        //(i.e. is it occupied or not?)
        //If the new position is occupied,
        if (isOccupied(newPos, targetPos))
        {
            //Try to adjust it first
            Vector2 adjustedPos = adjustForOccupant(newPos);
            if (!isOccupied(adjustedPos, targetPos))
            {
                return adjustedPos;
            }
            //Search for a good landing spot
            List<Vector3> possibleOptions = new List<Vector3>();
            const int pointsToTry = 5;//try 5 points along the line
            const float difference = 1.00f / pointsToTry;//how much the previous jump was different by
            const float variance = 0.4f;//max amount to adjust angle by
            const int anglesToTry = 7;//try 7 angles off the line
            const float anglesDiff = variance * 2 / (anglesToTry - 1);
            Vector2 normalizedDir = (newPos - oldPos).normalized;
            float oldDist = Vector2.Distance(oldPos, newPos);
            //Vary the angle
            for (float a = -variance; a <= variance; a += anglesDiff)
            {
                //Angle the direction
                Vector2 dir = normalizedDir.RotateZ(a); ;//the direction to search
                Vector2 angledNewPos = oldPos + dir * oldDist;
                //Backtrack from the new position
                float distance = Vector2.Distance(oldPos, angledNewPos);
                Vector2 norm = (angledNewPos - oldPos).normalized;
                norm *= distance;
                for (float percent = 1 + (difference * 2); percent >= 0; percent -= difference)
                {
                    Vector2 testPos = (norm * percent) + oldPos;
                    //If the test position is occupied,
                    if (isOccupied(testPos, targetPos))
                    {
                        //Adjust position based on occupant
                        testPos = adjustForOccupant(testPos);
                        //If the test position is no longer occupied,
                        if (!isOccupied(testPos, targetPos))
                        {
                            //Possible option found
                            possibleOptions.Add(testPos);
                            //If percent distance is in range (0 - 1),
                            //(percent > 1 would put Merky outside his teleport range)
                            if (percent <= 1)
                            {
                                //Try a new angle
                                break;
                            }
                        }
                    }
                    else
                    {
                        //Possible option found
                        possibleOptions.Add(testPos);
                        //If percent distance is in range (0 - 1),
                        //(percent > 1 would put Merky outside his teleport range)
                        if (percent <= 1)
                        {
                            //Try a new angle
                            break;
                        }
                    }
                }
            }
            //Choose the closest option 
            float closestSqrDistance = (newPos - oldPos).sqrMagnitude;
            Vector2 closestOption = oldPos;
            foreach (Vector2 option in possibleOptions)
            {
                float sqrDistance = (newPos - option).sqrMagnitude;
                if (sqrDistance < closestSqrDistance)
                {
                    closestSqrDistance = sqrDistance;
                    closestOption = option;
                }
            }
            return closestOption;
        }
        return newPos;
    }
    public delegate Vector2 FindTeleportablePositionOverride(Vector2 rangedPos, Vector2 tapPos);
    public event FindTeleportablePositionOverride overrideTeleportPosition;
    public event FindTeleportablePositionOverride findTeleportablePositionOverride;

    /// <summary>
    /// Determines whether the given position is occupied or not
    /// </summary>
    /// <param name="testPos">The position to test</param>
    /// <returns>True if something (besides Merky) is in the space, False if the space is clear</returns>
    private bool isOccupied(Vector3 testPos, Vector3 tapPos)
    {
        bool occupied = false;
        Vector3 testOffset = testPos - transform.position;
        testOffset = transform.InverseTransformDirection(testOffset);
        //If there's a max scout collider,
        if (scoutColliderMax)
        {
            //Test with max scout collider
            occupied = isOccupiedImpl(scoutColliderMax, testOffset, testPos, tapPos);
        }
        else
        {
            //Else, assume the space is occupied so that it processes with the other colliders
            occupied = true;
        }
        //If the max scout collider is occupied,
        if (occupied)
        {
            //There's something in or around merky, so
            //Test with min scout collider
            occupied = isOccupiedImpl(scoutColliderMin, testOffset, testPos, tapPos);
            //If the min scout collider is not occupied,
            if (!occupied)
            {
                //There's a possibility the space is clear
                //Test with actual collider
                occupied = isOccupiedImpl(pc2d, testOffset, testPos, tapPos);
            }
        }
        return occupied;
    }
    /// <summary>
    /// isOccupied Step 2. Only meant to be called by isOccupied(Vector3).
    /// </summary>
    private bool isOccupiedImpl(Collider2D coll, Vector3 testOffset, Vector3 testPos, Vector3 tapPos)
    {
        //Find out what objects could be occupying the space
        Vector3 savedOffset = coll.offset;
        coll.offset = testOffset;
        Utility.RaycastAnswer answer;
        answer = coll.CastAnswer(Vector2.zero, 0, true);
        coll.offset = savedOffset;

        //Go through the found objects and see if any actually occupy the space
        for (int i = 0; i < answer.count; i++)
        {
            RaycastHit2D rh2d = answer.rch2ds[i];
            GameObject go = rh2d.collider.gameObject;
            //If the object is not this gameobject,
            if (go != gameObject)
            {
                //And if the object is not a trigger,
                if (!rh2d.collider.isTrigger)
                {
                    //It's occupied!
                    //Yep, it's occupied by an object
                    return true;
                }
                //Else if it is a trigger,
                else
                {
                    //And if it's a hidden area,
                    if (go.CompareTag("NonTeleportableArea"))
                    {
                        //Yep, it's occupied by a hidden area
                        return true;
                    }
                }
            }
        }
        //There were no occupying objects or hidden areas, so
        //Nope, it's not occupied
        return false;
    }

    /// <summary>
    /// Adjusts the given Vector3 to avoid collision with the objects that it collides with
    /// </summary>
    /// <param name="testPos">The Vector3 to adjust</param>
    /// <returns>The Vector3, adjusted to avoid collision with objects it collides with</returns>
    private Vector3 adjustForOccupant(Vector3 testPos)
    {
        //Find the objects that it would collide with
        Vector3 testOffset = testPos - transform.position;
        testOffset = transform.InverseTransformDirection(testOffset);
        Vector3 savedOffset = pc2d.offset;
        pc2d.offset = testOffset;
        Utility.RaycastAnswer answer;
        answer = pc2d.CastAnswer(Vector2.zero, 0, true);
        pc2d.offset = savedOffset;
        //Adjust the move direction for each found object that it collides with
        Vector3 moveDir = Vector3.zero;//the direction to move the testPos
        for (int i = 0; i < answer.count; i++)
        {
            RaycastHit2D rh2d = answer.rch2ds[i];
            GameObject go = rh2d.collider.gameObject;
            //If the game object is not this game object,
            if (go != transform.gameObject)
            {
                //And if the game object is not a trigger,
                if (!rh2d.collider.isTrigger)
                {
                    //Figure out in which direction to move and how far
                    Vector3 outDir = testPos - (Vector3)rh2d.point;
                    //(half width is only an estimate of the dist from the sprite center to its edge)
                    float adjustDistance = playerController.halfWidth - rh2d.distance;
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
                        adjustDistance += playerController.halfWidth;
                    }
                    //Add the calculated direction and magnitude to the running total
                    moveDir += outDir.normalized * adjustDistance;
                }
            }
        }
        return testPos + moveDir;
    }

    #endregion

    #region Input Processing
    public void processTeleport(Vector2 pos)
    {
        //If teleport is not on cooldown,
        if (TeleportReady)
        {
            //Get pre-teleport position
            Vector2 oldPos = transform.position;
            //Override target position
            Vector2 targetPos = pos;
            if (overrideTeleportPosition != null)
            {
                Vector2 newPosOverride = Vector2.zero;
                foreach (FindTeleportablePositionOverride ftpo in overrideTeleportPosition.GetInvocationList())
                {
                    newPosOverride = ftpo.Invoke(pos, pos);
                    //The first one that returns a result
                    //that's not (0,0) is accepted
                    if (newPosOverride != Vector2.zero)
                    {
                        targetPos = newPosOverride;
                        break;
                    }
                }
            }
            //If no override was found,
            if (targetPos == pos)
            {
                //Find a teleportable spot
                targetPos = findTeleportablePosition(oldPos, targetPos);
            }
            //Get post-teleport position
            Vector3 newPos = targetPos;
            //Teleport
            teleport(newPos);
            //Save the game state
            if (playerController.Ground.GroundedPrev)
            {
                Managers.Rewind.Save();
            }
        }
        //Teleport on cooldown
        else
        {
            AudioSource.PlayClipAtPoint(teleportUnavailableSound, pos);
        }
    }

    public void processHoldGesture(Vector2 pos, float holdTime, bool finished)
    {
        //Show a preview of where Merky will teleport
        Vector2 futurePos = findTeleportablePosition(transform.position, pos);
        //Future Projection
        futureProjection.SetActive(true);
        futureProjection.transform.rotation = transform.rotation;
        futureProjection.transform.localScale = transform.localScale;
        futureProjection.transform.position = futurePos;
        //Teleport Preview Pointer
        teleportPreviewPointer.SetActive(true);
        teleportPreviewPointer.transform.localScale = transform.localScale;
        teleportPreviewPointer.transform.position = futurePos;
        //Account for teleport-on-player
        if (playerController.gestureOnPlayer(futurePos))
        {
            float newAngle = playerController.getNextRotation(futureProjection.transform.localEulerAngles.z);
            futureProjection.transform.localEulerAngles = new Vector3(0, 0, newAngle);
        }
    }

    public override void stopGestureEffects()
    {
        futureProjection.SetActive(false);
        teleportPreviewPointer.SetActive(false);
    }
    #endregion

    private void onGroundedChanged(bool grounded, bool groundedNormal)
    {
        //If Merky is grounded,
        if (grounded)
        {
            //Refresh teleport range
            //Check to see if it's less than base range,
            //because we don't want to remove any granted bonus range
            //(such as the one Long Teleport grants)
            if (range < baseRange)
            {
                Range = baseRange;
            }
            //Refresh teleport exhaust cooldowns
            exhaustCoolDownTime = 0;
        }
        //Else if Merky is in the air,
        else
        {
            //Decrease the teleport range
            if (range > exhaustRange)
            {
                Range = exhaustRange;
            }
        }
    }

    protected override void acceptUpgradeLevel(AbilityUpgradeLevel aul)
    {
        baseRange = aul.stat1;
        exhaustRange = aul.stat2;
    }

    public override SavableObject CurrentState
    {
        get => base.CurrentState;
        set
        {
            base.CurrentState = value;
            Range = baseRange;
        }
    }
}
