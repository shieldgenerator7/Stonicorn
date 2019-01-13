﻿using UnityEngine;
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
        camController.setRotation(player.transform.localRotation);

        // Disable balistics so the camera doesn't jerk around.
        // You could do some sort of "OnRotationFinished" event or just a timer to have this trigger a little later so the
        // menu doesn't instantly snap into focus, if such behavior is desired.
        camController.disableBalistics = true; 
    }
    public override void deactivate()
    {
        GameManager.showMainMenu(false);

        // We can have our camera balistics again!
        camController.disableBalistics = false;
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
        camController.ZoomLevel = zoomLevel;
        //GestureProfile switcher
        if (zoomLevel > camController.scalePointToZoomLevel(1))
        {
            gestureManager.switchGestureProfile("Main");
        }
    }
}
