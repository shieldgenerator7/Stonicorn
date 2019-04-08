using UnityEngine;
using System.Collections;

public class MenuGestureProfile : GestureProfile
{//2018-09-15: copied from RewindGestureProfile

    public override void activate()
    {
        MenuManager.Open = true;
        Managers.Camera.setRotation(Managers.Player.transform.up);
    }
    public override void deactivate()
    {
        MenuManager.Open = false;
        Managers.Camera.setRotation(-Managers.Player.Gravity.Gravity);
    }
    public override void processTapGesture(Vector2 curMPWorld)
    {
        Managers.Menu.processTapGesture(curMPWorld);
    }
    public override void processHoldGesture(Vector2 curMPWorld, float holdTime, PlayerInput.InputState state)
    {
        if (state == PlayerInput.InputState.End)
        {
            processTapGesture(curMPWorld);
        }
    }
    public override void processDragGesture(Vector2 origMPWorld, Vector2 newMPWorld, PlayerInput.InputState state)
    {
        if (!Managers.Menu.processDragGesture(origMPWorld, newMPWorld))
        {
            base.processDragGesture(origMPWorld, newMPWorld, state);
        }
    }
}
