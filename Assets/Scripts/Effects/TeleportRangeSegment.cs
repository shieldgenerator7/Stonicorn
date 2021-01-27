using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Teleport Range Segment", menuName = "Teleport Range Segment")]
public class TeleportRangeSegment : ScriptableObject
{
    public int segmentIndex;
    public int segmentCount;
    public Color color = Color.white;

    public List<float> getAngles(int upgradeLevel)
    {
        int partCount = segmentCount;
        float angleSpacing = 360 / partCount;
        float angleMin = angleSpacing * segmentIndex;
        float angleMax = angleSpacing * (segmentIndex + 1);
        if (upgradeLevel > 0 && upgradeLevel <= 6)
        {
            float angleLevel = (upgradeLevel / 6) * (angleMax - angleMin) + angleMin;
            return new List<float> { angleMin, upgradeLevel, angleMax };
        }
        return new List<float> { angleMin, angleMax };
    }

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
