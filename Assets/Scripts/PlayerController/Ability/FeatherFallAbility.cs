using UnityEngine;

/// <summary>
/// When holding and unable to teleport, this sways merky in the direction of the cursor
/// </summary>
public class FeatherFallAbility : PlayerAbility
{
    public float swaySpeed = 1;//how much force to apply to influencing left/right movement
    public float maxSpeed = 3;

    protected override void acceptUpgradeLevel(AbilityUpgradeLevel aul)
    {
    }

    protected override bool isGrounded() => false;

    protected override void processTeleport(Vector2 oldPos, Vector2 newPos)
    {
    }

    protected override void registerDelegates(bool register = true)
    {
        Managers.Player.onHoldGesture -= processHoldGesture;
        if (register)
        {
            Managers.Player.onHoldGesture += processHoldGesture;
        }
    }
    private void processHoldGesture(Vector3 holdPos, float holdTime, GestureState state)
    {
        Debug.Log($"feather fall holding {playerController.Teleport.TeleportReady}");
        //if cant teleport,
        if (!playerController.Teleport.TeleportReady)
        {
            //Turn off gravity while holding
            playerController.GravityAccepter.AcceptsGravity = state.Finished();
            //sway fall direction
            rb2d.AddForce((holdPos - transform.position).normalized * swaySpeed);
            //if (rb2d.linearVelocity.magnitude > maxSpeed)
            //{
            //    rb2d.linearVelocity = Vector2.Lerp(rb2d.linearVelocity, rb2d.linearVelocity.normalized*maxSpeed, Time.deltaTime);
            //}
        }
        if (state.Finished())
        {
            playerController.GravityAccepter.AcceptsGravity = true;
        }
    }
}
