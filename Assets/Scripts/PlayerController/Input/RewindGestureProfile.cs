using UnityEngine;
using System.Collections;

public class RewindGestureProfile : GestureProfile
{
    public override void activate()
    {
        //Show Previous Teleport Points
        Managers.Game.showPlayerGhosts(true);
        //Pause game
        Managers.Time.Paused = true;
    }
    public override void deactivate()
    {
        //Unpause
        Managers.Time.Paused = false;
        //Hide Previous Teleport Points
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
            Managers.Gesture.adjustHoldThreshold(holdTime);
        }
    }
}
