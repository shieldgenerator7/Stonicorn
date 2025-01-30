using UnityEngine;
using System.Collections;

public class MenuGestureProfile : GestureProfile
{//2018-09-15: copied from RewindGestureProfile

    public override void activate()
    {
        MenuManager.Open = true;
        Managers.Camera.Up = Managers.Player.transform.up;
    }
    public override void deactivate()
    {
        MenuManager.Open = false;
        Managers.Camera.Up = -Managers.Player.GravityDir;
    }
    public override void processHoverGesture(Vector2 curMPWorld)
    {
    }
    public override void processTapGesture(Vector3 curMPWorld)
    {
        if (MenuManager.Open)
        {
            Managers.Menu.processTapGesture(curMPWorld);
        }
    }
    public override void processHoldGesture(Vector3 curMPWorld, float holdTime, GestureState state)
    {
        if (MenuManager.Open && state.Finished())
        {
            processTapGesture(curMPWorld);
        }
    }
    public override void processDragGesture(Vector3 origMPWorld, Vector3 newMPWorld, GestureInput.DragType dragType, GestureState state)
    {
        if (MenuManager.Open)
        {
            switch (dragType)
            {
                case GestureInput.DragType.DRAG_CAMERA:
            //Drag the camera
            Managers.Camera.processDragGesture(origMPWorld, newMPWorld, state);
                    break;
                case GestureInput.DragType.DRAG_PLAYER:
                    Managers.Menu.processDragGesture(origMPWorld, newMPWorld, state);
                    break;
                case GestureInput.DragType.UNKNOWN:
                    break;
                default:
                    throw new UnityException($"Unknown value for dragType! {dragType}");
            }
        }
    }
}
