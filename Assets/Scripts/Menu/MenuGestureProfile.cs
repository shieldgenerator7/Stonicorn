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
    public override void processHoldGesture(Vector3 curMPWorld, float holdTime, bool finished)
    {
        if (MenuManager.Open && finished)
        {
            processTapGesture(curMPWorld);
        }
    }
    public override void processDragGesture(Vector3 origMPWorld, Vector3 newMPWorld, GestureInput.DragType dragType, bool finished)
    {
        if (MenuManager.Open && !Managers.Menu.processDragGesture(origMPWorld, newMPWorld))
        {
            //Drag the camera
            Managers.Camera.processDragGesture(origMPWorld, newMPWorld, finished);
        }
    }
}
