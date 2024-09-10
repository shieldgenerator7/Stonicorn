using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CactusBlossomController : MonoBehaviour
{
    [Tooltip("How far to the left to open to. 0 is pointing to its left")]
    public float openLeft = 0;
    [Tooltip("How far to the right to open to. 0 is pointing to its left")]
    public float openRight = 180;
    [Tooltip("How much to nudge the middle petals by")]
    public float middleOffset = 0;
    [Range(0f, 1f)]
    public float _openPercent = 1f;

    public List<Transform> petals;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        placePetals(_openPercent);
    }

    public void placePetals(float openPercent)
    {
        float openDiff = openRight - openLeft;
        float openHalf = openDiff / 2;

        float sign = Mathf.Sign(openDiff);
        float closePercent = 1 - openPercent;
        float left = openLeft + (openHalf * closePercent);
        float diff = openDiff - (Mathf.Abs(openLeft - left) * 2 * sign);

        for(int i = 0; i < petals.Count; i++)
        {
            Transform petal = petals[i];
            float percent = (float)i / (float)(petals.Count - 1);
            float offset = (i > 0 && i < petals.Count - 1) ? middleOffset * openPercent : 0;
            float angle = diff * percent + left + offset;
            petal.localEulerAngles = new Vector3(0,0,angle);
        }
    }
}
