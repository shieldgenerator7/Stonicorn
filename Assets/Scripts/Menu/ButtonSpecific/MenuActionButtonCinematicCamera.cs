using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuActionButtonCinematicCamera : MenuActionSwitch
{
    public override void doAction(bool active)
    {
        FindAnyObjectByType<CinematicCameraController>().Active = active;
    }

    public override bool getActiveState()
    {
        return FindAnyObjectByType<CinematicCameraController>().Active;
    }
}
