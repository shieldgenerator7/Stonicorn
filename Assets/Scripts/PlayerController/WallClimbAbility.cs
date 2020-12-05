using UnityEngine;
using System.Collections;
using System.Linq;

public class WallClimbAbility : PlayerAbility
{//2017-03-17: copied from ForceTeleportAbility

    [Header("Settings")]
    public float wallDetectRange = 1.0f;//how far from the center of the old position it should look for a wall
    public float wallMagnetAntiGravity = 0.1f;//how fast Merky should move towards the wall if he's grounded to it
    public float magnetDuration = 2;//how long you stick to the wall after teleporting
    [Header("Necessary Input")]
    public GameObject stickyPadPrefab;
    public GameObject climbSpikesPrefab;//prefab for the visual effect while wall climbing

    private GameObject climbSpikesEffect;

    private bool groundedLeft = false;
    private bool groundedRight = false;
    private bool groundedCeiling = false;

    private float magnetStartTime = -1;
    public bool Magneted
    {
        get => magnetStartTime >= 0;
        set
        {
            if (value)
            {
                magnetStartTime = Managers.Time.Time;
                gravityAccepter.gravityScale = 1 - wallMagnetAntiGravity;
            }
            else
            {
                magnetStartTime = -1;
                gravityAccepter.gravityScale = 1;
            }
            onMagnetChanged?.Invoke(value);
        }
    }
    public delegate void OnMagnetChanged(bool on);
    public event OnMagnetChanged onMagnetChanged;

    private GravityAccepter gravityAccepter;

    protected override void init()
    {
        base.init();
        playerController.Ground.isGroundedCheck += isGroundedAbility;
        playerController.onTeleport += processTeleport;
        gravityAccepter = playerController.Gravity;
        onMagnetChanged -= updateClimbSpikeEffect;
        onMagnetChanged += updateClimbSpikeEffect;
    }
    public override void OnDisable()
    {
        base.OnDisable();
        playerController.Ground.isGroundedCheck -= isGroundedAbility;
        playerController.onTeleport -= processTeleport;
    }

    bool isGroundedAbility()
    {
        //Make sure to check all possible directions
        bool grounded = isGroundedWall();
        grounded = (FeatureLevel >= 1 && isGroundedCeiling()) || grounded;
        return grounded;
    }

    bool isGroundedWall()
    {
        groundedLeft = groundedRight = false;
        Vector2 gravity = gravityAccepter.Gravity;
        //Test left side
        groundedLeft = playerController.Ground.isGroundedInDirection(
            -gravity.PerpendicularLeft()
            );
        //Test right side
        groundedRight = playerController.Ground.isGroundedInDirection(
            -gravity.PerpendicularRight()
            );
        return groundedLeft || groundedRight;
    }
    bool isGroundedCeiling()
    {
        groundedCeiling = false;
        groundedCeiling = playerController.Ground.isGroundedInDirection(
            -gravityAccepter.Gravity
            );
        return groundedCeiling;
    }

    /// <summary>
    /// Should be called after isGroundedWall() gets called
    /// </summary>
    /// <param name="oldPos"></param>
    /// <param name="newPos"></param>
    void processTeleport(Vector2 oldPos, Vector2 newPos)
    {
        if (groundedLeft || groundedRight || groundedCeiling)
        {
            //Update Stats
            Managers.Stats.addOne("WallClimb");
            rb2d.velocity = Vector2.zero;
            Magneted = true;
            //Plant Sticky
            if (FeatureLevel >= 2)
            {
                plantSticky(oldPos);
            }
        }
    }

    private void Update()
    {
        if (Magneted)
        {
            if (Managers.Time.Time <= magnetStartTime + magnetDuration
                && !playerController.Ground.GroundedNormal)
            {
                if (groundedLeft || groundedRight || groundedCeiling)
                {
                    //Update grounding variables
                    isGroundedAbility();
                    //If no longer grounded
                }
                if (!groundedLeft && !groundedRight && !groundedCeiling)
                {
                    //Stop magnet
                    Magneted = false;
                }
            }
            else
            {
                //Stop magnet
                Magneted = false;
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (Active)
        {
            //Updated grounded variables
            isGroundedAbility();
            //If no longer grounded
            if (!groundedLeft && !groundedRight)
            {
                //Stop magnet
                Magneted = false;
            }
        }
    }

    protected override void showTeleportEffect(Vector2 oldPos, Vector2 newPos)
    {
        if (playerController.Ground.GroundedAbilityPrev)
        {
            base.showTeleportEffect(oldPos, newPos);
        }
    }

    protected override void playTeleportSound(Vector2 oldPos, Vector2 newPos)
    {
        if (playerController.Ground.GroundedAbilityPrev)
        {
            base.playTeleportSound(oldPos, newPos);
        }
    }

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
                        cseSR.color = new Color(
                            this.EffectColor.r,
                            this.EffectColor.g,
                            this.EffectColor.b,
                            cseSR.color.a
                            )
                        );
            }
            climbSpikesEffect.SetActive(true);
        }
        else
        {
            climbSpikesEffect?.SetActive(false);
        }
    }

    /// <summary>
    /// Plants a sticky pad at the oldPos if it's near a wall
    /// </summary>
    /// <param name="teleportPos"></param>
    /// <param name="newPos"></param>
    public void plantSticky(Vector2 teleportPos)
    {
        if (playerController.Ground.GroundedAbilityPrev)
        {
            //Get the gravity direction
            Vector2 gravity = playerController.Gravity.Gravity;
            if (groundedLeft)
            {
                //Look left
                plantStickyInDirection(teleportPos, -gravity.PerpendicularLeft());
            }
            if (groundedRight)
            {
                //Look right
                plantStickyInDirection(teleportPos, -gravity.PerpendicularRight());
            }
            if (groundedCeiling)
            {
                //Look up
                plantStickyInDirection(teleportPos, -gravity);
            }
        }
    }
    void plantStickyInDirection(Vector2 pos, Vector2 dir)
    {
        Utility.RaycastAnswer answer = Utility.RaycastAll(pos, dir, wallDetectRange);
        for (int i = 0; i < answer.count; i++)
        {
            RaycastHit2D rch2d = answer.rch2ds[i];
            if (!rch2d.collider.isTrigger && rch2d.collider.gameObject != gameObject)
            {
                spawnSticky(rch2d.point);
                break;
            }
        }
    }
    void spawnSticky(Vector2 stickyPos)
    {
        bool tooClose = false;
        foreach (StickyPadChecker spc in GameObject.FindObjectsOfType<StickyPadChecker>())
        {
            SpriteRenderer spcSR = spc.GetComponent<SpriteRenderer>();
            float minDim = Mathf.Min(spcSR.size.x, spcSR.size.y) / 2;
            if (stickyPos.inRange(spc.transform.position, minDim))
            {
                tooClose = true;
                break;
            }
        }
        if (!tooClose)
        {
            GameObject stickyPad = Utility.Instantiate(stickyPadPrefab);
            stickyPad.GetComponent<StickyPadChecker>().init(playerController.Gravity.Gravity);
            stickyPad.transform.position = stickyPos;
            //Update Stats
            Managers.Stats.addOne("WallClimbSticky");
        }
    }

    protected override void acceptUpgradeLevel(AbilityUpgradeLevel aul)
    {
        wallMagnetAntiGravity = aul.stat1;
        magnetDuration = aul.stat2;
    }
}
