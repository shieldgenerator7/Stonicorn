using UnityEngine;

public class PilotGestureProfile: GestureProfile
{
    public override void activate()
    {
        base.activate();
        Managers.PlayerPilot.activate(true);
    }
    public override void deactivate()
    {
        base.deactivate();
        Managers.PlayerPilot.activate(false);
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
            base.processDragGesture(origMPWorld, newMPWorld, dragType, finished);
        }
    }
}