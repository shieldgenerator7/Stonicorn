using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

//2026-05-22: copied from ForceLaunchAbility
public class FlameDashAbility : PlayerAbility
{
    public bool teleportingRemovesMomentum = false;

    [Header("Activation Settings")]
    public int tapsToActivate = 3;//how many taps are required to activate this ability
    public float minTeleportDistancePerTap = 2.5f;//how far each tap is required to go in order to count
    public bool consecutiveTapsRequired = true;//true: any tap that doesnt count interrupts the entire combo
    public float tapAngleVariance = 5;//in degrees, how varied the taps can be and still count towards being "in the same direction"

    [Header("Ability Settings")]
    public float launchSpeed = 20;//how fast it launches Merky
    public float bounceEnergyConservationPercent = 0.1f;//how much energy to conserve after bouncing
    public float accelerationBoostPercent = 0.5f;//how much speed to add when tapping in the direction of movement
    public float speedMinimum = 0.5f;//if this speed isn't maintained, bounciness will be lost
    public float bouncinessLossDelay = 0.5f;//after this amount of time of being under speed, bounciness will be lost

    [Header("Components")]
    //public GameObject projectilePrefab;
    //public GameObject directionIndicatorPrefab;//prefab
    //private GameObject directionIndicator;//instance
    //private SpriteRenderer directionSR;
    //private float originalAlpha;
    //public GameObject explosionPrefab;

    private List<Vector2> tapDirs = new List<Vector2>();

    private Vector2 launchDirection;
    public Vector2 LaunchDirection
    {
        get => launchDirection;
        private set
        {
            launchDirection = value;
        }
    }

    public Vector2 LaunchVelocity
        => launchDirection.normalized
            * (launchSpeed * 1);

    private Vector2 currentVelocity;//used to recover the velocity when hitting a wall
    private bool affectingVelocity = false;//true if recently launched
    /// <summary>
    /// True if it is on fire and causing the player to be moving
    /// </summary>
    public bool AffectingVelocity
    {
        get => affectingVelocity;
        set
        {
            affectingVelocity = value;
            onAffectingVelocityChanged?.Invoke(affectingVelocity);
        }
    }
    public delegate void OnAffectingVelocityChanged(bool av);
    public event OnAffectingVelocityChanged onAffectingVelocityChanged;
    private float lastSpeedMetTime = 0;//the last time Merky had met the minimum bounciness speed requirement
    private Vector2 dragPos;

    protected override void registerDelegates(bool register = true)
    {
        if (playerController)
        {
            //playerController.onDragGesture -= processDrag;
            if (register)
            {
                //playerController.onDragGesture += processDrag;
            }
        }
    }

    protected override void processTeleport(Vector2 oldPos, Vector2 newPos)
    {
        if (affectingVelocity && teleportingRemovesMomentum)
        {
            //Nullify velocity
            rb2d.nullifyMovement();
            //Cancel effect on velocity
            AffectingVelocity = false;
        }
        tapDirs.Add(newPos - oldPos);
        checkActivation();
    }

    private void checkActivation()
    {
        //get rid of early taps that break the combo
        while (tapDirs.Count > 0)
        {
            bool collinear = tapDirs.All((v) => Vector3.Angle(tapDirs.First(), v) <= tapAngleVariance);
            //if combo is good, stop checking
            if (collinear)
            {
                break;
            }
            //if combo is bad, remove first tap
            else {
                tapDirs.RemoveAt(0);
            }
        }
        //if combo is complete, launch
        if (tapDirs.Count >= 3)
        {
            LaunchDirection = tapDirs.Last().normalized * launchSpeed;
            tapDirs.Clear();
            launch();
        }
    }

    protected override bool isGrounded()
        => affectingVelocity
            || playerController.Ground.isGroundedInDirection(rb2d.linearVelocity);

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (Managers.Rewind.Rewinding)
        {
            return;
        }
        //If this ability contributed to this collision,
        if (affectingVelocity)
        {
            //Push the object in your previous direction
            Rigidbody2D rb2dColl = collision.gameObject.GetComponent<Rigidbody2D>();
            if (rb2dColl)
            {
                rb2dColl.linearVelocity = rb2d.linearVelocity;
            }
            //Bounce off the surface
            Vector2 velocity = currentVelocity;
            Vector2 surfaceNormal = collision.GetContact(0).normal;
            Vector2 reflect = Vector2.Reflect(
                velocity,
                surfaceNormal
                ) * bounceEnergyConservationPercent;
            rb2d.linearVelocity = reflect;
            currentVelocity = rb2d.linearVelocity;
            //Save the game state
            Managers.Rewind.Save();
        }
    }

    void Update()
    {
        if (Managers.Rewind.Rewinding)
        {
            return;
        }
        //Update current velocity
        currentVelocity = rb2d.linearVelocity;
        //Check minimum bounciness speed requirements
        if (affectingVelocity)
        {
            if (currentVelocity.sqrMagnitude >= speedMinimum * speedMinimum)
            {
                lastSpeedMetTime = Managers.Time.Time;
            }
            else
            {
                if (Managers.Time.Time >= lastSpeedMetTime + bouncinessLossDelay)
                {
                    //End this ability's effect on velocity
                    AffectingVelocity = false;
                    //Save game state
                    Managers.Rewind.Save();
                }
            }
        }
    }

    /// <summary>
    /// True if the player is grounded
    /// or hasn't teleported since not being grounded
    /// </summary>
    bool CanLaunch =>
        (playerController.Ground.isGroundedWithoutAbility(this)
        || !rb2d.isMoving())
        && !Managers.Player.gestureOnPlayer(dragPos);

    void launch()
    {
        //Launch in indicated direction
        rb2d.nullifyMovement();
        rb2d.linearVelocity += LaunchVelocity;
        //Indicate effect on velocity
        AffectingVelocity = true;
        //Delegate
        onLaunch?.Invoke();
    }
    public delegate void OnLaunch();
    public event OnLaunch onLaunch;



    /// <summary>
    /// Set on fire without launching
    /// Used for projectile
    /// </summary>
    public void setOnFire()
    {
        AffectingVelocity = true;
    }

    /// <summary>
    /// Speed up in the direction of movement
    /// </summary>
    void speedUp()
    {
        float oldSpeed = rb2d.linearVelocity.magnitude;
        //If there's room to speed up
        if (oldSpeed < launchSpeed)
        {
            //Add velocity in the direction of movement
            rb2d.linearVelocity += (rb2d.linearVelocity.normalized * oldSpeed * accelerationBoostPercent);
            //Reduce speed if too high
            float newSpeed = rb2d.linearVelocity.magnitude;
            if (newSpeed > launchSpeed)
            {
                rb2d.linearVelocity = rb2d.linearVelocity.normalized * launchSpeed;
            }
        }
    }


    static byte key_affectingVelocity = 0;
    static byte key_currentVelocity = 1;
    public override SavableObject CurrentState
    {
        get => base.CurrentState.more(
            key_affectingVelocity, affectingVelocity,
            key_currentVelocity, currentVelocity
            );
        set
        {
            base.CurrentState = value;
            AffectingVelocity = value.Bool(key_affectingVelocity);
            currentVelocity = value.Vector2(key_currentVelocity);
        }
    }

    protected override void acceptUpgradeLevel(AbilityUpgradeLevel aul)
    {
        launchSpeed = aul.stat2;
        bounceEnergyConservationPercent = aul.stat3;
        accelerationBoostPercent = aul.stat4;
    }
}
