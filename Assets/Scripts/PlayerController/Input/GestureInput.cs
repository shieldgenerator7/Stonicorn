using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GestureInput
{
    public virtual bool InputSupported
    {
        get => true;
    }

    public abstract InputDeviceMethod InputType
    {
        get;
    }

    public abstract bool InputOngoing { get; }

    public abstract bool processInput(GestureProfile profile);

    public static implicit operator bool (GestureInput gi) => gi != null;
}
