using UnityEngine;
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

    public abstract void processHoldGesture(Vector3 curMPWorld, float holdTime, GestureState state);

    public abstract void processDragGesture(Vector3 origMPWorld, Vector3 newMPWorld, GestureInput.DragType dragType, GestureState state);

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
                PlayerPilotController pilot = CheckPointChecker.current.GetComponentInParent<PlayerPilotController>();
                if (pilot && pilot.enabled)
                {
                    Managers.Gesture.switchGestureProfile(GestureManager.GestureProfileType.PILOT);
                }
            }
            else
            {
                if (!Managers.Rewind.Rewinding || Managers.Rewind.rewindInterruptableByPlayer)
                {
                    Managers.Gesture.switchGestureProfile(GestureManager.GestureProfileType.REWIND);
                }
                else
                {
                    Managers.Camera.ZoomScalePoint = CameraController.CameraScalePoints.TIMEREWIND - 1;
                }
            }
        }
        else
        {
            Managers.Gesture.switchGestureProfile(GestureManager.GestureProfileType.MAIN);
        }
    }
}
