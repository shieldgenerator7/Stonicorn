using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraPositionActivatorTrigger : ActivatorTrigger
{
    public enum BoxSide
    {
        INSIDE,
        OUTSIDE
    }
    [Tooltip("Should it be active when the camera is inside or outside the box?")]
    public BoxSide activeBoxSide = BoxSide.OUTSIDE;
    [Tooltip("The collider that checks for the presence of the camera")]
    public Collider2D cameraPositionCollider;
    [Tooltip("The object the camera snaps to when it enters the trigger")]
    public GameObject cameraSnapAnchor;

    private bool triggered = false;
    public override bool Triggered => triggered;

    private void Start()
    {
        if (cameraPositionCollider != null)
        {
            Managers.Camera.onOffsetChange += OnCameraOffsetChanged;
            OnCameraOffsetChanged(Managers.Camera.Offset);
        }
    }
    private void OnDestroy()
    {
        Managers.Camera.onOffsetChange -= OnCameraOffsetChanged;
    }

    void OnCameraOffsetChanged(Vector3 offset)
    {
        bool inArea = cameraInArea();
        switch (activeBoxSide)
        {
            case BoxSide.INSIDE:
                triggered = inArea;
                break;
            case BoxSide.OUTSIDE:
                triggered = !inArea;
                break;
            default: throw new ArgumentException("Invalid BoxSide: " + activeBoxSide);
        }
        triggeredChanged();
        //2021-01-07: TODO: refactor
        //if (cameraSnapAnchor != null)
        //{
        //    Vector3 newCamPos = cameraSnapAnchor.transform.position;
        //    newCamPos.z = Camera.main.transform.position.z;
        //    Camera.main.transform.position = newCamPos;
        //}

    }

    bool cameraInArea()
    {
        if (cameraPositionCollider == null)
        {
            return false;
        }
        return cameraPositionCollider.OverlapPoint(Camera.main.transform.position);
    }

}
