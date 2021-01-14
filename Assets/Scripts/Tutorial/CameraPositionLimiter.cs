using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraPositionLimiter : MonoBehaviour
{
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
            limitPosition();
        }
    }

    void limitPosition()
    {
        Managers.Camera.transform.position = newCamPos;
        Managers.Camera.pinPoint();
    }
}
