using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpacityTeleportRangeEffect : TeleportRangeEffect
{
    [Range(0, 1)]
    public float transparency = 1.0f;
    [Range(0, 1)]
    public float timeTransparency = 0.5f;

    public override void updateEffect(List<GameObject> fragments, float timeLeft, float duration)
    {
        Vector2 upVector = transform.up;
        float angleMin = 0;
        float angleMax = 360 * timeLeft / duration;
        foreach (GameObject fragment in fragments)
        {
            //Set the alpha to standard
            SpriteRenderer sr = fragment.GetComponent<SpriteRenderer>();
            float newAlpha = transparency;
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
                    newAlpha = timeTransparency;
                }
            }
            //Put the color back in the fragment
            sr.color = sr.color.adjustAlpha(newAlpha);
        }
    }
}
