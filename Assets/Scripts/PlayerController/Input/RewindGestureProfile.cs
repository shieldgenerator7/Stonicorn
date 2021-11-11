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
    protected override void processHoverGesture(Gesture gesture)
    {
        Managers.PlayerRewind.processHoverGesture(gesture.position);
    }
    protected override void processTapGesture(Gesture gesture)
    {
        Managers.PlayerRewind.processTapGesture(gesture.position);
    }
    protected override void processHoldGesture(Gesture gesture)
    {
        if (gesture.state == GestureState.FINISHED)
        {
            Managers.PlayerRewind.processTapGesture(gesture.position);
        }
    }
    protected override void processDragGesture(Gesture gesture)
    {
        //Drag the camera
        Managers.Camera.processDragGesture(gesture.startPosition, gesture.position, gesture.state);
    }
}
