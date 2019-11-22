using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportRangeSegment : MonoBehaviour
{
    public int segmentIndex;
    public int segmentCount;
    public Color color = Color.white;
    
    public void processFragments(List<GameObject> fragments, Vector2 upVector)
    {
        int partCount = segmentCount;
        float angleSpacing = 360 / partCount;
        float angleMin = angleSpacing * segmentIndex;
        float angleMax = angleSpacing * (segmentIndex + 1);
        foreach (GameObject fragment in fragments)
        {
            if (Utility.between(
                Utility.RotationZ(upVector, fragment.transform.up),
                angleMin,
                angleMax
                )
                )
            {
                fragment.GetComponent<SpriteRenderer>().color = color;
            }
        }
    }
}
