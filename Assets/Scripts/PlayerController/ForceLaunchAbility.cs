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
    public float speedMinimum = 0.5f;//if this speed isn't maintained, bounciness will be lost
    public float bouncinessLossDelay = 0.5f;//after this amount of time of being under speed, bounciness will be lost


    public Color unavailableColor = Color.white;//the color the arrow will be when this ability's requirements are not met

    [Header("Components")]
    public GameObject directionIndicatorPrefab;//prefab
    private GameObject directionIndicator;
    private SpriteRenderer directionSR;
    private float originalAlpha;
    public GameObject bouncinessIndicatorPrefab;//prefab
    private GameObject bouncinessIndicator;

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
    private float lastSpeedMetTime = 0;//the last time Merky had met the minimum bounciness speed requirement
    private Vector2 dragPos;

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
        if (affectingVelocity)
        {
            //Nullify velocity
            rb2d.nullifyMovement();
            //Cancel effect on velocity
            affectingVelocity = false;
            updateBouncingVisuals();
        }
    }

    void processDrag(Vector2 oldPos, Vector2 newPos, bool finished)
    {
        Launching = !finished;
        dragPos = newPos;
        LaunchDirection = (Vector2)playerController.transform.position - newPos;
        if (finished && CanLaunch)
        {
            //Save the game state
            Managers.Rewind.Save();
            //Actually launch
            launch();
        }
        updateDirectionVisuals();
    }

    bool dashGroundedCheck()
        => playerController.Ground.isGroundedInDirection(rb2d.velocity);

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
                rb2dColl.velocity = rb2d.velocity;
            }
            //Bounce off the surface
            Vector2 velocity = currentVelocity;
            Vector2 surfaceNormal = collision.GetContact(0).normal;
            Vector2 reflect = Vector2.Reflect(
                velocity,
                surfaceNormal
                ) * bounceEnergyConservationPercent;
            rb2d.velocity = reflect;
            currentVelocity = rb2d.velocity;
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
        currentVelocity = rb2d.velocity;
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
                    affectingVelocity = false;
                    //Save game state
                    Managers.Rewind.Save();
                }
            }
            updateBouncingVisuals();
        }
    }

    /// <summary>
    /// True if the player is grounded
    /// or hasn't teleported since not being grounded
    /// </summary>
    bool CanLaunch =>
        (playerController.Ground.Grounded
        || rb2d.velocity.sqrMagnitude < 0.1f)
        && !Managers.Player.gestureOnPlayer(dragPos);

    void launch()
    {
        //Launch in indicated direction
        rb2d.nullifyMovement();
        float chargePercent = launchDirection.magnitude / maxPullBackDistance;
        rb2d.velocity += launchDirection.normalized * (maxLaunchSpeed * chargePercent);
        //Indicate effect on velocity
        affectingVelocity = true;
        updateBouncingVisuals();
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
        }
    }

    void updateDirectionVisuals()
    {
        if (launching)
        {
            if (directionIndicator == null)
            {
                directionIndicator = Instantiate(directionIndicatorPrefab);
                directionIndicator.transform.parent = transform;
                directionIndicator.transform.localPosition = Vector2.zero;
                directionSR = directionIndicator.GetComponent<SpriteRenderer>();
                originalAlpha = directionSR.color.a;
            }
            directionIndicator.SetActive(true);
            directionIndicator.transform.up = launchDirection;
            directionSR.size = new Vector2(
                1,
                launchDirection.magnitude
                );
            if (CanLaunch)
            {
                directionSR.color = new Color(
                    this.EffectColor.r,
                    this.EffectColor.g,
                    this.EffectColor.b,
                    originalAlpha
                    );
            }
            else
            {
                directionSR.color = unavailableColor;
            }
        }
        else
        {
            directionIndicator?.SetActive(false);
        }
    }

    void updateBouncingVisuals()
    {
        if (affectingVelocity)
        {
            if (bouncinessIndicator == null)
            {
                bouncinessIndicator = Instantiate(bouncinessIndicatorPrefab);
                bouncinessIndicator.transform.parent = transform;
                bouncinessIndicator.transform.localPosition = Vector2.zero;
                SpriteRenderer sr = bouncinessIndicator.GetComponent<SpriteRenderer>();
                sr.color = new Color(
                    this.EffectColor.r,
                    this.EffectColor.g,
                    this.EffectColor.b,
                    sr.color.a
                    );
                //Fixes error when Force Launch not used before rewind in a session
                if (!rb2d)
                {
                    rb2d = GetComponent<Rigidbody2D>();
                }
            }
            bouncinessIndicator.SetActive(true);
            bouncinessIndicator.transform.up = -rb2d.velocity;
        }
        else
        {
            bouncinessIndicator?.SetActive(false);
        }
    }

    public override SavableObject CurrentState
    {
        get => base.CurrentState.more(
            "affectingVelocity", affectingVelocity,
            "currentVelocity", currentVelocity
            );
        set
        {
            base.CurrentState = value;
            affectingVelocity = value.Bool("affectingVelocity");
            currentVelocity = value.Vector2("currentVelocity");
            updateBouncingVisuals();
        }
    }

    protected override void acceptUpgradeLevel(AbilityUpgradeLevel aul)
    {
        maxPullBackDistance = aul.stat1;
        maxLaunchSpeed = aul.stat2;
        bounceEnergyConservationPercent = aul.stat3;
        accelerationBoostPercent = aul.stat4;
    }
}
