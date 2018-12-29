using UnityEngine;
using System.Collections;

public class GestureProfile
{

    protected GameObject player;
    protected PlayerController plrController;
    protected Rigidbody2D rb2dPlayer;
    protected Camera cam;
    protected CameraController camController;
    protected GameManager gm;
    protected GestureManager gestureManager;

    public GestureProfile()
    {
        player = GameObject.FindGameObjectWithTag(GameManager.playerTag);
        plrController = player.GetComponent<PlayerController>();
        rb2dPlayer = player.GetComponent<Rigidbody2D>();
        cam = Camera.main;
        camController = cam.GetComponent<CameraController>();
        gm = GameObject.FindObjectOfType<GameManager>();
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
        if (GameManager.isRewinding())
        {
            gm.cancelRewind();
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
        camController.processDragGesture(origMPWorld, newMPWorld);
    }
    public virtual void processZoomLevelChange(float zoomLevel)
    {
        //GestureProfile switcher
        if (zoomLevel > camController.scalePointToZoomLevel((int)CameraController.CameraScalePoints.TIMEREWIND - 1))
        {
            gestureManager.switchGestureProfile("Rewind");
        }
        if (zoomLevel < camController.scalePointToZoomLevel(1))
        {
            gestureManager.switchGestureProfile("Menu");
        }
    }
}
