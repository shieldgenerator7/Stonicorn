using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShakeTeleportRangeEffect : TeleportRangeEffect
{
    [Range(0, 60)]
    public float timeLeftShake = 10;//when this much time is left, it starts shaking
    [Range(0, 1)]
    public float maxShakeDistance = 0.2f;//how far it moves left and right when shaking

    public override void updateEffect(List<GameObject> fragments, float timeLeft, float duration)
    {
        //Check to see if it should shake
        if (timeLeft <= timeLeftShake)
        {
            float range = 3;
            Vector2 upVector = transform.up;
            float angleMin = 0;
            float angleMax = 360 * timeLeft / duration;
            float maxShake = maxShakeDistance * (timeLeftShake - timeLeft) / timeLeftShake;
            foreach (GameObject fragment in fragments)
            {
                //Check to see if it's in the timer range
                if (Utility.between(
                    Utility.RotationZ(upVector, fragment.transform.up),
                    angleMin,
                    angleMax
                    )
                    )
                {
                }
                else
                {
                    //Shake each fragment individually
                    float randomRange = Random.Range(-maxShake, maxShake);
                    fragment.transform.localPosition = fragment.transform.localPosition.normalized * (range + randomRange);
                }
            }
        }
    }
}
