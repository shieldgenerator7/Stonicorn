using UnityEngine;
using System.Collections;

public class GestureProfile
{

    protected PlayerController plrController;
    protected Rigidbody2D rb2dPlayer;
    private CameraController cameraController;
    protected CameraController Cam
    {
        get
        {
            if (cameraController == null)
            {
                cameraController = Camera.main.GetComponent<CameraController>();
            }
            return cameraController;
        }
    }
    protected GameManager gameManager;
    protected GestureManager gestureManager;

    public GestureProfile()
    {
        plrController = GameManager.Player;
        rb2dPlayer = plrController.GetComponent<Rigidbody2D>();
        gameManager = GameObject.FindObjectOfType<GameManager>();
        gestureManager = GameObject.FindObjectOfType<GestureManager>();
    }
    /// <summary>
    /// Called when this profile is set to the current one
    /// </summary>
    public virtual void activate() { }
    /// <summary>
    /// Called when the GestureManager switches off this profile to a different one
    /// </summary>
    public virtual void deactivate() { }

    public virtual void processTapGesture(GameObject go)
    {
        plrController.processTapGesture(go);
    }
    public virtual void processTapGesture(Vector3 curMPWorld)
    {
        if (GameManager.Rewinding)
        {
            gameManager.cancelRewind();
        }
        else
        {
            plrController.processTapGesture(curMPWorld);
        }
    }
    public virtual void processHoldGesture(Vector3 curMPWorld, float holdTime, bool finished)
    {
        plrController.processHoldGesture(curMPWorld, holdTime, finished);
    }
    public virtual void processDragGesture(Vector3 origMPWorld, Vector3 newMPWorld)
    {
        Cam.processDragGesture(origMPWorld, newMPWorld);
    }
    public virtual void processZoomLevelChange(float zoomLevel)
    {
        //GestureProfile switcher
        if (zoomLevel > Cam.scalePointToZoomLevel((int)CameraController.CameraScalePoints.TIMEREWIND - 1))
        {
            gestureManager.switchGestureProfile("Rewind");
        }
        if (zoomLevel < Cam.scalePointToZoomLevel(1))
        {
            gestureManager.switchGestureProfile("Menu");
        }
    }
}
