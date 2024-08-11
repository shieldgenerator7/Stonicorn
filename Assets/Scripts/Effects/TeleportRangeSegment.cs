using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Teleport Range Segment", menuName = "Teleport Range Segment")]
public class TeleportRangeSegment : ScriptableObject
{
    public int segmentIndex;
    public int segmentCount;
    public Color color = Color.white;

    public List<float> getAngles(float upgradeLevel)
    {
        int partCount = segmentCount;
        float angleSpacing = 360 / partCount;
        float angleMin = angleSpacing * segmentIndex;
        float angleMax = angleSpacing * (segmentIndex + 1);
        if (upgradeLevel > 0 && upgradeLevel <= 6)
        {
            float angleLevel = (upgradeLevel / 6.0f) * (angleMax - angleMin) + angleMin;
            return new List<float> { angleMin, angleLevel, angleMax };
        }
        return new List<float> { angleMin, angleMax };
    }

    public void processFragments(List<TeleportRangeFragment> fragments, Vector2 upVector)
    {
        int partCount = segmentCount;
        float angleSpacing = 360 / partCount;
        float angleMin = angleSpacing * segmentIndex;
        float angleMax = angleSpacing * (segmentIndex + 1);
        foreach (TeleportRangeFragment fragment in fragments)
        {
            if (Utility.between(
                Utility.RotationZ(upVector, fragment.transform.up),
                angleMin,
                angleMax
                )
                )
            {
                fragment.SpriteRenderer.color = color;
            }
        }
    }
}
