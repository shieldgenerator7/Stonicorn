using UnityEngine;
using System.Collections;

public class WallClimbAbility : PlayerAbility
{//2017-03-17: copied from ForceTeleportAbility

    [Header("Settings")]
    public float wallDetectRange = 1.0f;//how far from the center of the old position it should look for a wall
    [Header("Necessary Input")]
    public GameObject stickyPadPrefab;

    protected override void init()
    {
        base.init();
        playerController.isGroundedCheck += isGroundedWall;
        playerController.onTeleport += plantSticky;
    }
    public override void OnDisable()
    {
        base.OnDisable();
        playerController.isGroundedCheck -= isGroundedWall;
        playerController.onTeleport -= plantSticky;
    }

    bool isGroundedWall()
    {
        bool isgrounded = false;
        Vector2 gravity = playerController.Gravity.Gravity;
        //Test right side
        isgrounded = playerController.isGroundedInDirection(
            -gravity.PerpendicularRight()
            );
        if (!isgrounded)
        {
            //Test left side
            isgrounded = playerController.isGroundedInDirection(
                -gravity.PerpendicularLeft()
                );
        }
        return isgrounded;
    }

    protected override void showTeleportEffect(Vector2 oldPos, Vector2 newPos)
    {
        if (playerController.GroundedAbilityPrev)
        {
            base.showTeleportEffect(oldPos, newPos);
        }
    }

    protected override void playTeleportSound(Vector2 oldPos, Vector2 newPos)
    {
        if (playerController.GroundedAbilityPrev)
        {
            base.playTeleportSound(oldPos, newPos);
        }
    }

    /// <summary>
    /// Plants a sticky pad at the oldPos if it's near a wall
    /// </summary>
    /// <param name="oldPos"></param>
    /// <param name="newPos"></param>
    public void plantSticky(Vector2 oldPos, Vector2 newPos)
    {
        if (playerController.GroundedAbilityPrev)
        {
            Vector2 gravity = playerController.Gravity.Gravity;
            //Look right
            plantStickyInDirection(oldPos, -gravity.PerpendicularRight());
            //Look left
            plantStickyInDirection(oldPos, -gravity.PerpendicularLeft());
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
            if (((Vector2)spc.transform.position - stickyPos).sqrMagnitude < minDim * minDim)
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
        }
    }
}
