using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    public ActivationOptions cameraZoomInAction = ActivationOptions.DO_NOTHING;
    public ActivationOptions cameraZoomOutAction = ActivationOptions.DO_NOTHING;
    /// <summary>
    /// Camera actions only active while something is in the trigger area
    /// </summary>
    public bool cameraActionsRequireTrigger = true;

    //
    // Runtime Vars
    //
    private bool triggerActive = false;

    private void Start()
    {
        CameraController cc = FindObjectOfType<CameraController>();
        cc.onZoomLevelChanged += OnCameraZoomLevelChanged;
    }

    private void OnTriggerEnter2D(Collider2D coll)
    {
        if (!forPlayerOnly || coll.gameObject.CompareTag("Player"))
        {
            processObjects(triggerEnterAction);
            triggerActive = true;
        }
    }
    private void OnTriggerExit2D(Collider2D coll)
    {
        if (!forPlayerOnly || coll.gameObject.CompareTag("Player"))
        {
            processObjects(triggerExitAction);
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

    void OnCameraZoomLevelChanged(int newScalePoint, int delta)
    {
        if (!cameraActionsRequireTrigger || triggerActive)
        {
            if (delta < 0)
            {
                processObjects(cameraZoomInAction);
            }
            else if (delta > 0)
            {
                processObjects(cameraZoomOutAction);
            }
        }
    }
}
