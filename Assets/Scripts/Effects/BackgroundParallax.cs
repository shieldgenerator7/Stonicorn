using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundParallax : MonoBehaviour
{
    public Vector2 startPos = Vector2.zero;

    public Vector2 minBounds;//the position will stay inside these bounds
    public Vector2 maxBounds;

    public BoxCollider2D cameraContainerCollider;//the collider that has the initial camera containments
    private Bounds camBounds;

    private void Start()
    {
        if (startPos != (Vector2)transform.position)
        {
            if (startPos != Vector2.zero)
            {
                transform.position = startPos;
            }
            else
            {
                startPos = transform.position;
            }
        }
        camBounds = cameraContainerCollider.bounds;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        Vector2 adjustedPos = Vector2.zero;
        {
            float percentX = (Camera.main.transform.position.x - camBounds.min.x) / (camBounds.max.x - camBounds.min.x);
            adjustedPos.x = Mathf.Clamp(percentX, 0, 1)
                * (maxBounds.x - minBounds.x) + minBounds.x;
            adjustedPos.x = Mathf.Clamp(adjustedPos.x, minBounds.x, maxBounds.x);
        }
        {
            float percentY = (Camera.main.transform.position.y - camBounds.min.y) / (camBounds.max.y - camBounds.min.y);
            adjustedPos.y = Mathf.Clamp(percentY, 0, 1)
                * (maxBounds.y - minBounds.y) + minBounds.y;
            adjustedPos.y = Mathf.Clamp(adjustedPos.y, minBounds.y, maxBounds.y);
        }
        transform.position = adjustedPos; 
    }
}
