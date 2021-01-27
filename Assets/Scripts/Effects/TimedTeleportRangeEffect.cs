using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TimedTeleportRangeEffect : TeleportRangeEffect
{
    protected TimerTeleportRangeEffect ttre;
    public void init(TeleportRangeUpdater updater, TimerTeleportRangeEffect ttre)
    {
        init(updater);
        this.ttre = ttre;
    }
}
