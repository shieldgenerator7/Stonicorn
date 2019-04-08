﻿using UnityEngine;
using System.Collections;

public class GestureProfile
{
    /// <summary>
    /// Called when this profile is set to the current one
    /// </summary>
    public virtual void activate() { }
    /// <summary>
    /// Called when the GestureManager switches off this profile to a different one
    /// </summary>
    public virtual void deactivate() { }

    public void processTapGesture(CheckPointChecker cpc)
    {
        Managers.Player.processTapGesture(cpc);
    }
    public virtual void processTapGesture(Vector3 curMPWorld)
    {
        if (Managers.Game.Rewinding)
        {
            Managers.Game.cancelRewind();
        }
        else
        {
            Managers.Player.processTapGesture(curMPWorld);
        }
    }
    public virtual void processHoldGesture(Vector3 curMPWorld, float holdTime, bool finished)
    {
        Managers.Player.processHoldGesture(curMPWorld, holdTime, finished);
    }
    public virtual void processDragGesture(Vector3 origMPWorld, Vector3 newMPWorld)
    {
        Managers.Camera.processDragGesture(origMPWorld, newMPWorld);
    }
    public void processZoomGesture(float zoomMultiplier)
    {
        //TODO: Make the camera process it with a processZoomGesture() method
        Managers.Camera.ZoomLevel *= zoomMultiplier;
    }
    public void onZoomLevelChange(float zoomLevel)
    {
        //GestureProfile switcher
        if (zoomLevel < Managers.Camera.toZoomLevel(CameraController.CameraScalePoints.MENU + 1))
        {
            Managers.Gesture.switchGestureProfile(GestureManager.GestureProfileType.MENU);
        }
        else if (!Managers.Player.HardMaterial.isIntact()
            || zoomLevel > Managers.Camera.toZoomLevel(CameraController.CameraScalePoints.TIMEREWIND - 1))
        {
            Managers.Gesture.switchGestureProfile(GestureManager.GestureProfileType.REWIND);
        }
        else
        {
            Managers.Gesture.switchGestureProfile(GestureManager.GestureProfileType.MAIN);
        }
    }
}
