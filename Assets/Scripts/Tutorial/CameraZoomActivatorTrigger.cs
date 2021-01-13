using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraZoomActivatorTrigger : ActivatorTrigger
{
    public CameraZoomRange zoomRange;

    private bool zoomInRange = false;
    public override bool Triggered => zoomInRange;

    private void Start()
    {
        Managers.Camera.onZoomLevelChanged += OnCameraZoomLevelChanged;
        zoomInRange = scalePointInRange(Managers.Camera.ZoomLevel);
        triggeredChanged();
    }
    private void OnDestroy()
    {
        Managers.Camera.onZoomLevelChanged -= OnCameraZoomLevelChanged;
    }
    void OnCameraZoomLevelChanged(float newZoomLevel, float delta)
    {
        zoomInRange = scalePointInRange(newZoomLevel);
        triggeredChanged();
    }
    bool scalePointInRange(float zoomLevel)
    {
        float minZoom = (zoomRange.minZoomScalePoint < 0) 
            ? -1 
            : Managers.Camera.toZoomLevel(zoomRange.minZoomScalePoint);
        float maxZoom = (zoomRange.maxZoomScalePoint < 0) 
            ? -1 
            : Managers.Camera.toZoomLevel(zoomRange.maxZoomScalePoint);
        return (
                zoomRange.minZoomScalePoint < 0
                || (zoomRange.minZoomClusivity == CameraZoomRange.ClusivityOption.INCLUSIVE &&
                zoomLevel >= minZoom)
                || (zoomRange.minZoomClusivity == CameraZoomRange.ClusivityOption.EXCLUSIVE &&
                zoomLevel > minZoom)
            )
            && (
                zoomRange.maxZoomScalePoint < 0
                || (zoomRange.maxZoomClusivity == CameraZoomRange.ClusivityOption.INCLUSIVE &&
                zoomLevel <= maxZoom)
                || (zoomRange.maxZoomClusivity == CameraZoomRange.ClusivityOption.EXCLUSIVE &&
                zoomLevel < maxZoom)
            );
    }
}
