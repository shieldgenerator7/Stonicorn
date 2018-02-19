using UnityEngine;
using System.Collections;

public class WallClimbAbility : PlayerAbility
{//2017-03-17: copied from ForceTeleportAbility

    public AudioClip wallClimbSound;
    public GameObject stickyPadPrefab;

    private GravityAccepter gravity;

    protected override void Start()
    {
        base.Start();
        gravity = GetComponent<GravityAccepter>();
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
        isgrounded = playerController.isGrounded(Utility.PerpendicularRight(-gravity.Gravity));//right side
        if (!isgrounded)
        {
            isgrounded = playerController.isGrounded(Utility.PerpendicularLeft(-gravity.Gravity));//left side
        }
        return isgrounded;
    }

    public void playWallClimbEffects(Vector2 pos)
    {
        particleSystem.transform.position = pos;
        particleSystem.Play();
    }

    /// <summary>
    /// Plants a sticky pad at the oldPos if it's near a wall
    /// </summary>
    /// <param name="oldPos"></param>
    /// <param name="newPos"></param>
    public void plantSticky(Vector2 oldPos, Vector2 newPos)
    {
        if (playerController.GroundedPreTeleportAbility)
        {
            GameObject stickyPad = GameObject.Instantiate(stickyPadPrefab);
            stickyPad.GetComponent<StickyPadChecker>().init(gravity.Gravity);
            stickyPad.transform.position = oldPos;
        }
    }
}
