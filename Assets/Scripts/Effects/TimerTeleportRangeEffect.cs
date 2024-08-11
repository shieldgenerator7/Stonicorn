using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimerTeleportRangeEffect : TeleportRangeEffect
{
    public Timer timer;

    internal readonly List<TeleportRangeFragment> fragmentsBurned = new List<TeleportRangeFragment>();//the fragments that represent time that is used up
    internal readonly List<TeleportRangeFragment> fragmentsFuse = new List<TeleportRangeFragment>();//the fragments that represent time that has not been used up

    public List<TimedTeleportRangeEffect> effects;

    public override void init(TeleportRangeUpdater tru)
    {
        base.init(tru);
        effects.ForEach(fx => fx.init(tru, this));
        //Timer
        timer.onTimeLeftChanged += timerTick;
        timerTick(timer.TimeLeft, timer.Duration);
    }

    public void timerTick(float timeLeft, float duration)
    {
        updateEffect();
    }

    public override void updateEffect()
    {
        fragmentsBurned.Clear();
        fragmentsFuse.Clear();
        float angleMax = 360 * timer.TimeLeft / timer.Duration;
        List<List<TeleportRangeFragment>> fragmentGroups = updater.getFragmentGroups(angleMax);
        fragmentsBurned.AddRange(fragmentGroups[0]);
        fragmentsFuse.AddRange(fragmentGroups[1]);
        effects.ForEach(fx => fx.updateEffect());
    }
}
