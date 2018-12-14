﻿using System.Collections;
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
    [Tooltip("Is the player the only one that can activate the trigger?")]
    public bool forPlayerOnly = true;
    [Header("Camera Zoom Activation Settings")]
    public ActivationOptions zoomLevelEnterAction = ActivationOptions.DO_NOTHING;
    public ActivationOptions zoomLevelExitAction = ActivationOptions.DO_NOTHING;
    public ActivationOptions zoomLevelEnterStayAction = ActivationOptions.DO_NOTHING;
    public ActivationOptions zoomLevelExitStayAction = ActivationOptions.DO_NOTHING;
    /// <summary>
    /// Camera actions only active while something is in the trigger area
    /// </summary>
    [Tooltip("Does something have to be in the trigger area for the camera zoom listener to do its action?")]
    public bool zoomRequireTrigger = true;
    /// <summary>
    /// Trigger actions only active while the camera is at the right zoom level
    /// </summary>
    [Tooltip("Does the camera have to be at the right zoom level for the trigger area listener to do its action?")]
    public bool triggerRequireZoom = false;
    /// <summary>
    /// The minimum zoom scale point that defines the zoom level activation trigger
    /// Set it negative to have no maximum
    /// </summary>
    public CameraController.CameraScalePoints minZoomScalePoint = 0;
    /// <summary>
    /// The maximum zoom scale point that defines the zoom level activation trigger
    /// Set it negative to have no maximum
    /// </summary>
    [Tooltip("The maximum zoom scale point that defines the zoom level activation trigger.\n"
    + "Set it negative to have no maximum")]
    public CameraController.CameraScalePoints maxZoomScalePoint = CameraController.CameraScalePoints.TIMEREWIND;
    public enum ClusivityOption
    {
        INCLUSIVE,
        EXCLUSIVE
    }
    public ClusivityOption minZoomClusivity = ClusivityOption.INCLUSIVE;
    public ClusivityOption maxZoomClusivity = ClusivityOption.INCLUSIVE;

    [Header("Camera Position Activation Settings")]
    [Tooltip("The collider that checks for the presence of the camera.\nLeave it null to deactivate this feature.")]
    public Collider2D cameraPositionCollider;
    public ActivationOptions cameraEnterAction = ActivationOptions.ACTIVATE;
    public ActivationOptions cameraExitAction = ActivationOptions.DEACTIVATE;
    [Tooltip("Does something have to be in the trigger area for the camera position listener to do its action?")]
    public bool cameraPositionRequireTrigger = true;
    public GameObject cameraSnapAnchor;//the object the camera snaps to when it enters the trigger

    //
    // Runtime Vars
    //
    private bool triggerActive = false;
    private bool zoomLevelActive = false;
    private bool cameraPositionActive = false;

    //
    // Components
    //
    private CameraController camController;

    private void Start()
    {
        //Camera zoom trigger set up
        if (zoomLevelEnterAction != ActivationOptions.DO_NOTHING
            || zoomLevelExitAction != ActivationOptions.DO_NOTHING
            || triggerRequireZoom)
        {
            camController = FindObjectOfType<CameraController>();
            camController.onZoomLevelChanged += OnCameraZoomLevelChanged;
            zoomLevelActive = scalePointInRange(camController.ZoomLevel);
        }
        //Camera position trigger set up
        if (cameraPositionCollider != null)
        {
            if (camController == null)
            {
                camController = FindObjectOfType<CameraController>();
            }
            camController.onOffsetChange += OnCameraOffsetChanged;
            cameraPositionActive = cameraInArea();
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

    bool scalePointInRange(float zoomLevel)
    {
        float minZoom = (minZoomScalePoint < 0) ? -1 : camController.scalePointToZoomLevel((int)minZoomScalePoint);
        float maxZoom = (maxZoomScalePoint < 0) ? -1 : camController.scalePointToZoomLevel((int)maxZoomScalePoint);
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

    void OnCameraZoomLevelChanged(float newZoomLevel, float delta)
    {
        if (scalePointInRange(newZoomLevel))
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

    void OnCameraOffsetChanged()
    {
        if (!cameraPositionRequireTrigger || triggerActive)
        {
            //If camera is in the area,
            if (cameraInArea())
            {
                //but it wasn't before,
                if (!cameraPositionActive)
                {
                    //Process on area enter actions
                    processObjects(cameraEnterAction);
                }
                cameraPositionActive = true;
                if (cameraSnapAnchor != null)
                {
                    Vector3 newCamPos = cameraSnapAnchor.transform.position;
                    newCamPos.z = camController.transform.position.z;
                    camController.transform.position = newCamPos;
                }
            }
            else
            {
                if (cameraPositionActive)
                {
                    processObjects(cameraExitAction);
                }
                cameraPositionActive = false;
            }
        }
    }

    bool cameraInArea()
    {
        return cameraPositionCollider.OverlapPoint((Vector2)camController.transform.position);
    }
}
