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
    protected override void processTapGesture(Gesture gesture)
    {
        if (MenuManager.Open)
        {
            Managers.Menu.processTapGesture(gesture.position);
        }
    }
    protected override void processHoldGesture(Gesture gesture)
    {
        if (MenuManager.Open && gesture.state == GestureState.FINISHED)
        {
            processTapGesture(gesture);
        }
    }
    protected override void processDragGesture(Gesture gesture)
    {
        if (MenuManager.Open && !Managers.Menu.processDragGesture(gesture.startPosition, gesture.position))
        {
            //Drag the camera
            Managers.Camera.processDragGesture(gesture.startPosition, gesture.position, gesture.state);
        }
    }
}
