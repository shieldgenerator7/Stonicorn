using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundParallax : MonoBehaviour
{

    [Range(0, 1)]
    public float cameraFollowPercent = 0.25f;

    private Vector2 startPos;

    private void Start()
    {
        startPos = transform.position;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        transform.position = startPos + (((Vector2)Camera.main.transform.position - startPos) * cameraFollowPercent);
    }
}
