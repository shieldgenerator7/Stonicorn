using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraPositionLimiter : MonoBehaviour
{
    [Tooltip("True to snap only when in range, false to force camera to be at the position")]
    public bool snapCameraPosition = true;
    public float snapRange = 2;

    private Vector3 newCamPos;

    private void OnEnable()
    {
        newCamPos = transform.position;
        newCamPos.z = Camera.main.transform.position.z;
        limitPosition();
        Managers.Camera.onOffsetChange += offsetChanged;
    }
    private void OnDisable()
    {
        Managers.Camera.onOffsetChange -= offsetChanged;
    }

    void offsetChanged(Vector3 offset)
    {
        if (Managers.Camera.transform.position != newCamPos)
        {
            if (!snapCameraPosition || Vector2.Distance(Managers.Camera.transform.position, newCamPos) <= snapRange)
            {
            limitPosition();
            }
        }
    }

    void limitPosition()
    {
        Managers.Camera.transform.position = newCamPos;
        Managers.Camera.pinPoint();
    }
}
