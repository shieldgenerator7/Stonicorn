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

    public virtual void processHoverGesture(Vector2 curMPWorld) { }

    public virtual void processTapGesture(Vector3 curMPWorld)
    {
        if (Managers.Rewind.Rewinding)
        {
            if (Managers.Rewind.rewindInterruptableByPlayer)
            {
                Managers.Rewind.cancelRewind();
            }
        }
        else
        {
            Managers.Player.processGesture(new Gesture(curMPWorld));
        }
    }
    public virtual void processHoldGesture(Vector3 curMPWorld, float holdTime, GestureState state)
    {
        Managers.Player.processGesture(new Gesture(curMPWorld, holdTime, state));
    }
    public virtual void processDragGesture(Vector3 origMPWorld, Vector3 newMPWorld, GestureDragType dragType, GestureState state)
    {
        //If the player drags on Merky,
        if (dragType == GestureDragType.PLAYER)
        {
            //Activate the ForceLaunch ability
            Managers.Player.processGesture(new Gesture(origMPWorld, newMPWorld, dragType, state));
        }
        else if (dragType == GestureDragType.CAMERA)
        {
            //Drag the camera
            Managers.Camera.processDragGesture(origMPWorld, newMPWorld, state);
        }
        else
        {
            throw new System.ArgumentException("DragType must be a valid value! dragType: " + dragType);
        }
    }
    public void processZoomLevelChange(float zoomLevel)
    {
        //GestureProfile switcher
        if (zoomLevel < Managers.Camera.toZoomLevel(CameraController.CameraScalePoints.MENU + 1))
        {
            Managers.Gesture.switchGestureProfile(GestureManager.GestureProfileType.MENU);
        }
        else if (zoomLevel > Managers.Camera.toZoomLevel(CameraController.CameraScalePoints.TIMEREWIND - 1))
        {
            Managers.Gesture.switchGestureProfile(GestureManager.GestureProfileType.REWIND);
        }
        else
        {
            Managers.Gesture.switchGestureProfile(GestureManager.GestureProfileType.MAIN);
        }
    }
}
