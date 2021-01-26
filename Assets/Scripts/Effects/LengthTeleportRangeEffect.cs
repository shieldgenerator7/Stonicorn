using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LengthTeleportRangeEffect : TeleportRangeEffect
{
    [Range(0, 1)]
    public float normalLength = 0.5f;
    [Range(0, 1)]
    public float timeLength = 0.8f;

    public override void updateEffect(List<GameObject> fragments, float timeLeft, float duration)
    {
        Vector2 upVector = transform.up;
        float angleMin = 0;
        float angleMax = 360 * timeLeft / duration;
        foreach (GameObject fragment in fragments)
        {
            //Set the length to standard
            Vector3 scale = fragment.transform.localScale;
            scale.y = normalLength;
            if (timeLeft > 0)
            {
                //Check to see if it's in the timer range
                if (Utility.between(
                    Utility.RotationZ(upVector, fragment.transform.up),
                    angleMin,
                    angleMax
                    )
                    )
                {
                    //If so, set the length to the time length
                    scale.y = timeLength;
                }
            }
            //Put the size back in the fragment
            fragment.transform.localScale = scale;
        }
    }
}
