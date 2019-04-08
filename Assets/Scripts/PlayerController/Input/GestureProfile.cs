using UnityEngine;
using System.Collections;

public class GestureProfile: InputProcessor
{
    /// <summary>
    /// Called when this profile is set to the current one
    /// </summary>
    public virtual void activate() { }
    /// <summary>
    /// Called when the GestureManager switches off this profile to a different one
    /// </summary>
    public virtual void deactivate() { }
    
    public virtual void processTapGesture(Vector2 curMPWorld)
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
    public virtual void processHoldGesture(Vector2 curMPWorld, float holdTime, PlayerInput.InputState state)
    {
        Managers.Player.processHoldGesture(curMPWorld, holdTime, state);
    }
    public virtual void processDragGesture(Vector2 origMPWorld, Vector2 newMPWorld, PlayerInput.InputState state)
    {
        Managers.Camera.processDragGesture(origMPWorld, newMPWorld, state);
    }
    public void processZoomGesture(float zoomMultiplier, PlayerInput.InputState state)
    {
        Managers.Camera.processZoomGesture(zoomMultiplier, state);
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
