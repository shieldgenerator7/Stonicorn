using UnityEngine;
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

    public virtual void processTapGesture(GameObject go)
    {
        Managers.Player.processTapGesture(go);
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
    public void processZoomLevelChange(float zoomLevel)
    {
        //GestureProfile switcher
        if (zoomLevel < Managers.Camera.toZoomLevel(CameraController.CameraScalePoints.MENU + 1))
        {
            Managers.Gesture.switchGestureProfile("Menu");
        }
        else if (!Managers.Player.HardMaterial.isIntact()
            || zoomLevel > Managers.Camera.toZoomLevel(CameraController.CameraScalePoints.TIMEREWIND - 1))
        {
            Managers.Gesture.switchGestureProfile("Rewind");
        }
        else
        {
            Managers.Gesture.switchGestureProfile("Main");
        }
    }
}
