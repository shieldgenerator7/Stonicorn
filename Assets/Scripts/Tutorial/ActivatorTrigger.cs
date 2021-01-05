using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ActivatorTrigger : MonoBehaviour
{
    public abstract bool Triggered { get; }

    public delegate void OnTriggeredChanged(bool triggered);
    public event OnTriggeredChanged onTriggeredChanged;

    protected void triggeredChanged()
    {
        onTriggeredChanged?.Invoke(Triggered);
    }
}
