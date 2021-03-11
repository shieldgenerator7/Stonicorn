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
        zoomInRange = zoomRange.scalePointInRange(Managers.Camera.ZoomLevel);
        triggeredChanged();
    }
    private void OnDestroy()
    {
        Managers.Camera.onZoomLevelChanged -= OnCameraZoomLevelChanged;
    }
    void OnCameraZoomLevelChanged(float newZoomLevel, float delta)
    {
        zoomInRange = zoomRange.scalePointInRange(newZoomLevel);
        triggeredChanged();
    }    
}
