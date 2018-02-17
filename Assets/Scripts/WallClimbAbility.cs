using UnityEngine;
using System.Collections;

public class WallClimbAbility : PlayerAbility
{//2017-03-17: copied from ForceTeleportAbility

    public AudioClip wallClimbSound;

    private GravityAccepter gravity;

    protected override void Start()
    {
        base.Start();
        gravity = GetComponent<GravityAccepter>();
        playerController.isGroundedCheck += isGroundedWall;
    }
    public override void OnDisable()
    {
        base.OnDisable();
        playerController.isGroundedCheck -= isGroundedWall;
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
}
