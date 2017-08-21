using UnityEngine;
using System.Collections;

public class GestureProfile {

    protected GameObject player;
    protected PlayerController plrController;
    protected Rigidbody2D rb2dPlayer;
    protected Camera cam;
    protected CameraController cmaController;
    protected GameManager gm;
    protected GestureManager gestureManager;

    public GestureProfile()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        plrController = player.GetComponent<PlayerController>();
        rb2dPlayer = player.GetComponent<Rigidbody2D>();
        cam = Camera.main;
        cmaController = cam.GetComponent<CameraController>();
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
        if (gm.isRewinding())
        {
            gm.cancelRewind();
        }
        else {
            GestureAccepter ga = Utility.findGestureAccepter(curMPWorld);
            if (ga != null && ga.acceptsTapGesture())
            {
                ga.processTapGesture();
            }
            else {
                plrController.processTapGesture(curMPWorld);
            }
        }
    }
    public virtual void processHoldGesture(Vector3 curMPWorld, float holdTime, bool finished)
    {
        GestureAccepter ga = Utility.findGestureAccepter(curMPWorld);
        if (ga != null && ga.acceptsHoldGesture())
        {
            ga.processHoldGesture(finished);
            plrController.dropHoldGesture();
        }
        else {
            plrController.processHoldGesture(curMPWorld, holdTime, finished);
        }
    }
    public void processDragGesture()
    {

    }
    public virtual void processPinchGesture(int adjustment)
    {
        cmaController.adjustScalePoint(adjustment);
        //GestureProfile switcher
        if (cmaController.getScalePointIndex() == CameraController.SCALEPOINT_TIMEREWIND)
        {
            gestureManager.switchGestureProfile("Rewind");
        }
    }
}
