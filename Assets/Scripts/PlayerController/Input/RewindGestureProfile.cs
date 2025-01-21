using UnityEngine;
using System.Collections;

public class RewindGestureProfile : GestureProfile
{
    public override void activate()
    {
        //Show Previous Teleport Points
        showPlayerGhostsBasedOnZoom(Managers.Camera.ZoomLevel);
        //Stop rewinding, if possible
        if (Managers.Rewind.Rewinding)
        {
            if (Managers.Rewind.rewindInterruptableByPlayer)
            {
                Managers.Rewind.cancelRewind();
            }
        }
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
        if (state.Finished())
        {
            Managers.PlayerRewind.processTapGesture(curMPWorld);
        }
    }
    public override void processDragGesture(Vector3 origMPWorld, Vector3 newMPWorld, GestureInput.DragType dragType, GestureState state)
    {
        //Drag the camera
        Managers.Camera.processDragGesture(origMPWorld, newMPWorld, state);
    }
    public override void processZoomLevelChange(float zoomLevel)
    {
        base.processZoomLevelChange(zoomLevel);
        showPlayerGhostsBasedOnZoom(zoomLevel);
    }
    private void showPlayerGhostsBasedOnZoom(float zoomLevel)
    {
        float minZoomLevel = Managers.Camera.toZoomLevel(CameraController.CameraScalePoints.TIMEREWIND - 1);
        if (zoomLevel <= minZoomLevel)
        {
            return;
        }
        float maxZoomLevel = Managers.Camera.toZoomLevel(CameraController.CameraScalePoints.TIMEREWIND);
        float percent = (zoomLevel - minZoomLevel) / (maxZoomLevel - minZoomLevel);
        Managers.PlayerRewind.showPlayerGhosts(percent);
    }
}
