using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TeleportRangeEffect : MonoBehaviour
{
    protected TeleportRangeUpdater updater;
    public virtual void init(TeleportRangeUpdater updater)
    {
        this.updater = updater;
    }
    public abstract void updateEffect();
}
