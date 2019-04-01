using UnityEngine;
using System.Collections;

public class RewindGestureProfile : GestureProfile
{
    public override void activate()
    {
        gameManager.showPlayerGhosts();
    }
    public override void deactivate()
    {
        gameManager.hidePlayerGhosts();
    }
    public override void processTapGesture(Vector3 curMPWorld)
    {
        gameManager.processTapGesture(curMPWorld);
    }
    public override void processHoldGesture(Vector3 curMPWorld, float holdTime, bool finished)
    {
        if (finished)
        {
            gameManager.processTapGesture(curMPWorld);
            GameObject.FindObjectOfType<GestureManager>().adjustHoldThreshold(holdTime);
        }
    }
    public override void processZoomLevelChange(float zoomLevel)
    {
        Cam.ZoomLevel = zoomLevel;
        //GestureProfile switcher
        if (zoomLevel <= Cam.scalePointToZoomLevel((int)CameraController.CameraScalePoints.TIMEREWIND - 1)
            && plrController.HardMaterial.isIntact())
        {
            gestureManager.switchGestureProfile("Main");
        }
    }
}
