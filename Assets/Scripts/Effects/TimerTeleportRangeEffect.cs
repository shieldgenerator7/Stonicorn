using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimerTeleportRangeEffect : TeleportRangeEffect
{
    public Timer timer;

    internal readonly List<GameObject> fragmentsBurned = new List<GameObject>();//the fragments that represent time that is used up
    internal readonly List<GameObject> fragmentsFuse = new List<GameObject>();//the fragments that represent time that has not been used up

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
        Vector2 upVector = transform.up;
        float angleMin = 0;
        float angleMax = 360 * timer.TimeLeft / timer.Duration;
        foreach (GameObject fragment in updater.fragments)
        {
            //Check to see if it's in the timer range
            if (Utility.between(
                Utility.RotationZ(upVector, fragment.transform.up),
                angleMin,
                angleMax
                )
                )
            {
                fragmentsFuse.Add(fragment);
            }
            else
            {
                fragmentsBurned.Add(fragment);
            }
        }
        effects.ForEach(fx => fx.updateEffect());
    }
}
