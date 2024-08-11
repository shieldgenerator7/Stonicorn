﻿using UnityEngine;
using System.Collections;

public abstract class GestureProfile
{
    /// <summary>
    /// Called when this profile is set to the current one
    /// </summary>
    public abstract void activate();

    /// <summary>
    /// Called when the GestureManager switches off this profile to a different one
    /// </summary>
    public abstract void deactivate();

    public abstract void processHoverGesture(Vector2 curMPWorld);

    public abstract void processTapGesture(Vector3 curMPWorld);

    public abstract void processHoldGesture(Vector3 curMPWorld, float holdTime, bool finished);

    public abstract void processDragGesture(Vector3 origMPWorld, Vector3 newMPWorld, GestureInput.DragType dragType, bool finished);

    public virtual void processZoomLevelChange(float zoomLevel)
    {
        //GestureProfile switcher
        if (zoomLevel < Managers.Camera.toZoomLevel(CameraController.CameraScalePoints.MENU + 1))
        {
            Managers.Gesture.switchGestureProfile(GestureManager.GestureProfileType.MENU);
        }
        else if (zoomLevel > Managers.Camera.toZoomLevel(CameraController.CameraScalePoints.TIMEREWIND - 1))
        {
            if (CheckPointChecker.InCheckPoint)
            {
                if (CheckPointChecker.current.GetComponentInParent<PlayerPilotController>())
                {
                    Managers.Gesture.switchGestureProfile(GestureManager.GestureProfileType.PILOT);
                }
            }
            else
            {
                Managers.Gesture.switchGestureProfile(GestureManager.GestureProfileType.REWIND);
            }
        }
        else
        {
            Managers.Gesture.switchGestureProfile(GestureManager.GestureProfileType.MAIN);
        }
    }
}
