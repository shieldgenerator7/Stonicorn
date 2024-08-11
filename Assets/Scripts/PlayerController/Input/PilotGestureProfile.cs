using UnityEngine;

public class PilotGestureProfile : GestureProfile
{
    public override void activate()
    {
        PlayerPilotController pilot = CheckPointChecker.current?.GetComponentInParent<PlayerPilotController>();
        if (pilot)
        {
            pilot.activate(true);
        }
        else
        {
            deactivate();
        }
    }
    public override void deactivate()
    {
        CheckPointChecker.current?.GetComponentInParent<PlayerPilotController>()?.activate(false);
    }

    public override void processHoverGesture(Vector2 curMPWorld)
    {
    }

    public override void processTapGesture(Vector3 curMPWorld)
    {
        Managers.PlayerPilot.processTapGesture(curMPWorld);
    }

    public override void processHoldGesture(Vector3 curMPWorld, float holdTime, bool finished)
    {
        Managers.PlayerPilot.processHoldGesture(curMPWorld, holdTime, finished);
    }

    public override void processDragGesture(Vector3 origMPWorld, Vector3 newMPWorld, GestureInput.DragType dragType, bool finished)
    {
        //If the player drags on Merky,
        if (dragType == GestureInput.DragType.DRAG_PLAYER)
        {
            //Activate the ForceLaunch ability
            Managers.PlayerPilot.processDragGesture(origMPWorld, newMPWorld, finished);
        }
        else if (dragType == GestureInput.DragType.DRAG_CAMERA)
        {
            //Drag the camera
            Managers.Camera.processDragGesture(origMPWorld, newMPWorld, finished);
        }
        else
        {
            throw new System.ArgumentException("DragType must be a valid value! dragType: " + dragType);
        }
    }
}
