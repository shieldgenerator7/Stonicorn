using UnityEngine;
using System.Collections;

public class MenuGestureProfile : GestureProfile
{//2018-09-15: copied from RewindGestureProfile
    private MenuManager menuManager;

    public override void activate()
    {
        GameManager.showMainMenu(true);
    }
    public override void deactivate()
    {
        GameManager.showMainMenu(false);
    }
    public override void processTapGesture(Vector3 curMPWorld)
    {
        if (menuManager == null)
        {
            menuManager = GameObject.FindObjectOfType<MenuManager>();
        }
        menuManager.processTapGesture(curMPWorld);
    }
    public override void processHoldGesture(Vector3 curMPWorld, float holdTime, bool finished)
    {
        if (finished)
        {
            processTapGesture(curMPWorld);
            GameObject.FindObjectOfType<GestureManager>().adjustHoldThreshold(holdTime);
        }
    }
    public override void processPinchGesture(float zoomLevel)
    {
        camController.ZoomLevel = zoomLevel;
        //GestureProfile switcher
        if (zoomLevel > camController.scalePointToZoomLevel(1))
        {
            gestureManager.switchGestureProfile("Main");
        }
    }
}
