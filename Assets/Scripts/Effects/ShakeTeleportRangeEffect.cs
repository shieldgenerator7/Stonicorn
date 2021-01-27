using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShakeTeleportRangeEffect : TimedTeleportRangeEffect
{
    [Range(0, 60)]
    public float timeLeftShake = 10;//when this much time is left, it starts shaking
    [Range(0, 1)]
    public float maxShakeDistance = 0.2f;//how far it moves left and right when shaking

    public override void updateEffect()
    {
        //Check to see if it should shake
        float timeLeft = ttre.timer.TimeLeft;
        float range = updater.Range;
        if (timeLeft <= timeLeftShake)
        {
            float maxShake = maxShakeDistance * (timeLeftShake - timeLeft) / timeLeftShake;

            ttre.fragmentsBurned.ForEach(fragment =>
            {
                //Shake each fragment individually
                float randomRange = Random.Range(-maxShake, maxShake);
                fragment.transform.localPosition =
                    fragment.transform.localPosition.normalized
                    * (range + randomRange);
            });
            ttre.fragmentsFuse.ForEach(fragment =>
            {
                //Place fuse fragments at normal position
                fragment.transform.localPosition =
                    fragment.transform.localPosition.normalized * range;
            });
        }
        else
        {
            updater.fragments.ForEach(fragment =>
            {
                //Place all fragments at normal position
                fragment.transform.localPosition =
                    fragment.transform.localPosition.normalized * range;
            });
        }
    }
}
