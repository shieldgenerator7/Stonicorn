using UnityEngine;
using System.Collections;

public class RewindGestureProfile : GestureProfile
{
    public override void activate()
    {
        Managers.Game.showPlayerGhosts(true);
    }
    public override void deactivate()
    {
        Managers.Game.showPlayerGhosts(false);
    }
    public override void processTapGesture(Vector2 curMPWorld)
    {
        Managers.Game.processTapGesture(curMPWorld);
    }
    public override void processHoldGesture(Vector2 curMPWorld, float holdTime, PlayerInput.InputState state)
    {
        if (state == PlayerInput.InputState.End)
        {
            Managers.Game.processTapGesture(curMPWorld);
        }
    }
}
