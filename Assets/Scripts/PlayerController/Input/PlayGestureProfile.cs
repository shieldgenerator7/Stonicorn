using UnityEngine;
using System.Collections;

public class PlayGestureProfile : GestureProfile
{
    /// <summary>
    /// Called when this profile is set to the current one
    /// </summary>
    public override void activate() { }
    /// <summary>
    /// Called when the GestureManager switches off this profile to a different one
    /// </summary>
    public override void deactivate() { }

    public override void processHoverGesture(Vector2 curMPWorld) { }

    public override void processTapGesture(Vector3 curMPWorld)
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
            Managers.Player.processTapGesture(curMPWorld);
        }
    }
    public override void processHoldGesture(Vector3 curMPWorld, float holdTime, GestureState state)
    {
        if (Managers.Rewind.Rewinding)
        {
            if (state == GestureState.FINISH)
            {
                if (Managers.Rewind.rewindInterruptableByPlayer)
                {
                    Managers.Rewind.cancelRewind();
                }
            }
        }
        else
        {
        Managers.Player.processHoldGesture(curMPWorld, holdTime, state);
        }
    }
    public override void processDragGesture(Vector3 origMPWorld, Vector3 newMPWorld, GestureInput.DragType dragType, GestureState state)
    {
        if (Managers.Rewind.Rewinding)
        {
            return;
        }
        //If the player drags on Merky,
        if (dragType == GestureInput.DragType.DRAG_PLAYER)
        {
            //Activate the ForceLaunch ability
            Managers.Player.processDragGesture(origMPWorld, newMPWorld, state);
        }
        else if (dragType == GestureInput.DragType.DRAG_CAMERA)
        {
            //Drag the camera
            Managers.Camera.processDragGesture(origMPWorld, newMPWorld, state);
        }
        else
        {
            throw new System.ArgumentException("DragType must be a valid value! dragType: " + dragType);
        }
    }
}
