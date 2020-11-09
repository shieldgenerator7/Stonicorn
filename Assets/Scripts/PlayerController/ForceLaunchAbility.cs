using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

public class ForceLaunchAbility : PlayerAbility
{
    [Header("Settings")]
    public float maxPullBackDistance = 3;//how far back the player can pull the sling
    public float maxLaunchSpeed = 20;//how fast Merky can go after launching

    [Header("Components")]
    public GameObject directionIndicatorPrefab;//prefab
    private GameObject directionIndicator;
    private SpriteRenderer directionSR;

    private bool launching = false;//true: player is getting ready to launch
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

    protected override void init()
    {
        base.init();
        playerController.onDragGesture += processDrag;
        playerController.Ground.isGroundedCheck += dashGroundedCheck;
    }

    public override void OnDisable()
    {
        base.OnDisable();
        playerController.onDragGesture -= processDrag;
        playerController.Ground.isGroundedCheck -= dashGroundedCheck;
    }

    void processDrag(Vector2 oldPos, Vector2 newPos, bool finished)
    {
        launching = !finished;
        LaunchDirection = (Vector2)playerController.transform.position - newPos;
        if (finished && CanLaunch)
        {
            launch();
        }
        updateDirectionVisuals();
    }

    bool dashGroundedCheck()
    {
        return playerController.Ground.isGroundedInDirection(rb2d.velocity);
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
