using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZoomOutTrigger : MemoryMonoBehaviour
{
    //Settings
    public CameraController.CameraScalePoints scalePoint = CameraController.CameraScalePoints.DEFAULT;
    public bool triggersOnce = true;//true if it only triggers once

    protected override void nowDiscovered()
    {
        CameraController camCtr = Managers.Camera;
        camCtr.ZoomLevel = camCtr.toZoomLevel(scalePoint);//zoom out
        if (scalePoint == CameraController.CameraScalePoints.TIMEREWIND)
        {
            Managers.Gesture.switchGestureProfile(GestureManager.GestureProfileType.REWIND);
        }
        if (triggersOnce)
        {
            Destroy(gameObject);
        }
    }

    protected override void previouslyDiscovered()
    {
        if (triggersOnce)
        {
            Destroy(gameObject);
        }
    }
}
