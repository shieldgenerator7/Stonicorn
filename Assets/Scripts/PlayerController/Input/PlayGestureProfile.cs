using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayGestureProfile : GestureProfile
{
    protected override void processTapGesture(Gesture gesture)
    {
        if (Managers.Rewind.Rewinding)
        {
            if (Managers.Rewind.rewindInterruptableByPlayer)
            {
                Managers.Rewind.cancelRewind();
            }
        }
        else
        {
            Managers.Player.processGesture(gesture);
        }
    }

    protected override void processHoldGesture(Gesture gesture)
    {
        Managers.Player.processGesture(gesture);
    }

    protected override void processDragGesture(Gesture gesture)
    {
        //If the player drags on Merky,
        if (gesture.dragType == GestureDragType.PLAYER)
        {
            //Activate the ForceLaunch ability
            Managers.Player.processGesture(gesture);
        }
        else if (gesture.dragType == GestureDragType.CAMERA)
        {
            //Drag the camera
            Managers.Camera.processDragGesture(gesture.startPosition, gesture.position, gesture.state);
        }
        else
        {
            throw new System.ArgumentException("DragType must be a valid value! dragType: " + gesture.dragType);
        }
    }
}
