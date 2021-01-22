using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuActionButtonPlay : MenuActionButton
{
    public override void activate()
    {
        if (Managers.Stats.get(Stat.MENU_BUTTON_PLAY) == 0)
        {
            //First time:
            //Zoom out to radius
            Managers.Camera.ZoomScalePoint = CameraController.CameraScalePoints.RANGE;
        }
        else
        {
            //Following time:
            //Zoom out to default
            Managers.Camera.ZoomScalePoint = CameraController.CameraScalePoints.DEFAULT;
        }
        Managers.Stats.addOne(Stat.MENU_BUTTON_PLAY);
    }
}
