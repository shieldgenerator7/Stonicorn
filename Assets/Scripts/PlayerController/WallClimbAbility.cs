using UnityEngine;
using System.Collections;

public class WallClimbAbility : PlayerAbility
{//2017-03-17: copied from ForceTeleportAbility

    [Header("Settings")]
    public float wallDetectRange = 1.0f;//how far from the center of the old position it should look for a wall
    public float wallMagnetSpeed = 0.5f;//how fast Merky should move towards the wall if he's grounded to it
    [Header("Necessary Input")]
    public GameObject stickyPadPrefab;

    private bool groundedLeft = false;
    private bool groundedRight = false;

    protected override void init()
    {
        base.init();
        playerController.Ground.isGroundedCheck += isGroundedWall;
        playerController.onTeleport += processTeleport;
    }
    public override void OnDisable()
    {
        base.OnDisable();
        playerController.Ground.isGroundedCheck -= isGroundedWall;
        playerController.onTeleport -= processTeleport;
    }

    bool isGroundedWall()
    {
        groundedLeft = groundedRight = false;
        Vector2 gravity = playerController.Gravity.Gravity;
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

    /// <summary>
    /// Should be called after isGroundedWall() gets called
    /// </summary>
    /// <param name="oldPos"></param>
    /// <param name="newPos"></param>
    void processTeleport(Vector2 oldPos, Vector2 newPos)
    {
        if (groundedLeft || groundedRight)
        {
            //plantSticky(newPos);
            rb2d.velocity = Vector2.zero;
        }
    }

    private void Update()
    {
        if (!playerController.Ground.GroundedNormal)
        {
            if (groundedLeft)
            {
                rb2d.AddForce(
                    wallMagnetSpeed
                    * rb2d.mass
                    * -playerController.Gravity.Gravity.PerpendicularLeft()
                    );
            }
            if (groundedRight)
            {
                rb2d.AddForce(
                    wallMagnetSpeed
                    * rb2d.mass
                    * -playerController.Gravity.Gravity.PerpendicularRight()
                    );
            }
            if (groundedLeft || groundedRight)
            {
                //Update grounding variables
                isGroundedWall();
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        //Updated grounded variables
        isGroundedWall();
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

    /// <summary>
    /// Plants a sticky pad at the oldPos if it's near a wall
    /// </summary>
    /// <param name="teleportPos"></param>
    /// <param name="newPos"></param>
    public void plantSticky(Vector2 teleportPos)
    {
        if (playerController.Ground.GroundedAbilityPrev)
        {
            //Update Stats
            GameStatistics.addOne("WallClimb");
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
            GameStatistics.addOne("WallClimbSticky");
        }
    }
}
