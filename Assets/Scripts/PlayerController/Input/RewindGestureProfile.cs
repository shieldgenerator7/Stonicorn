using UnityEngine;
using System.Collections;

public class RewindGestureProfile : GestureProfile
{
    public override void activate()
    {
        Managers.Game.showPlayerGhosts();
    }
    public override void deactivate()
    {
        Managers.Game.hidePlayerGhosts();
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
            Managers.Gesture.adjustHoldThreshold(holdTime);
        }
    }
}
