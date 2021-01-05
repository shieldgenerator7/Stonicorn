using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraZoomActivatorTrigger : ActivatorTrigger
{
    /// <summary>
    /// The minimum zoom scale point that defines the zoom level activation trigger
    /// </summary>
    public CameraController.CameraScalePoints minZoomScalePoint = 0;
    /// <summary>
    /// The maximum zoom scale point that defines the zoom level activation trigger
    /// </summary>
    public CameraController.CameraScalePoints maxZoomScalePoint = CameraController.CameraScalePoints.TIMEREWIND;
    public enum ClusivityOption
    {
        INCLUSIVE,
        EXCLUSIVE
    }
    public ClusivityOption minZoomClusivity = ClusivityOption.INCLUSIVE;
    public ClusivityOption maxZoomClusivity = ClusivityOption.INCLUSIVE;

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
        float minZoom = (minZoomScalePoint < 0) ? -1 : Managers.Camera.toZoomLevel(minZoomScalePoint);
        float maxZoom = (maxZoomScalePoint < 0) ? -1 : Managers.Camera.toZoomLevel(maxZoomScalePoint);
        return (
                minZoomScalePoint < 0
                || (minZoomClusivity == ClusivityOption.INCLUSIVE &&
                zoomLevel >= minZoom)
                || (minZoomClusivity == ClusivityOption.EXCLUSIVE &&
                zoomLevel > minZoom)
            )
            && (
                maxZoomScalePoint < 0
                || (maxZoomClusivity == ClusivityOption.INCLUSIVE &&
                zoomLevel <= maxZoom)
                || (maxZoomClusivity == ClusivityOption.EXCLUSIVE &&
                zoomLevel < maxZoom)
            );
    }
}
