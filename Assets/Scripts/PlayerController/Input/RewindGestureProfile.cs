using UnityEngine;
using System.Collections;

public class RewindGestureProfile : GestureProfile
{
    public override void activate()
    {
        GameManager.showPlayerGhosts();
    }
    public override void deactivate()
    {
        gm.hidePlayerGhosts();
    }
    public override void processTapGesture(Vector3 curMPWorld)
    {
        gm.processTapGesture(curMPWorld);
    }
    public override void processHoldGesture(Vector3 curMPWorld, float holdTime, bool finished)
    {
        if (finished)
        {
            gm.processTapGesture(curMPWorld);
            GameObject.FindObjectOfType<GestureManager>().adjustHoldThreshold(holdTime);
        }
    }
    public override void processZoomLevelChange(float zoomLevel)
    {
        camController.ZoomLevel = zoomLevel;
        //GestureProfile switcher
        if (zoomLevel <= camController.scalePointToZoomLevel((int)CameraController.CameraScalePoints.TIMEREWIND - 1)
            && plrController.isIntact())
        {
            gestureManager.switchGestureProfile("Main");
        }
    }
}
