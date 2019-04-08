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
    public override void processTapGesture(Vector3 curMPWorld)
    {
        Managers.Game.processTapGesture(curMPWorld);
    }
    public override void processHoldGesture(Vector3 curMPWorld, float holdTime, bool finished)
    {
        if (finished)
        {
            Managers.Game.processTapGesture(curMPWorld);
        }
    }
}
