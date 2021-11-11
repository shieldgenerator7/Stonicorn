using UnityEngine;
using System.Collections;

public class RewindGestureProfile : GestureProfile
{
    public override void activate()
    {
        //Show Previous Teleport Points
        Managers.PlayerRewind.showPlayerGhosts(true);
        //Pause game
        Managers.Time.setPause(Managers.Gesture, true);
    }
    public override void deactivate()
    {
        //Unpause
        Managers.Time.setPause(Managers.Gesture, false);
        //Hide Previous Teleport Points
        Managers.PlayerRewind.showPlayerGhosts(false);
    }
    public override void processHoverGesture(Vector2 curMPWorld)
    {
        Managers.PlayerRewind.processHoverGesture(curMPWorld);
    }
    public override void processTapGesture(Vector3 curMPWorld)
    {
        Managers.PlayerRewind.processTapGesture(curMPWorld);
    }
    public override void processHoldGesture(Vector3 curMPWorld, float holdTime, GestureState state)
    {
        if (state == GestureState.FINISHED)
        {
            Managers.PlayerRewind.processTapGesture(curMPWorld);
        }
    }
    public override void processDragGesture(Vector3 origMPWorld, Vector3 newMPWorld, GestureDragType dragType, GestureState state)
    {
        //Drag the camera
        Managers.Camera.processDragGesture(origMPWorld, newMPWorld, state);
    }
}
