using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    //
    //Settings
    //
    [Header("Settings")]
    public float baseRange = 3;
    public float exhaustRange = 1;
    public int maxAirPorts = 0;
    public float exhaustCoolDownTime = 0.5f;//the cool down time (sec) for teleporting while exhausted
    [Range(0, 1)]
    public float gravityImmuneDuration = 0.2f;//amount of time (sec) Merky is immune to gravity after landing
    public float autoTeleportDelay = 0.1f;//how long (sec) between each auto teleport using the hold gesture
    public float groundTestDistance = 0.25f;//how far from Merky the ground test should go

    //
    //Timer Processing Vars
    //
    private float gravityImmuneStartTime;//when Merky last became immune to gravity
    private float lastAutoTeleportTime;//the last time that Merky auto teleported using the hold gesture

    private float teleportTime;//the earliest time that Merky can teleport
    /// <summary>
    /// Returns whether the teleport ability is ready
    /// True: teleport is able to be used
    /// False: teleport is still on cooldown and can't be used
    /// </summary>
    public bool TeleportReady
    {
        get { return Time.time >= teleportTime; }
    }

    //
    // State vars
    //
    private float range = 3;
    public float Range
    {
        get { return range; }
        set
        {
            range = Mathf.Max(value, 0);
            if (onRangeChanged != null)
            {
                onRangeChanged(range);
            }
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
    //Grounded state variables
    //
    private int airPorts = 0;//"air teleports": how many airports merky has used since touching the ground
    public int AirPortsUsed
    {
        get { return airPorts; }
        set { airPorts = Mathf.Clamp(airPorts, 0, maxAirPorts); }
    }
    private bool grounded = true;
    public bool Grounded
    {
        get
        {
            GroundedPrev = grounded;
            grounded = GroundedNormal;
            //If it's grounded normally,
            if (grounded)
            {
                //It's not going to even check the abilities
                GroundedAbilityPrev = groundedAbility;
                groundedAbility = false;
            }
            else
            {
                //Else, check the abilities
                grounded = GroundedAbility;
            }
            return grounded;
        }
        set
        {
            GroundedPrev = grounded;
            grounded = value;
            if (grounded)
            {
                airPorts = 0;
                if (range < baseRange)
                {
                    Range = baseRange;
                }
            }
            else
            {
                if (airPorts >= maxAirPorts)
                {
                    if (range > exhaustRange)
                    {
                        Range = exhaustRange;
                    }
                }
            }
        }
    }

    private bool groundedNormal = true;
    public bool GroundedNormal
    {
        get
        {
            GroundedNormalPrev = groundedNormal;
            groundedNormal = isGroundedInDirection(Gravity.Gravity);
            return groundedNormal;
        }
    }

    private bool groundedAbility = false;//true if grounded to a wall
    public bool GroundedAbility
    {
        get
        {
            GroundedAbilityPrev = groundedAbility;
            groundedAbility = false;
            if (isGroundedCheck != null)//if nothing found yet and there is an extra ground check to do
            {
                //Check each IsGroundedCheck delegate
                foreach (IsGroundedCheck igc in isGroundedCheck.GetInvocationList())
                {
                    bool result = igc.Invoke();
                    //If at least 1 returns true, Merky is grounded
                    if (result == true)
                    {
                        groundedAbility = true;
                        break;
                    }
                }
            }
            return groundedAbility;
        }
    }
    public delegate bool IsGroundedCheck();
    public IsGroundedCheck isGroundedCheck;

    private bool groundedPrev;
    public bool GroundedPrev
    {
        get { return groundedPrev; }
        private set { groundedPrev = value; }
    }
    private bool groundedNormalPrev;
    public bool GroundedNormalPrev
    {
        get { return groundedNormalPrev; }
        private set { groundedNormalPrev = value; }
    }
    private bool groundedAbilityPrev;
    public bool GroundedAbilityPrev
    {
        get { return groundedAbilityPrev; }
        private set { groundedAbilityPrev = value; }
    }

    //
    // Gravity Immunity Variables
    //
    private bool shouldGrantGIT = false;//whether or not to grant gravity immunity, true after teleport
    private Vector2 savedVelocity;
    private float savedAngularVelocity;

    //
    // Runtime Constants
    //
    private float[] rotations = new float[] { 285, 155, 90, 0 };
    private float halfWidth;//half of Merky's sprite width

    //
    // Components
    //
    [Header("Components")]
    public BoxCollider2D scoutColliderMin;//collider used to scout the level for teleportable spots
    public BoxCollider2D scoutColliderMax;//collider used to scout the level for teleportable spots

    private Rigidbody2D rb2d;
    private PolygonCollider2D pc2d;
    private PolygonCollider2D groundedTrigger;//used to determine when Merky is near ground
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


    private HardMaterial hardMaterial;
    public HardMaterial HardMaterial
    {
        get
        {
            if (hardMaterial == null)
            {
                hardMaterial = GetComponent<HardMaterial>();
            }
            return hardMaterial;
        }
    }

    private TeleportAbility tpa;

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

    // Use this for initialization
    private void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
        pc2d = GetComponent<PolygonCollider2D>();
        HardMaterial.shattered += shattered;
        tpa = GetComponent<TeleportAbility>();
        //Estimate the halfWidth
        Vector3 extents = GetComponent<SpriteRenderer>().bounds.extents;
        halfWidth = (extents.x + extents.y) / 2;
        //
        onPreTeleport += canTeleport;
        Range = baseRange;
        updateGroundTrigger();
    }

    private void FixedUpdate()
    {
        checkGravityImmunity(false);
        updateGroundTrigger();
    }
    private void OnTriggerEnter2D(Collider2D coll2D)
    {
        if (!coll2D.isTrigger)
        {
            checkGravityImmunity(true);//first grounded frame after teleport
        }
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.collider.isTrigger)
        {
            checkGravityImmunity(true);//first grounded frame after teleport
        }
    }

    private void updateGroundTrigger()
    {
        if (groundedTrigger == null)
        {
            //PC2D ground trigger
            groundedTrigger = gameObject.AddComponent<PolygonCollider2D>();
            groundedTrigger.points = pc2d.points;
            groundedTrigger.isTrigger = true;
        }
        //Move triggerPC2D to its new position based on the current gravity
        Vector3 offset = Gravity.Gravity.normalized * groundTestDistance;
        groundedTrigger.offset = transform.InverseTransformDirection(offset);
    }

    public bool GravityImmune
    {
        get { return gravityImmuneStartTime > 0; }
        set
        {
            bool grantGravityImmunity = value;
            if (grantGravityImmunity)
            {
                gravityImmuneStartTime = Time.time;
                Gravity.AcceptsGravity = false;
                savedVelocity = rb2d.velocity;
                rb2d.velocity = Vector2.zero;
                savedAngularVelocity = rb2d.angularVelocity;
                rb2d.angularVelocity = 0;
            }
            else
            {
                gravityImmuneStartTime = 0;
                Gravity.AcceptsGravity = true;
                rb2d.velocity = savedVelocity;
                savedVelocity = Vector2.zero;
                rb2d.angularVelocity = savedAngularVelocity;
                savedAngularVelocity = 0;
            }
        }
    }
    /// <summary>
    /// Updates gravity immunity
    /// </summary>
    private void checkGravityImmunity(bool checkToTurnOn)
    {
        if (checkToTurnOn)
        {
            if (shouldGrantGIT)
            {
                if (Grounded)
                {
                    checkGroundedState();
                    shouldGrantGIT = false;
                    GravityImmune = true;
                }
            }
        }
        else
        {
            if (GravityImmune)
            {
                if (Time.time >= gravityImmuneStartTime + gravityImmuneDuration)
                {
                    GravityImmune = false;
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
    public float getNextRotation(float angleZ)
    {
        int closestRotationIndex = 0;
        //Figure out current rotation
        float gravityRot = Utility.RotationZ(Gravity.Gravity, Vector3.down);
        float currentRotation = angleZ - gravityRot;
        currentRotation = Utility.loopValue(currentRotation, 0, 360);
        //Figure out which default rotation is closest
        float closest = 360;
        for (int i = 0; i < rotations.Length; i++)
        {
            float rotation = rotations[i];
            float diff = Mathf.Abs(rotation - currentRotation);
            diff = Mathf.Min(diff, Mathf.Abs(rotation - (currentRotation - 360)));
            if (diff < closest)
            {
                closest = diff;
                closestRotationIndex = i;
            }
        }
        int newRotationIndex = closestRotationIndex + 1;
        if (newRotationIndex >= rotations.Length)
        {
            newRotationIndex = 0;
        }
        //Set rotation
        float angle = rotations[newRotationIndex] + gravityRot;
        angle = Utility.loopValue(angle, 0, 360);
        return angle;
    }
    /// <summary>
    /// Checks to make sure teleport is not on cooldown
    /// </summary>
    /// <param name="oldPos"></param>
    /// <param name="newPOs"></param>
    /// <param name="triedPos"></param>
    /// <returns></returns>
    private bool canTeleport(Vector2 oldPos, Vector2 newPos, Vector2 triedPos)
    {
        if (!Grounded)
        {
            if (!TeleportReady)
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Teleports, without any checking
    /// </summary>
    /// <param name="targetPos">Position to teleport to in world coordinates</param>
    private void teleport(Vector3 targetPos)
    {
        //Update mid-air cooldowns
        if (!Grounded)
        {
            airPorts++;
        }
        if (airPorts >= maxAirPorts)
        {
            //Put the teleport ability on cooldown, longer if teleporting up
            //2017-03-06: copied from https://docs.unity3d.com/Manual/AmountVectorMagnitudeInAnotherDirection.html
            float upAmount = Vector3.Dot((targetPos - transform.position).normalized, -Gravity.Gravity.normalized);
            teleportTime = Time.time + exhaustCoolDownTime * upAmount;
        }

        //Store old and new positions
        Vector3 oldPos = transform.position;
        Vector3 newPos = targetPos;

        //Actually Teleport
        transform.position = newPos;

        //Show effect
        showTeleportEffect(oldPos, newPos);

        //Play Sound
        playTeleportSound(oldPos, newPos);

        //Health Regen
        HardMaterial.addIntegrity(Vector2.Distance(oldPos, newPos));

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
                    //keep from exploiting boost in opposite direction
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
                    //keep from exploiting boost in opposite direction
                    newY = 0;
                }
            }
            //Update velocity
            rb2d.velocity = new Vector2(newX, newY);
        }

        //Gravity Immunity
        shouldGrantGIT = true;

        //Check grounded state
        //have to check it again because state has changed
        checkGroundedState();

        //reset the ground check trigger's offset to zero,
        //so Unity knows to trigger OnTriggerEnter2D() again in certain cases
        groundedTrigger.offset = Vector2.zero;

        //On Teleport Effects
        if (onTeleport != null)
        {
            onTeleport(oldPos, newPos);
        }

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
    /// <param name="targetPos"></param>
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

        //Determine if you can teleport to the position
        //(i.e. is it occupied or not?)
        //If the new position is occupied,
        if (isOccupied(newPos))
        {
            //Try to adjust it first
            Vector2 adjustedPos = adjustForOccupant(newPos);
            if (!isOccupied(adjustedPos))
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
                    if (isOccupied(testPos))
                    {
                        //adjust pos based on occupant
                        testPos = adjustForOccupant(testPos);
                        //If the test position is no longer occupied,
                        if (!isOccupied(testPos))
                        {
                            //Possible option found
                            possibleOptions.Add(testPos);
                            //If percent is in range (0 - 1)
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
                        //found an open spot (tho it might not be optimal)
                        possibleOptions.Add(testPos);
                        //If percent is in range (0 - 1)
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

    private void checkGroundedState()
    {
        Grounded = Grounded;
    }

    public bool isGroundedInDirection(Vector3 direction)
    {
        Utility.RaycastAnswer answer;
        answer = pc2d.CastAnswer(direction, groundTestDistance, true);
        for (int i = 0; i < answer.count; i++)
        {
            RaycastHit2D rch2d = answer.rch2ds[i];
            if (!rch2d.collider.isTrigger)
            {
                GameObject ground = rch2d.collider.gameObject;
                if (!ground.Equals(transform.gameObject))
                {
                    return true;
                }
            }
        }
        return false;
    }

    /// <summary>
    /// Determines whether the given position is occupied or not
    /// </summary>
    /// <param name="testPos">The position to test</param>
    /// <returns>True if there is something in the space, False if the space is clear</returns>
    private bool isOccupied(Vector3 testPos)
    {
        bool occupied = false;
        Vector3 testOffset = testPos - transform.position;
        testOffset = transform.InverseTransformDirection(testOffset);
        //If there's a max scout collider,
        if (scoutColliderMax)
        {
            //Test with max scout collider
            occupied = isOccupiedImpl(scoutColliderMax, testOffset, testPos);
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
            occupied = isOccupiedImpl(scoutColliderMin, testOffset, testPos);
            //If the min scout collider is not occupied,
            if (!occupied)
            {
                //There's a possibility the space is clear
                //Test with actual collider
                occupied = isOccupiedImpl(pc2d, testOffset, testPos);
            }
        }
        return occupied;
    }
    /// <summary>
    /// isOccupied Step 2. Only meant to be called by isOccupied(Vector3).
    /// </summary>
    private bool isOccupiedImpl(Collider2D coll, Vector3 testOffset, Vector3 testPos)
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
                    //unless...
                    //If there are possible exceptions,
                    //(such as if Switch Teleport is active)
                    if (isOccupiedException != null)
                    {
                        //Check each isOccupiedCatch delegate for an exception
                        foreach (IsOccupiedException ioc in isOccupiedException.GetInvocationList())
                        {
                            //Make it do what it needs to do, then return the result
                            bool result = ioc.Invoke(rh2d.collider, testPos);
                            //If at least 1 returns true, it's considered not occupied
                            if (result == true)
                            {
                                return false;//return false for "not occupied"
                            }
                        }
                    }
                    //Nope, no exceptions, so
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
    public delegate bool IsOccupiedException(Collider2D coll, Vector3 testPos);
    public IsOccupiedException isOccupiedException;

    /// <summary>
    /// Adjusts the given Vector3 to avoid collision with the objects that it collides with
    /// </summary>
    /// <param name="testPos">The Vector3 to adjust</param>
    /// <returns>The Vector3, adjusted to avoid collision with objects it collides with</returns>
    private Vector3 adjustForOccupant(Vector3 testPos)
    {
        Vector3 moveDir = Vector3.zero;//the direction to move the testPos
        //Find the objects that it would collide with
        Vector3 testOffset = testPos - transform.position;
        testOffset = transform.InverseTransformDirection(testOffset);
        Vector3 savedOffset = pc2d.offset;
        pc2d.offset = testOffset;
        Utility.RaycastAnswer answer;
        answer = pc2d.CastAnswer(Vector2.zero, 0, true);
        pc2d.offset = savedOffset;
        //Adjust the move direction for each found object that it collides with
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
    /// Delegate method called when HardMaterial's integrity reaches 0
    /// </summary>
    private void shattered()
    {
        //Put the gesture manager in rewind mode
        Managers.Gesture.switchGestureProfile("Rewind");
        //Let the game manager know the player died
        Managers.Game.playerShattered();
        //Increment death counter
        GameStatistics.incrementCounter("deathCount");
        //If this is the first death,
        if (GameStatistics.counter("deathCount") == 1)
        {
            //Highlight the past preview that makes the most sense to rewind to
            Vector2 lsrgp = Managers.Game.getLatestSafeRewindGhostPosition();
            transform.position = ((Vector2)transform.position + lsrgp) / 2;
            Managers.Effect.highlightTapArea(lsrgp);
        }
    }

    /// <summary>
    /// Returns true if the given Vector3 is on Merky's sprite
    /// </summary>
    /// <param name=""></param>
    /// <returns></returns>
    public bool gestureOnPlayer(Vector3 pos)
    {
        return (pos - transform.position).sqrMagnitude < halfWidth * halfWidth;
    }

    public void processTapGesture(Vector3 tapPos)
    {
        if (gestureOnPlayer(tapPos))
        {
            //Rotate player 90 degrees
            rotate();
        }
        Vector3 prevPos = transform.position;
        Vector3 newPos = findTeleportablePosition(tapPos);
        bool continueTeleport = true;
        if (onPreTeleport != null)
        {
            //Check each onPreTeleport delegate
            foreach (OnPreTeleport opt in onPreTeleport.GetInvocationList())
            {
                //Make it do what it needs to do, then return the result
                bool result = opt.Invoke(prevPos, newPos, tapPos);
                //If at least 1 returns false, don't teleport
                if (result == false)
                {
                    continueTeleport = false;
                    break;
                }
            }
        }
        if (continueTeleport)
        {
            teleport(newPos);
            Managers.Game.Save();
            //Reposition checkpoint previews
            if (inCheckPoint)
            {
                CheckPointChecker.readjustCheckPointGhosts(transform.position);
            }
        }
    }
    //Used when you also need to know where the user clicked, and may need to stop the teleport
    public delegate bool OnPreTeleport(Vector2 oldPos, Vector2 newPos, Vector2 triedPos);
    public OnPreTeleport onPreTeleport;

    public void processTapGesture(GameObject checkPoint)
    {
        CheckPointChecker cpc = checkPoint.GetComponent<CheckPointChecker>();
        if (cpc != null)
        {
            Vector2 oldPos = transform.position;
            Vector3 offset = transform.position - CheckPointChecker.current.transform.position;
            Vector3 newPos = checkPoint.transform.position + offset;
            if (onPreTeleport != null)
            {
                //Pass in newPos for both here because player teleported exactly where they intended to
                onPreTeleport(oldPos, newPos, newPos);
            }
            teleport(newPos);
            Managers.Camera.recenter();
            cpc.trigger();
            Managers.Game.Save();
        }
    }


    public void processHoldGesture(Vector3 holdPos, float holdTime, bool finished)
    {
        float reducedHoldTime = holdTime - Managers.Gesture.HoldThreshold;
        //If the camera is centered on the player,
        if (!Managers.Camera.offsetOffPlayer())
        {
            //Rapidly auto-teleport
            //If enough time has passed since the last auto-teleport,
            if (Time.time > lastAutoTeleportTime + autoTeleportDelay)
            {
                //Teleport
                lastAutoTeleportTime = Time.time;
                processTapGesture(holdPos);
            }
        }
        //Else if the player is on the edge of the screen,
        else
        {
            //Show a teleport preview

            //If this is the first counted frame of the hold gesture,
            if (reducedHoldTime < Time.deltaTime)
            {
                //Erase any visual effects of the other abilities
                dropHoldGesture();
            }
            //Show the teleport preview effect
            tpa.processHoldGesture(holdPos, reducedHoldTime, finished);
            //If this is the last frame of the hold gesture,
            if (finished)
            {
                //Teleport to the location
                processTapGesture(holdPos);
                //Erase the teleport preview effects
                tpa.dropHoldGesture();
            }
        }
    }
    private void dropHoldGesture()
    {
        foreach (PlayerAbility ability in ActiveAbilities)
        {
            ability.dropHoldGesture();
        }
    }
}


