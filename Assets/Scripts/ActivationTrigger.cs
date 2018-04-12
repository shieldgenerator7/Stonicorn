using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ActivationTrigger : MonoBehaviour
{
    public enum ActivationOptions
    {
        ACTIVATE,
        DEACTIVATE,
        SWITCH,
        DO_NOTHING
    }

    //
    //Settings
    //
    [Header("Trigger Activation Settings")]
    public ActivationOptions triggerEnterAction = ActivationOptions.ACTIVATE;
    public ActivationOptions triggerExitAction = ActivationOptions.DEACTIVATE;

    public List<GameObject> objectsToActivate;

    /// <summary>
    /// Trigger can only be activated by player
    /// </summary>
    public bool forPlayerOnly = true;
    [Header("Camera Zoom Activation Settings")]
    public ActivationOptions zoomLevelEnterAction = ActivationOptions.DO_NOTHING;
    public ActivationOptions zoomLevelExitAction = ActivationOptions.DO_NOTHING;
    public ActivationOptions zoomLevelEnterStayAction = ActivationOptions.DO_NOTHING;
    public ActivationOptions zoomLevelExitStayAction = ActivationOptions.DO_NOTHING;
    /// <summary>
    /// Camera actions only active while something is in the trigger area
    /// </summary>
    public bool zoomRequireTrigger = true;
    /// <summary>
    /// Trigger actions only active while the camera is at the right zoom level
    /// </summary>
    public bool triggerRequireZoom = false;
    /// <summary>
    /// The minimum zoom scale point that defines the zoom level activation trigger
    /// Set it negative to have no maximum
    /// </summary>
    public int minZoomScalePoint = 0;
    /// <summary>
    /// The maximum zoom scale point that defines the zoom level activation trigger
    /// Set it negative to have no maximum
    /// </summary>
    public int maxZoomScalePoint = 3;

    //
    // Runtime Vars
    //
    private bool triggerActive = false;
    private bool zoomLevelActive = false;

    private void Start()
    {
        //Camera zoom trigger set up
        if (zoomLevelEnterAction != ActivationOptions.DO_NOTHING
            || zoomLevelExitAction != ActivationOptions.DO_NOTHING
            || triggerRequireZoom)
        {
            CameraController cc = FindObjectOfType<CameraController>();
            cc.onZoomLevelChanged += OnCameraZoomLevelChanged;
            zoomLevelActive = scalePointInRange(cc.getScalePointIndex());
        }
        //Error checking
        if (Application.isEditor)
        {
            if (objectsToActivate == null || objectsToActivate.Count <= 0)
            {
                throw new UnityException("Activation Trigger (" + gameObject.name + ") does not have any objects to activate.");
            }
            Collider2D coll2d = GetComponent<Collider2D>();
            if (!coll2d.isTrigger)
            {
                throw new UnityException("Activation Trigger (" + gameObject.name + ") needs its Collider2D to be a trigger! (set 'Is Trigger' to true)");
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D coll)
    {
        if (!forPlayerOnly || coll.gameObject.CompareTag("Player"))
        {
            if (!triggerRequireZoom || zoomLevelActive)
            {
                processObjects(triggerEnterAction);
            }
            triggerActive = true;
        }
    }
    private void OnTriggerExit2D(Collider2D coll)
    {
        if (!forPlayerOnly || coll.gameObject.CompareTag("Player"))
        {
            if (!triggerRequireZoom || zoomLevelActive)
            {
                processObjects(triggerExitAction);
            }
            triggerActive = false;
        }
    }
    private void processObjects(ActivationOptions action)
    {
        foreach (GameObject go in objectsToActivate)
        {
            switch (action)
            {
                case ActivationOptions.ACTIVATE:
                    go.SetActive(true);
                    break;
                case ActivationOptions.DEACTIVATE:
                    go.SetActive(false);
                    break;
                case ActivationOptions.SWITCH:
                    go.SetActive(!go.activeSelf);
                    break;
                case ActivationOptions.DO_NOTHING:
                    break;
            }
        }
    }

    bool scalePointInRange(int scalePoint)
    {
        return (minZoomScalePoint < 0 || scalePoint >= minZoomScalePoint)
            && (maxZoomScalePoint < 0 || scalePoint <= maxZoomScalePoint);
    }

    void OnCameraZoomLevelChanged(int newScalePoint, int delta)
    {
        if (scalePointInRange(newScalePoint))
        {
            if (!zoomRequireTrigger || triggerActive)
            {
                if (!zoomLevelActive)
                {
                    processObjects(zoomLevelEnterAction);
                }
                else
                {
                    processObjects(zoomLevelEnterStayAction);
                }
            }
            zoomLevelActive = true;
        }
        else
        {
            if (!zoomRequireTrigger || triggerActive)
            {
                if (zoomLevelActive)
                {
                    processObjects(zoomLevelExitAction);
                }
                else
                {
                    processObjects(zoomLevelExitStayAction);
                }
            }
            zoomLevelActive = false;
        }
    }
}
