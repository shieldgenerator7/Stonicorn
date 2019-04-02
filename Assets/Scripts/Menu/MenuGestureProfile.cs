using UnityEngine;
using System.Collections;

public class MenuGestureProfile : GestureProfile
{//2018-09-15: copied from RewindGestureProfile

    public override void activate()
    {
        GameManager.showMainMenu(true);
        Managers.Camera.setRotation(Managers.Player.transform.up);
    }
    public override void deactivate()
    {
        GameManager.showMainMenu(false);
        Managers.Camera.setRotation(-Managers.Player.Gravity.Gravity);
    }
    public override void processTapGesture(Vector3 curMPWorld)
    {
        Managers.Menu.processTapGesture(curMPWorld);
    }
    public override void processHoldGesture(Vector3 curMPWorld, float holdTime, bool finished)
    {
        if (finished)
        {
            processTapGesture(curMPWorld);
            Managers.Gesture.adjustHoldThreshold(holdTime);
        }
    }
    public override void processDragGesture(Vector3 origMPWorld, Vector3 newMPWorld)
    {
        if (!Managers.Menu.processDragGesture(origMPWorld, newMPWorld))
        {
            base.processDragGesture(origMPWorld, newMPWorld);
        }
    }
    public override void processZoomLevelChange(float zoomLevel)
    {
        Managers.Camera.ZoomLevel = zoomLevel;
        //GestureProfile switcher
        if (zoomLevel > Managers.Camera.scalePointToZoomLevel(1))
        {
            Managers.Gesture.switchGestureProfile("Main");
        }
    }
}
