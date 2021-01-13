using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraZoomLimiter : MonoBehaviour
{
    public CameraZoomRange zoomRange;

    public float exclusiveBuffer = 0.01f;

    private float minZoomLevel = 0;
    private float maxZoomLevel = 0;

    private void OnEnable()
    {
        //Set processing variables
        minZoomLevel = Managers.Camera.toZoomLevel(zoomRange.minZoomScalePoint);
        if (zoomRange.minZoomClusivity == CameraZoomRange.ClusivityOption.EXCLUSIVE)
        {
            minZoomLevel += exclusiveBuffer;
        }
        maxZoomLevel = Managers.Camera.toZoomLevel(zoomRange.maxZoomScalePoint);
        if (zoomRange.maxZoomClusivity == CameraZoomRange.ClusivityOption.EXCLUSIVE)
        {
            maxZoomLevel -= exclusiveBuffer;
        }
        //Limit zoom
        Managers.Camera.onZoomLevelChanged += limitZoom;
        limitZoom(Managers.Camera.ZoomLevel, 0);
    }


    private void OnDisable()
    {
        Managers.Camera.onZoomLevelChanged -= limitZoom;
    }

    void limitZoom(float zoomLevel, float delta)
    {
        if (zoomLevel < minZoomLevel)
        {
            Managers.Camera.ZoomLevel = minZoomLevel;
        }
        if (zoomLevel > maxZoomLevel)
        {
            Managers.Camera.ZoomLevel = maxZoomLevel;
        }
    }
}
