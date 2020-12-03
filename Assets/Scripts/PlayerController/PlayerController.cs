using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls Merky's teleport ability and other abilities
/// </summary>
public class PlayerController : MonoBehaviour
{
    //
    //Settings
    //
    [Header("Settings")]
    [Range(0, 10)]
    public float baseRange = 3;//the range after touching the ground
    [Range(0, 10)]
    public float exhaustRange = 1;//the range after teleporting into the air (and being exhausted)
    [Range(0, 1)]
    public float coolDownTime = 0.1f;//the minimum time between teleports
    [Range(0, 1)]
    public float baseExhaustCoolDownTime = 0.5f;//the base cool down time (sec) for teleporting while exhausted
    [Range(0, 1)]
    public float pauseMovementDuration = 0.2f;//amount of time (sec) Merky's movement is paused after landing
    [Range(0, 0.5f)]
    public float autoTeleportDelay = 0.1f;//how long (sec) between each auto teleport using the hold gesture
    [Range(0, 3)]
    public float hitStunDuration = 1;//how long merky freezes after getting hit before he auto-rewinds
    //
    //Timer Processing Vars
    //
    private float pauseMovementStartTime = -1;//when Merky last had his movement paused
    private float lastTeleportTime;//the last time that Merky teleported
    private float lastAutoTeleportTime;//the last time that Merky auto teleported using the hold gesture

    private float exhaustCoolDownTime;//the current cool down time (sec) for teleporting while exhausted
    private float teleportTime;//the earliest time that Merky can teleport. To be set only in TeleportReady
    /// <summary>
    /// Returns whether the teleport ability is ready
    /// True: teleport is able to be used
    /// False: teleport is still on cooldown and can't be used
    /// </summary>
    public bool TeleportReady
    {
        get { return Time.unscaledTime >= teleportTime; }
        set
        {
            bool teleportReady = value;
            if (teleportReady)
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
        get { return range; }
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
    public OnRangeChanged onRangeChanged;

    private bool inCheckPoint = false;//whether or not the player is inside a checkpoint
    public bool InCheckPoint
    {
        get { return inCheckPoint; }
        set { inCheckPoint = value; }
    }

    //
    // Movement Pausing Variables
    //
    private bool shouldPauseMovement = false;//whether or not to pause movement, true after teleport
    private bool hazardHit = false;

    //
    // Runtime Constants
    //
    private float[] rotations = new float[] { 285, 155, 90, 0 };//the default rotations for Merky
    public float halfWidth { get; private set; }//half of Merky's sprite width

    //
    // Components
    //
    [Header("Components")]
    public BoxCollider2D scoutColliderMin;//small collider (inside Merky) used to scout the level for teleportable spots
    public BoxCollider2D scoutColliderMax;//big collider (outside Merky) used to scout the level for teleportable spots

    private PolygonCollider2D pc2d;
    private PolygonCollider2D groundedTrigger;//used to determine when Merky is near ground
    private Rigidbody2D rb2d;
    public float Speed
    {
        get
        {
            float speed = rb2d.velocity.magnitude;
            return speed;
        }
    }
    public Vector2 Velocity
    {
        get
        {
            Vector2 velocity = rb2d.velocity;
            return velocity;
        }
    }
    private GravityAccepter gravity;
    public GravityAccepter Gravity
    {
        get
        {
            if (gravity == null)
            {
                gravity = GetComponent<GravityAccepter>();
            }
            return gravity;
        }
    }

    private GroundChecker ground;
    public GroundChecker Ground
    {
        get
        {
            if (ground == null)
            {
                ground = GetComponent<GroundChecker>();
            }
            return ground;
        }
    }

    private TeleportAbility tpa;

    /// <summary>
    /// Returns a list of active abilities
    /// </summary>
    public List<PlayerAbility> ActiveAbilities
    {
        get
        {
            List<PlayerAbility> activeAbilities = new List<PlayerAbility>();
            foreach (PlayerAbility ability in GetComponents<PlayerAbility>())
            {
                if (ability.enabled)
                {
                    activeAbilities.Add(ability);
                }
            }
            return activeAbilities;
        }
    }

    public void abilityActivated(PlayerAbility ability, bool active)
    {
        onAbilityActivated?.Invoke(ability, active);
    }
    public delegate void OnAbilityActivated(PlayerAbility ability, bool active);
    public OnAbilityActivated onAbilityActivated;

    // Use this for initialization
    public void init()
    {
        //Retrieve components
        rb2d = GetComponent<Rigidbody2D>();
        pc2d = GetComponent<PolygonCollider2D>();
        tpa = GetComponent<TeleportAbility>();
        //Register the delegates
        Managers.Rewind.onRewindFinished += pauseMovementAfterRewind;
        //Estimate the halfWidth
        Vector3 extents = GetComponent<SpriteRenderer>().bounds.extents;
        halfWidth = (extents.x + extents.y) / 2;
        //Initialize the range
        Range = baseRange;
        //Initialize the ground trigger
        updateGroundTrigger();
    }

    /// <summary>
    /// Updates Merky's range when his ground trigger hits ground
    /// </summary>
    /// <param name="coll2D"></param>
    private void OnTriggerEnter2D(Collider2D coll2D)
    {
        if (!coll2D.isTrigger)
        {
            checkMovementPause(true);//first grounded frame after teleport
        }
    }

    /// <summary>
    /// Updates Merky's range when he hits ground
    /// </summary>
    /// <param name="collision"></param>
    private void OnCollisionEnter2D(Collision2D collision)
    {
        //If collided with a Hazard,
        Hazard hazard = collision.gameObject.GetComponent<Hazard>();
        if (hazard && hazard.Hazardous)
        {
            //Take damage (and rewind)
            forceRewindHazard(hazard.DamageDealt, collision.contacts[0].point);
        }
        //Else if collided with stand-on-able object,
        else if (!collision.collider.isTrigger)
        {
            //Grant gravity immunity
            checkMovementPause(true);//first grounded frame after teleport
        }
    }

    /// <summary>
    /// Updates the position of the copycat collider that hits the ground before Merky does
    /// This is done to refresh Merky's range slightly before
    /// he actually hits the ground
    /// </summary>
    internal void updateGroundTrigger()
    {
        //If ground trigger is not present,
        if (groundedTrigger == null)
        {
            //Make a new ground trigger collider
            //by copying Merky's collider
            groundedTrigger = gameObject.AddComponent<PolygonCollider2D>();
            groundedTrigger.points = pc2d.points;
            groundedTrigger.isTrigger = true;
        }
        //Move ground trigger to its new position based on the current gravity
        Vector3 offset = Gravity.Gravity.normalized * Ground.groundTestDistance;
        groundedTrigger.offset = transform.InverseTransformDirection(offset);
    }

    /// <summary>
    /// True if pausing movement for a time, false if otherwise
    /// </summary>
    public bool MovementPaused
    {
        get { return pauseMovementStartTime >= 0; }
        set
        {
            bool pauseMovement = value;
            if (pauseMovement)
            {
                if (pauseMovementStartTime < 0)
                {
                    //Slow down time
                    Managers.Time.setPause(this, true);
                }
                pauseMovementStartTime = Time.unscaledTime;
                Timer.startTimerRecyclable(
                    pauseMovementDuration,
                    endMovementPause,
                    gameObject
                    );
            }
            else
            {
                //Resume normal time speed
                Managers.Time.setPause(this, false);
                pauseMovementStartTime = -1;
            }
        }
    }

    private void endMovementPause()
    {
        MovementPaused = false;
    }

    /// <summary>
    /// Turns movement pausing on or off, if conditions are right
    /// </summary>
    /// <param name="checkToTurnOn">Whether to check if should be turned on</param>
    private void checkMovementPause(bool checkToTurnOn)
    {
        //If the caller wants it turned on,
        if (checkToTurnOn)
        {
            //And movement should be paused,
            //(such as the first grounded frame after a teleport)
            if (shouldPauseMovement)
            {
                //And Merky is grounded,
                if (Ground.Grounded)
                {
                    //Updated grounded state
                    updateGroundedState();
                    //Turn off shouldPauseMovement
                    shouldPauseMovement = false;
                    //Pause Movement
                    MovementPaused = true;
                }
            }
        }
        //Else if the caller wants it turned off,
        else
        {
            //And it's currently on,
            if (MovementPaused)
            {
                //And the movement pause time has expired,
                if (Time.unscaledTime >= pauseMovementStartTime + pauseMovementDuration)
                {
                    //Turn off movement pausing
                    MovementPaused = false;
                }
            }
        }
    }

    /// <summary>
    /// Rotates Merky to the next default rotation clockwise
    /// </summary>
    private void rotate()
    {
        float newAngle = getNextRotation(transform.localEulerAngles.z);
        transform.localEulerAngles = new Vector3(0, 0, newAngle);
    }
    /// <summary>
    /// Returns the next default rotation clockwise to the given angle
    /// </summary>
    /// <param name="angleZ">The angle to get the next angle from</param>
    /// <returns>The next default rotation clockwise to the given angle</returns>
    public float getNextRotation(float angleZ)
    {
        //Convert given angle to current gravity space
        float gravityRot = Utility.RotationZ(Gravity.Gravity, Vector3.down);
        float givenRotation = angleZ - gravityRot;
        givenRotation = Utility.loopValue(givenRotation, 0, 360);
        //Figure out which default rotation is closest to given
        int givenRotationIndex = 0;
        float closest = 360;
        for (int i = 0; i < rotations.Length; i++)
        {
            float rotation = rotations[i];
            float diff = Mathf.Abs(rotation - givenRotation);
            diff = Mathf.Min(diff, Mathf.Abs(rotation - (givenRotation - 360)));
            if (diff < closest)
            {
                closest = diff;
                givenRotationIndex = i;
            }
        }
        //Find the next default rotation
        int newRotationIndex = (givenRotationIndex + 1) % rotations.Length;
        //Convert rotation back to global space
        float angle = rotations[newRotationIndex] + gravityRot;
        angle = Utility.loopValue(angle, 0, 360);
        return angle;
    }

    /// <summary>
    /// Teleports, without any checking
    /// </summary>
    /// <param name="targetPos">Position to teleport to in world coordinates</param>
    private void teleport(Vector3 targetPos)
    {
        //If Merky is teleporting from the air,
        if (!Ground.Grounded)
        {
            //Put the teleport ability on cooldown, longer if teleporting up
            //2017-03-06: copied from https://docs.unity3d.com/Manual/AmountVectorMagnitudeInAnotherDirection.html
            float upAmount = Vector3.Dot((targetPos - transform.position).normalized, -Gravity.Gravity.normalized);
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

        //Show effect
        showTeleportEffect(oldPos, newPos);

        //Play Sound
        playTeleportSound(oldPos, newPos);

        //Movement pausing
        //If movement paused,
        if (MovementPaused)
        {
            //Turn it off
            MovementPaused = false;
        }
        //When Merky touches ground next,
        //he should get his movement paused
        shouldPauseMovement = true;

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
        updateGroundedState();

        //reset the ground check trigger's offset to zero,
        //so Unity knows to trigger OnTriggerEnter2D() again in certain cases
        groundedTrigger.offset = Vector2.zero;

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
    public OnTeleport onTeleport;

    /// <summary>
    /// Finds the teleportable position closest to the given targetPos
    /// </summary>
    /// <param name="targetPos">The ideal position to teleport to</param>
    /// <returns>targetPos if it is teleportable, else the closest teleportable position to it</returns>
    public Vector3 findTeleportablePosition(Vector2 targetPos)
    {
        //TSFS: Teleport Spot Finding System
        Vector2 newPos = targetPos;
        Vector2 oldPos = transform.position;
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
    public FindTeleportablePositionOverride findTeleportablePositionOverride;

    /// <summary>
    /// Shows a visual teleport effect at the given locations
    /// </summary>
    /// <param name="oldPos">The pre-teleport position</param>
    /// <param name="newPos">The post-teleport position</param>
    private void showTeleportEffect(Vector2 oldPos, Vector2 newPos)
    {
        Managers.Effect.showTeleportStar(oldPos);
        //Process on show teleport effect delegates
        if (onShowTeleportEffect != null)
        {
            onShowTeleportEffect(oldPos, newPos);
        }
    }
    public OnTeleport onShowTeleportEffect;

    /// <summary>
    /// Plays a teleport sound at the previous position
    /// </summary>
    /// <remarks>
    /// It assumes that there's at least 1 delegate in onPlayTeleportSound
    /// </remarks>
    /// <param name="oldPos">The pre-teleport position</param>
    /// <param name="newPos">The post-teleport position</param>
    private void playTeleportSound(Vector2 oldPos, Vector2 newPos)
    {
        if (onPlayTeleportSound != null)
        {
            onPlayTeleportSound(oldPos, newPos);
        }
        else
        {
            throw new UnityException("No delegates added for playing sound! PlayerController: " + name);
        }
    }
    public OnTeleport onPlayTeleportSound;

    /// <summary>
    /// Updates variables depending on whether or not Merky is grounded.
    /// Not done in the Grounded property because
    /// sometimes you want to know the grounded state
    /// without changing the rest of Merky's state
    /// </summary>
    private void updateGroundedState()
    {
        //If Merky is grounded,
        if (Ground.Grounded)
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
        //Grounded delegates
        onGroundedStateUpdated?.Invoke(Ground.grounded, Ground.groundedNormal);
    }
    public delegate void OnGroundedStateUpdated(bool grounded, bool groundedNormal);
    public OnGroundedStateUpdated onGroundedStateUpdated;//called when grounded becomes true

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
                    if (go.CompareTag("NonTeleportableArea")
                        || (go.transform.parent != null && go.transform.parent.gameObject.CompareTag("NonTeleportableArea")))
                    {
                        //And if it's not a trigger that reveals said hidden area,
                        if (go.GetComponent<SecretAreaTrigger>() == null)
                        {
                            //Yep, it's occupied by a hidden area
                            return true;
                        }
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
                    float adjustDistance = halfWidth - rh2d.distance;
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
                        adjustDistance += halfWidth;
                    }
                    //Add the calculated direction and magnitude to the running total
                    moveDir += outDir.normalized * adjustDistance;
                }
            }
        }
        return testPos + moveDir;
    }

    /// <summary>
    /// Call this to force Merky to rewind due to hitting a hazard
    /// </summary>
    /// <param name="damageToSelf"></param>
    /// <param name="damageToOther"></param>
    /// <param name="contactPoint"></param>
    private void forceRewindHazard(int damageToSelf, Vector2 contactPoint)
    {
        if (damageToSelf > 0)
        {
            //Mark hit
            hazardHit = true;
            //Increment damaged counter
            Managers.Stats.addOne("Damaged");
            //Start hit timer
            Timer.startTimer(hitStunDuration, () => hitTimerUp(damageToSelf));
            //Highlight impact area
            Managers.Effect.showPointEffect("effect_contact", contactPoint);
            //Pause game
            Managers.Time.setPause(this, true);
        }
    }

    private void hitTimerUp(int damageToSelf)
    {
        //Mark not hit
        hazardHit = false;
        //Unpause game
        Managers.Time.setPause(this, false);
        //Remove highlight
        Managers.Effect.showPointEffect("effect_contact", Vector2.zero, false);
        //Rewind
        Managers.Rewind.Rewind(damageToSelf);
    }

    /// <summary>
    /// Returns true if the given Vector3 is on Merky's sprite
    /// </summary>
    /// <param name="pos">The position to check</param>
    /// <returns>True if the position is on Merky's sprite, false if otherwise</returns>
    public bool gestureOnPlayer(Vector2 pos) =>
        pos.inRange(transform.position, halfWidth);

    /// <summary>
    /// Returns true if the given Vector3 is on Merky's sprite
    /// </summary>
    /// <param name="pos">The position to check</param>
    /// <param name="range">How far out to check, default is half of Merky's sprite width</param>
    /// <returns>True if the position is on Merky's sprite, false if otherwise</returns>
    public bool gestureOnPlayer(Vector2 pos, float range) =>
        pos.inRange(transform.position, range);

    /// <summary>
    /// Process the tap gesture at the given position
    /// </summary>
    /// <param name="tapPos">The position to teleport to</param>
    public void processTapGesture(Vector3 tapPos)
    {
        //If the game is paused because Merky is hit,
        if (hazardHit)
        {
            //Don't process input
            return;
        }
        //If the player tapped on Merky,
        if (gestureOnPlayer(tapPos))
        {
            //Rotate player ~90 degrees
            rotate();
        }
        //If teleport is not on cooldown,
        if (TeleportReady)
        {
            //If Merky is in a checkpoint,
            if (inCheckPoint)
            {
                //And the tap pos is on a checkpoint preview,
                foreach (CheckPointChecker cpc in FindObjectsOfType<CheckPointChecker>())
                {
                    if (cpc.checkGhostActivation(tapPos))
                    {
                        //Teleport to that checkpoint
                        processTapGesture(cpc);
                        //Don't process the rest of this method
                        return;
                    }
                }
            }
            //Get pre-teleport position
            Vector3 oldPos = transform.position;
            //Get post-teleport position
            Vector3 newPos = findTeleportablePosition(tapPos);
            //Process onPreTeleport delegates
            _onPreTeleport?.Invoke(oldPos, newPos, tapPos);
            //Teleport
            teleport(newPos);
            //Save the game state
            Managers.Rewind.Save();
            //If Merky is in a checkpoint,
            if (inCheckPoint)
            {
                //Reposition checkpoint previews
                CheckPointChecker.readjustCheckPointGhosts(transform.position);
            }
        }
    }
    //Used when you also need to know where the player tapped
    public delegate void OnPreTeleport(Vector2 oldPos, Vector2 newPos, Vector2 triedPos);
    private OnPreTeleport _onPreTeleport;
    public event OnPreTeleport onPreTeleport
    {
        add
        {
            _onPreTeleport -= value;
            _onPreTeleport += value;
        }
        remove
        {
            _onPreTeleport -= value;
        }
    }

    /// <summary>
    /// Processes the tap gesture on the given checkpoint
    /// </summary>
    /// <param name="checkPoint">The checkpoint to teleport to</param>
    private void processTapGesture(CheckPointChecker checkPoint)
    {
        //Get pre-teleport position
        Vector2 oldPos = transform.position;
        //Get post-teleport position inside of new checkpoint
        Vector3 newPos = checkPoint.getTelepadPosition(CheckPointChecker.current);
        //If pre-processing needs done before teleporting,
        if (_onPreTeleport != null)
        {
            //Pre-process onPreTeleport delegates
            //Pass in newPos for both here because player teleported exactly where they intended to
            _onPreTeleport(oldPos, newPos, newPos);
        }
        //Teleport
        teleport(newPos);
        //Move the camera to Merky's center
        Managers.Camera.recenter();
        //Activate the new checkpoint
        checkPoint.trigger();
        //Save the game state
        Managers.Rewind.Save();
        //NOTE: processTapGesture(Vector3) is NOT called here,
        //because that method cancels the teleport
        //depending on the return value from onPreTeleport(),
        //whereas here, we don't want the teleport canceled
    }

    /// <summary>
    /// Process a hold gesture
    /// </summary>
    /// <param name="holdPos">The current hold position</param>
    /// <param name="holdTime">The current hold duration</param>
    /// <param name="finished">True if this is the last frame of the hold gesture</param>
    public void processHoldGesture(Vector3 holdPos, float holdTime, bool finished)
    {
        //If the camera is centered on the player,
        if (!Managers.Camera.offsetOffPlayer())
        {
            //Rapidly auto-teleport
            //If enough time has passed since the last auto-teleport,
            if (Time.unscaledTime > lastAutoTeleportTime + autoTeleportDelay)
            {
                //Teleport
                lastAutoTeleportTime = Time.unscaledTime;
                processTapGesture(holdPos);
            }
        }
        //Else if the player is on the edge of the screen,
        else
        {
            //Show a teleport preview

            //If this is the first frame of the hold gesture,
            if (holdTime < Time.deltaTime)
            {
                //Erase any visual effects of the other abilities
                dropHoldGesture();
            }
            //Show the teleport preview effect
            tpa.processHoldGesture(holdPos, holdTime, finished);
            //If this is the last frame of the hold gesture,
            if (finished)
            {
                //Finally teleport to the location
                processTapGesture(holdPos);
                //Erase the teleport preview effects
                tpa.stopGestureEffects();
            }
        }
    }
    /// <summary>
    /// Erase any visual effects of active abilities
    /// </summary>
    private void dropHoldGesture()
    {
        foreach (PlayerAbility ability in ActiveAbilities)
        {
            ability.stopGestureEffects();
        }
    }

    public delegate void OnDragGesture(Vector2 origPos, Vector2 newPos, bool finished);
    public OnDragGesture onDragGesture;
    /// <summary>
    /// Process a drag gesture
    /// </summary>
    /// <param name="origPos"></param>
    /// <param name="newPos"></param>
    public void processDragGesture(Vector3 origPos, Vector3 newPos, bool finished)
    {
        onDragGesture?.Invoke(origPos, newPos, finished);
    }

    void pauseMovementAfterRewind(List<GameState> gameStates, int gameStateId)
    {
        //Grant gravity immunity
        MovementPaused = true;
        //Reset range to default
        Range = baseRange;
    }
}


