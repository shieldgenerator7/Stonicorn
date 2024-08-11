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

    public virtual void processHoverGesture(Vector2 curMPWorld) { }

    public virtual void processTapGesture(Vector3 curMPWorld)
    {
    }
    public virtual void processHoldGesture(Vector3 curMPWorld, float holdTime, bool finished)
    {
    }
    public virtual void processDragGesture(Vector3 origMPWorld, Vector3 newMPWorld, GestureInput.DragType dragType, bool finished)
    {
    }
    public virtual void processZoomLevelChange(float zoomLevel)
    {
    }
}
