using System.Collections.Generic;
using System.Linq;
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
    [Range(0, 0.5f)]
    public float autoTeleportDelay = 0.1f;//how long (sec) between each auto teleport using the hold gesture
    public float lastAutoTeleportTime { get; private set; }//the last time that Merky auto teleported using the hold gesture
    [Range(0, 1)]
    public float pauseMovementDuration = 0.2f;//amount of time (sec) Merky's movement is paused after landing
    [Range(0, 3)]
    public float hitStunDuration = 1;//how long merky freezes after getting hit before he auto-rewinds
    public float bigHitThreshold = 50;//how much damage taken at once counts as a "big" hit (ex: the sun)

    [Header("Components")]
    public BoxCollider2D scoutColliderMin;//small collider (inside Merky) used to scout the level for teleportable spots
    public BoxCollider2D scoutColliderMax;//big collider (outside Merky) used to scout the level for teleportable spots

    //
    // Movement Pausing Variables
    //
    private bool shouldPauseMovement = false;//whether or not to pause movement, true after teleport
    private float pauseMovementStartTime = -1;//when Merky last had his movement paused
    private bool hazardHit = false;

    //
    // Runtime Constants
    //
    private float[] rotations = new float[] { 285, 155, 90, 0 };//the default rotations for Merky
    [SerializeField]
    private float _halfWidth = 0;
    public float halfWidth { get => _halfWidth; }//half of Merky's sprite width

    //
    // Components
    //

    private PolygonCollider2D groundedTrigger;//used to determine when Merky is near ground
    private Rigidbody2D rb2d;
    private PolygonCollider2D pc2d;
    public float Speed
        => rb2d.velocity.magnitude;

    public GravityAccepter GravityAccepter { get; private set; }
    public Vector2 GravityDir => GravityAccepter.Gravity;

    public GroundChecker Ground { get; private set; }

    public TeleportAbility Teleport { get; private set; }

    /// <summary>
    /// Returns a list of active abilities
    /// </summary>
    public List<PlayerAbility> ActiveAbilities
        => GetComponents<PlayerAbility>().ToList()
            .FindAll(ability => ability.enabled);

    public void abilityActivated(PlayerAbility ability, bool active)
    {
        onAbilityActivated?.Invoke(ability, active);
    }
    public delegate void OnAbilityActivated(PlayerAbility ability, bool active);
    public event OnAbilityActivated onAbilityActivated;

    public void abilityUpgraded(PlayerAbility ability, int upgradeLevel)
    {
        onAbilityUpgraded?.Invoke(ability, upgradeLevel);
    }
    public delegate void OnAbilityUpgraded(PlayerAbility ability, int upgradeLevel);
    public event OnAbilityUpgraded onAbilityUpgraded;

    // Use this for initialization
    public void init()
    {
        //Retrieve components
        rb2d = GetComponent<Rigidbody2D>();
        Ground = GetComponent<GroundChecker>();
        GravityAccepter = GetComponent<GravityAccepter>();
        pc2d = GetComponent<PolygonCollider2D>();
        //Estimate the halfWidth
        if (_halfWidth == 0)
        {
            Vector3 extents = GetComponent<SpriteRenderer>().bounds.extents;
            _halfWidth = (extents.x + extents.y) / 2;
        }
        //Initialize the ground trigger
        updateGroundTrigger();
        //Teleport Ability
        Teleport = GetComponent<TeleportAbility>();
        //Register the delegates
        registerDelegates();
    }

    protected virtual void registerDelegates()
    {
        Managers.Rewind.onRewindFinished += pauseMovementAfterRewind;
        Teleport.onTeleport += onTeleported;
    }

    /// <summary>
    /// Updates Merky's range when his ground trigger hits ground
    /// </summary>
    /// <param name="coll2D"></param>
    private void OnTriggerEnter2D(Collider2D coll2D)
    {
        if (coll2D.isSolid())
        {
            updateGroundedState();
            checkMovementPause();//first grounded frame after teleport
        }
    }

    /// <summary>
    /// Updates Merky's range when he hits ground
    /// </summary>
    /// <param name="collision"></param>
    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.isSolid())
        {
            updateGroundedState();
            //If collided with a Hazard,
            Hazard hazard = collision.gameObject.GetComponent<Hazard>();
            bool hazardous = hazard && hazard.Hazardous;
            //If any delegate says yes there is an exception,
            //it's no longer a hazard
            Vector2 point = collision.contacts[0].point;
            bool hazardException = hazardous && onHazardHitException != null
                && onHazardHitException.GetInvocationList().ToList()
                .Any(ohhe => (bool)ohhe.DynamicInvoke(point));
            if (hazardous && !hazardException)
            {
                //Take damage (and rewind)
                forceRewindHazard(hazard.DamageDealt, collision.contacts[0].point);
            }
            else
            {
                //Grant gravity immunity
                checkMovementPause();//first grounded frame after teleport
            }
        }
    }
    public delegate bool OnHazardHitException(Vector2 contactPoint);
    /// <summary>
    /// Returns true if there is an exception and the hazard does not hit
    /// </summary>
    public event OnHazardHitException onHazardHitException;

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
            PolygonCollider2D pc2d = GetComponent<PolygonCollider2D>();
            groundedTrigger = gameObject.AddComponent<PolygonCollider2D>();
            groundedTrigger.points = pc2d.points;
            groundedTrigger.isTrigger = true;
        }
        //Move ground trigger to its new position based on the current gravity
        Vector3 offset = GravityDir.normalized * Ground.groundTestDistance;
        groundedTrigger.offset = transform.InverseTransformDirection(offset);
    }

    /// <summary>
    /// True if pausing movement for a time, false if otherwise
    /// </summary>
    public bool MovementPaused
    {
        get => pauseMovementStartTime >= 0;
        set
        {
            if (value)
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
    private void checkMovementPause()
    {
        //And movement should be paused,
        //(such as the first grounded frame after a teleport)
        if (shouldPauseMovement)
        {
            //And Merky is grounded,
            if (Ground.Grounded)
            {
                //Turn off shouldPauseMovement
                shouldPauseMovement = false;
                //Pause Movement
                MovementPaused = true;
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
        float gravityRot = Utility.RotationZ(GravityDir, Vector3.down);
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
    /// Shows a visual teleport effect at the given locations
    /// </summary>
    /// <param name="oldPos">The pre-teleport position</param>
    /// <param name="newPos">The post-teleport position</param>
    public void showTeleportEffect(Vector2 oldPos, Vector2 newPos)
    {
        Managers.Effect.showTeleportStar(oldPos);
    }

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
    public event TeleportAbility.OnTeleport onPlayTeleportSound;

    /// <summary>
    /// Updates variables depending on whether or not Merky is grounded.
    /// Not done in the Grounded property because
    /// sometimes you want to know the grounded state
    /// without changing the rest of Merky's state
    /// </summary>
    internal void updateGroundedState()
    {
        Ground.checkGroundedState();
        //Grounded delegates
        onGroundedStateUpdated?.Invoke(Ground);
    }
    public delegate void OnGroundedStateUpdated(GroundChecker grounder);
    public event OnGroundedStateUpdated onGroundedStateUpdated;//called when grounded becomes true

    private void onTeleported(Vector2 oldPos, Vector2 newPos)
    {
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

        //Show effect
        showTeleportEffect(oldPos, newPos);

        //Play Sound
        playTeleportSound(oldPos, newPos);
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
            Managers.Stats.addOne(Stat.DAMAGED);
            //Start hit timer
            Timer.startTimer(hitStunDuration, () => hitTimerUp(damageToSelf));
            //Highlight impact area
            Managers.Effect.showPointEffect("effect_contact", contactPoint, true, damageToSelf >= bigHitThreshold);
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
    /// Determines whether the given position is occupied or not
    /// </summary>
    /// <param name="testPos">The position to test</param>
    /// <returns>True if something (besides Merky) is in the space, False if the space is clear</returns>
    public bool isOccupied(Vector3 testPos)
    {
        bool occupied = false;
        Vector3 testOffset = testPos - transform.position;
        testOffset = transform.InverseTransformDirection(testOffset);
        //If there's a max scout collider,
        if (scoutColliderMax)
        {
            //Test with max scout collider
            occupied = isOccupiedImpl(scoutColliderMax, testOffset);
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
            occupied = isOccupiedImpl(scoutColliderMin, testOffset);
            //If the min scout collider is not occupied,
            if (!occupied)
            {
                //There's a possibility the space is clear
                //Test with actual collider
                occupied = isOccupiedImpl(pc2d, testOffset);
            }
        }
        return occupied;
    }
    /// <summary>
    /// isOccupied Step 2. Only meant to be called by isOccupied(Vector3).
    /// </summary>
    private bool isOccupiedImpl(Collider2D coll, Vector3 testOffset)
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

    public delegate bool TeleportOverride(Vector2 tapPos);
    public event TeleportOverride teleportOverride;
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
        //Teleport Override
        bool overrideTeleport = teleportOverride?.Invoke(tapPos) ?? false;
        if (!overrideTeleport)
        {
            //Teleport
            Teleport.processTeleport(tapPos);
        }
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
            Teleport.processHoldGesture(holdPos, holdTime, finished);
            //If this is the last frame of the hold gesture,
            if (finished)
            {
                //Finally teleport to the location
                processTapGesture(holdPos);
                //Erase the teleport preview effects
                Teleport.stopGestureEffects();
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
    public event OnDragGesture onDragGesture;
    /// <summary>
    /// Process a drag gesture
    /// </summary>
    /// <param name="origPos"></param>
    /// <param name="newPos"></param>
    public void processDragGesture(Vector3 origPos, Vector3 newPos, bool finished)
    {
        onDragGesture?.Invoke(origPos, newPos, finished);
    }

    void pauseMovementAfterRewind(int gameStateId)
    {
        //Grant gravity immunity
        MovementPaused = true;
    }
}


