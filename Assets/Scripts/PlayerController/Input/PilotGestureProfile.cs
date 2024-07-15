using UnityEngine;

public class PilotGestureProfile: GestureProfile
{
    public override void activate()
    {
        base.activate();
    }
    public override void deactivate()
    {
        base.deactivate();
    }

    public override void processTapGesture(Vector3 curMPWorld)
    {
        Managers.PlayerPilot.processTapGesture(curMPWorld);
    }
}
