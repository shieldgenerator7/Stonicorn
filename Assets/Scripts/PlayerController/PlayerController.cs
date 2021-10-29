using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Sends control commands to the current Stonicorn
/// </summary>
public class PlayerController : MonoBehaviour
{
    private Stonicorn stonicorn;
    public Stonicorn Stonicorn
    {
        private get => stonicorn;
        set
        {
            if (stonicorn)
            {
                registerStonicornDelegates(false);
            }
            stonicorn = value;
            if (stonicorn)
            {
                registerStonicornDelegates(true);
            }
        }
    }

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

    //
    // Movement Pausing Variables
    //
    private bool shouldPauseMovement = false;//whether or not to pause movement, true after teleport
    private float pauseMovementStartTime = -1;//when Merky last had his movement paused
    private bool hazardHit = false;

    // Use this for initialization
    public void init()
    {
        stonicorn?.Awake();
        //Register the delegates
        registerDelegates();
    }

    #region Delegates and Stonicorn Interfaces

    public float Speed => stonicorn?.Speed ?? 0;
    public float Range => stonicorn?.Teleport.Range ?? 0;
    public float BaseRange => stonicorn?.Teleport.baseRange ?? 0;

    public Vector2 GravityDir => stonicorn?.GravityDir ?? Vector2.zero;

    protected virtual void registerDelegates()
    {
        Managers.Rewind.onRewindFinished += pauseMovementAfterRewind;
        registerStonicornDelegates();
    }
    private void registerStonicornDelegates(bool register = true)
    {
        //unregister
        stonicorn.onTriggerEntered -= onTriggerEntered;
        stonicorn.onCollisionEntered -= OnCollisionEntered;
        stonicorn.Teleport.onTeleport -= onTeleported;
        stonicorn.onAbilityActivated -= onAbilityActivated;
        stonicorn.onAbilityUpgraded -= onAbilityUpgraded;
        stonicorn.onGroundedStateUpdated -= onGroundedStateUpdated;
        stonicorn.Teleport.onRangeChanged -= onRangeChanged;
        stonicorn.Teleport.findTeleportablePositionOverride -= findTeleportablePositionOverride;
        stonicorn.Teleport.onTeleport -= onTeleport;
        //register
        if (register)
        {
            stonicorn.onTriggerEntered += onTriggerEntered;
            stonicorn.onCollisionEntered += OnCollisionEntered;
            stonicorn.Teleport.onTeleport += onTeleported;
            stonicorn.onAbilityActivated += onAbilityActivated;
            stonicorn.onAbilityUpgraded += onAbilityUpgraded;
            stonicorn.onGroundedStateUpdated += onGroundedStateUpdated;
            stonicorn.Teleport.onRangeChanged += onRangeChanged;
            stonicorn.Teleport.findTeleportablePositionOverride += findTeleportablePositionOverride;
            stonicorn.Teleport.onTeleport += onTeleport;
        }
    }

    private void onTriggerEntered(Collider2D coll2D)
    {
        checkMovementPause();//first grounded frame after teleport
    }
    private void OnCollisionEntered(Collision2D coll2D, int hazardDamage)
    {
        if (hazardDamage > 0)
        {
            //Take damage (and rewind)
            forceRewindHazard(hazardDamage, coll2D.contacts[0].point);
        }
        else
        {
            //Grant gravity immunity
            checkMovementPause();//first grounded frame after teleport
        }
    }

    public event Stonicorn.OnAbilityActivated onAbilityActivated;
    public event Stonicorn.OnAbilityUpgraded onAbilityUpgraded;

    public event Stonicorn.OnGroundedStateUpdated onGroundedStateUpdated;

    public event TeleportAbility.OnRangeChanged onRangeChanged;
    public event TeleportAbility.FindTeleportablePositionOverride findTeleportablePositionOverride;
    public event TeleportAbility.OnTeleport onTeleport;

    #endregion

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
            if (stonicorn.Ground.Grounded)
            {
                //Turn off shouldPauseMovement
                shouldPauseMovement = false;
                //Pause Movement
                MovementPaused = true;
            }
        }
    }

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
        if (stonicorn.gestureOnSprite(tapPos))
        {
            //Rotate player ~90 degrees
            stonicorn.rotate();
        }
        //Teleport
        stonicorn.Teleport.processTeleport(tapPos);
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
            stonicorn.Teleport.processHoldGesture(holdPos, holdTime, finished);
            //If this is the last frame of the hold gesture,
            if (finished)
            {
                //Finally teleport to the location
                processTapGesture(holdPos);
                //Erase the teleport preview effects
                stonicorn.Teleport.stopGestureEffects();
            }
        }
    }
    /// <summary>
    /// Erase any visual effects of active abilities
    /// </summary>
    private void dropHoldGesture()
    {
        foreach (StonicornAbility ability in stonicorn.ActiveAbilities)
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


