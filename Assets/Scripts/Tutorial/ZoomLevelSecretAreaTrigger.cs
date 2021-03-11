using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZoomLevelSecretAreaTrigger : SecretAreaTrigger
{
    [Tooltip("When the camera zoom level goes outside this range," +
        " the hidden area is revealed.")]
    public CameraZoomRange hiddenRange;

    private bool colliderReq = false;
    private bool cameraReq = false;
    private void Start()
    {
        Managers.Camera.onZoomLevelChanged += OnZoomLevelChanged;
    }
    private void OnDestroy()
    {
        Managers.Camera.onZoomLevelChanged -= OnZoomLevelChanged;
    }

    protected override void OnTriggerEnter2D(Collider2D coll)
    {
        if (coll.isPlayerSolid())
        {
            colliderReq = true;
            checkReveal();
        }
    }
    void OnTriggerExit2D(Collider2D coll)
    {
        if (coll.isPlayerSolid())
        {
            colliderReq = false;
        }
    }

    void OnZoomLevelChanged(float zoomLevel, float delta)
    {
        if (!hiddenRange.scalePointInRange(zoomLevel))
        {
            cameraReq = true;
            checkReveal();
        }
        else
        {
            cameraReq = false;
        }
    }

    void checkReveal()
    {
        if (colliderReq && cameraReq)
        {
            GetComponentInParent<HiddenArea>()
                .Discovered = true;
        }
    }
}
