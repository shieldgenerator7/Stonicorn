using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

public class ForceLaunchAbility : PlayerAbility
{
    [Header("Settings")]
    public float maxPullBackDistance = 3;//how far back the player can pull the sling
    public float maxLaunchSpeed = 20;//how fast Merky can go after launching
    public float bounceEnergyConservationPercent = 0.1f;//how much energy to conserve after bouncing
    public float accelerationBoostPercent = 0.5f;//how much speed to add when tapping in the direction of movement

    [Header("Components")]
    public GameObject directionIndicatorPrefab;//prefab
    private GameObject directionIndicator;
    private SpriteRenderer directionSR;

    private bool launching = false;//true: player is getting ready to launch
    public bool Launching
    {
        get => launching;
        set
        {
            //If turning launching off when it's on,
            if (launching && !value)
            {
                //Stop slowing time
                Managers.Time.SlowTime = false;
            }
            //If turning launching on,
            else if (value)
            {
                //Start slowing time
                Managers.Time.SlowTime = true;
            }
            //Set the launching variable
            launching = value;
            
        }
    }
    private Vector2 launchDirection;
    public Vector2 LaunchDirection
    {
        get => launchDirection;
        private set
        {
            launchDirection = value;
            if (launchDirection.magnitude > maxPullBackDistance)
            {
                launchDirection = launchDirection.normalized * maxPullBackDistance;
            }
        }
    }

    private Vector2 currentVelocity;//used to recover the velocity when hitting a wall
    private bool affectingVelocity = false;//true if recently launched

    protected override void init()
    {
        base.init();
        playerController.onTeleport += processTap;
        playerController.onDragGesture += processDrag;
        playerController.Ground.isGroundedCheck += dashGroundedCheck;
    }

    public override void OnDisable()
    {
        base.OnDisable();
        playerController.onTeleport -= processTap;
        playerController.onDragGesture -= processDrag;
        playerController.Ground.isGroundedCheck -= dashGroundedCheck;
    }
    void processTap(Vector2 oldPos, Vector2 newPos)
    {
        //If Merky is moving,
        if (rb2d.velocity.magnitude > 0.1f)
        {
            float angle = Vector2.Angle(
                (newPos - oldPos), 
                rb2d.velocity
                );
            //If the tap was in front of Merky,
            if (angle <= 45)
            {
                //Speed him up
                speedUp();
            }
            else
            {
                //Cancel effect on velocity
                affectingVelocity = false;
            }
        }
    }

    void processDrag(Vector2 oldPos, Vector2 newPos, bool finished)
    {
        if (CanLaunch)
        {
            Launching = !finished;
            LaunchDirection = (Vector2)playerController.transform.position - newPos;
            if (finished)
            {
                //Save the game state
                Managers.Game.Save();
                //Actually launch
                launch();
            }
        }
        else
        {
            Launching = false;
        }
        updateDirectionVisuals();
    }

    bool dashGroundedCheck()
    {
        return playerController.Ground.isGroundedInDirection(rb2d.velocity);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (Managers.Game.Rewinding)
        {
            return;
        }
        //If this ability contributed to this collision,
        if (affectingVelocity)
        {
            //Bounce off the surface
            Vector2 velocity = currentVelocity;
            Vector2 surfaceNormal = collision.GetContact(0).normal;
            Vector2 reflect = Vector2.Reflect(
                velocity,
                surfaceNormal
                ) * bounceEnergyConservationPercent;
            rb2d.velocity = reflect;
        }
    }

    private void Update()
    {
        if (Managers.Game.Rewinding)
        {
            return;
        }
        ////If Merky hardly moved last frame,
        //if (currentVelocity.sqrMagnitude < 0.1f * 0.1f)
        //{
        //    //End this ability's effect on velocity
        //    affectingVelocity = false;
        //}
        //Update current velocity
        currentVelocity = rb2d.velocity;
    }

    /// <summary>
    /// True if the player is grounded
    /// or hasn't teleported since not being grounded
    /// </summary>
    bool CanLaunch =>
        playerController.Ground.Grounded
        || rb2d.velocity.magnitude < 0.1f;

    void launch()
    {
        //Launch in indicated direction
        rb2d.velocity = Vector2.zero;
        float chargePercent = launchDirection.magnitude / maxPullBackDistance;
        rb2d.velocity += launchDirection.normalized * (maxLaunchSpeed * chargePercent);
        //Indicate effect on velocity
        affectingVelocity = true;
    }

    /// <summary>
    /// Speed up in the direction of movement
    /// </summary>
    void speedUp()
    {
        float oldSpeed = rb2d.velocity.magnitude;
        //If there's room to speed up
        if (oldSpeed < maxLaunchSpeed)
        {
            //Add velocity in the direction of movement
            rb2d.velocity += (rb2d.velocity.normalized * oldSpeed * accelerationBoostPercent);
            //Reduce speed if too high
            float newSpeed = rb2d.velocity.magnitude;
            if (newSpeed > maxLaunchSpeed)
            {
                rb2d.velocity = rb2d.velocity.normalized * maxLaunchSpeed;
            }
            //Indicate effect on velocity
            affectingVelocity = true;
        }
    }

    void updateDirectionVisuals()
    {
        if (launching && CanLaunch)
        {
            if (directionIndicator == null)
            {
                directionIndicator = Instantiate(directionIndicatorPrefab);
                directionIndicator.transform.parent = transform;
                directionIndicator.transform.localPosition = Vector2.zero;
                directionSR = directionIndicator.GetComponent<SpriteRenderer>();
                directionSR.color = new Color(
                    this.effectColor.r,
                    this.effectColor.g,
                    this.effectColor.b,
                    directionSR.color.a
                    );
            }
            directionIndicator.SetActive(true);
            directionIndicator.transform.up = launchDirection;
            directionSR.size = new Vector2(
                1,
                launchDirection.magnitude
                );
        }
        else
        {
            directionIndicator?.SetActive(false);
        }
    }
}
