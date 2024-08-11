using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GestureInput
{
    public virtual bool InputSupported
        => true;

    public abstract InputDeviceMethod InputType { get; }

    public enum DragType
    {
        UNKNOWN,
        DRAG_PLAYER,
        DRAG_CAMERA
    }

    public abstract bool InputOngoing { get; }

    public abstract bool processInput(PlayGestureProfile profile);

    public static implicit operator bool(GestureInput gi)
        => gi != null;
}
