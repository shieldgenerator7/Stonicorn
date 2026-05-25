using UnityEngine;
using System.Collections;
using System.Linq;

/// <summary>
/// When Merky has his feet against a solid non-hazard surface, he clings to it
/// </summary>
public class WallClingAbility : PlayerAbility
{//2026-05-25: copied from WallClimbAbility

    [Header("Settings")]
    public float wallDetectRange = 1.0f;//how far from the center of the old position it should look for a wall
    public float wallMagnetAntiGravity = 0.1f;//how fast Merky should move towards the wall if he's grounded to it
    public Vector2 feetDirection = Vector2.down;
    [Header("Necessary Input")]
    public GameObject climbSpikesPrefab;//prefab for the visual effect while wall climbing

    private GameObject climbSpikesEffect;

    private bool groundedFeet = false;
    private bool prevGroundedFeet = false;

    private bool magneted = false;
    public bool Magneted
    {
        get => magneted;
        set
        {
            magneted = value;
            playerController.GravityAccepter.AcceptsGravity = !magneted;
            onMagnetChanged?.Invoke(value);
        }
    }
    public delegate void OnMagnetChanged(bool on);
    public event OnMagnetChanged onMagnetChanged;


    protected override void registerDelegates(bool register = true)
    {
        onMagnetChanged -= updateClimbSpikeEffect;
        if (register)
        {
            onMagnetChanged += updateClimbSpikeEffect;
        }
    }

    protected override bool isGrounded()
    {
        bool grounded = isGroundedFeet();
        return grounded;
    }

    bool isGroundedFeet()
    {
        prevGroundedFeet = groundedFeet;
        Vector2 feetDir = transform.InverseTransformDirection(feetDirection);
        //Test feet side
        groundedFeet = playerController.Ground.isGroundedInDirection(
            feetDir,
            wallDetectRange
            );
        return groundedFeet;
    }


    /// <summary>
    /// Should be called after isGroundedFeet() gets called
    /// </summary>
    /// <param name="oldPos"></param>
    /// <param name="newPos"></param>
    protected override void processTeleport(Vector2 oldPos, Vector2 newPos)
    {
        if (prevGroundedFeet)
        {
        }
        if (groundedFeet)
        {
            //Update Stats
            Managers.Stats.addOne(Stat.WALL_CLIMB);
            rb2d.nullifyMovement();
            Magneted = true;
            //Effect Teleport
            effectTeleport(oldPos, newPos);
        }
    }

    private void FixedUpdate()
    {
        if (Magneted)
        {
            if (playerController.Ground.GroundedNormal)
            {
                //Stop magnet
                Magneted = false;
            }
            else
            {
                if (groundedFeet)
                {
                    //Update grounding variables
                    isGrounded();
                    if (!groundedFeet)
                    {
                        //Stop magnet
                        Magneted = false;
                    }
                }
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (Active)
        {
            //Updated grounded variables
            isGrounded();
            //If no longer grounded
            if (!groundedFeet)
            {
                //Stop magnet
                Magneted = false;
            }
        }
    }

    protected override void playTeleportSound(Vector2 oldPos, Vector2 newPos)
    {
        if (playerController.Ground.GroundedAbilityPrev)
        {
            base.playTeleportSound(oldPos, newPos);
        }
    }

    //TODO: make this a separate effect script
    private void updateClimbSpikeEffect(bool on)
    {
        if (on)
        {
            if (climbSpikesEffect == null)
            {
                climbSpikesEffect = Instantiate(climbSpikesPrefab, transform);
                climbSpikesEffect.transform.parent = transform;
                climbSpikesEffect.GetComponentsInChildren<SpriteRenderer>().ToList()
                    .ForEach(cseSR =>
                        cseSR.color = this.EffectColor.adjustAlpha(cseSR.color.a)
                        );
            }
            climbSpikesEffect.SetActive(true);
        }
        else
        {
            climbSpikesEffect?.SetActive(false);
        }
    }

    protected override void acceptUpgradeLevel(AbilityUpgradeLevel aul)
    {
        wallMagnetAntiGravity = aul.stat1;
    }
}
