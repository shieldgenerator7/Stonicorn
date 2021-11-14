using UnityEngine;
using System.Collections;

public abstract class GestureProfile
{
    /// <summary>
    /// Called when this profile is set to the current one
    /// </summary>
    public virtual void activate() { }

    /// <summary>
    /// Called when the GestureManager switches off this profile to a different one
    /// </summary>
    public virtual void deactivate() { }

    public void processGesture(Gesture gesture)
    {
        switch (gesture.type)
        {
            case GestureType.TAP:
                processTapGesture(gesture);
                break;
            case GestureType.HOLD:
                processHoldGesture(gesture);
                break;
            case GestureType.DRAG:
                processDragGesture(gesture);
                break;
            case GestureType.HOVER:
                processHoverGesture(gesture);
                break;
        }
    }

    protected virtual void processHoverGesture(Gesture gesture) { }

    protected virtual void processTapGesture(Gesture gesture) { }

    protected virtual void processHoldGesture(Gesture gesture) { }

    protected virtual void processDragGesture(Gesture gesture) { }

    public virtual void processZoomLevelChange(float zoomLevel)
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
