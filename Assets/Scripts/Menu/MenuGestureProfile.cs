using UnityEngine;
using System.Collections;

public class MenuGestureProfile : GestureProfile
{//2018-09-15: copied from RewindGestureProfile
    private MenuManager menuManager;

    public MenuGestureProfile() : base()
    {
        menuManager = GameObject.FindObjectOfType<MenuManager>();
    }

    public override void activate()
    {
        GameManager.showMainMenu(true);
        Cam.setRotation(plrController.transform.up);
    }
    public override void deactivate()
    {
        GameManager.showMainMenu(false);
        Cam.setRotation(-plrController.Gravity.Gravity);
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
    public override void processDragGesture(Vector3 origMPWorld, Vector3 newMPWorld)
    {
        if (menuManager == null)
        {
            menuManager = GameObject.FindObjectOfType<MenuManager>();
        }
        if (!menuManager.processDragGesture(origMPWorld, newMPWorld))
        {
            base.processDragGesture(origMPWorld, newMPWorld);
        }
    }
    public override void processZoomLevelChange(float zoomLevel)
    {
        Cam.ZoomLevel = zoomLevel;
        //GestureProfile switcher
        if (zoomLevel > Cam.scalePointToZoomLevel(1))
        {
            gestureManager.switchGestureProfile("Main");
        }
    }
}
