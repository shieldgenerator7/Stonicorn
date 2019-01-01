using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraController : MonoBehaviour
{
    public GameObject player;
    public float zoomSpeed = 0.5f;//how long it takes to fully change to a new zoom level
    public GameObject planModeCanvas;//the canvas that has the UI for plan mode
    public float cameraOffsetGestureThreshold = 2.0f;//how far off the center of the screen Merky must be for the hold gesture to behave differently
    public float screenEdgeThreshold = 0.9f;//the percentage of half the screen that is in the middle, the rest is the edge
    public float autoOffsetScreenEdgeThreshold = 0.7f;//same as screenEdgeThreshold, but used for the purposes of autoOffset
    public float cameraMoveFactor = 1.5f;
    public float autoOffsetDuration = 1;//how long autoOffset lasts after the latest teleport
    public float autoOffsetAngleThreshold = 15f;//how close two teleport directions have to be to activate auto offset
    public float maxTapDelay = 1;//the maximum amount of time (sec) between two taps that can activate auto offset


    private Vector3 offset;
    public Vector3 Offset
    {
        get { return offset; }
        set
        {
            if (value.z == 0)
            {
                value.z = offset.z;
            }
            offset = value;
            if (onOffsetChange != null)
            {
                onOffsetChange();
            }
        }
    }
    /// <summary>
    /// Offset the camera automatically adds itself to make sure the player can see where they're going
    /// </summary>
    private Vector2 autoOffset;
    private Vector2 previousMoveDir;//the direction of the last teleport, used to determine if autoOffset should activate
    private float autoOffsetCancelTime = 0;//the time at which autoOffset will be removed automatically (updated after each teleport)
    private float lastTapTime = 0;//the last time a teleport was processed
    /// <summary>
    /// How far away the camera is from where it wants to be
    /// </summary>
    public Vector2 Displacement
    {
        get { return transform.position - player.transform.position + offset; }
        private set { }
    }
    private Vector3 rotationUp;//the up direction that the camera should be rotated towards
    private float scale = 1;//scale used to determine orthographicSize, independent of (landscape or portrait) orientation
    private float desiredScale = 0;//the value that scale should move towards
    private Camera cam;
    private Rigidbody2D playerRB2D;
    private GestureManager gm;
    private PlayerController plyrController;

    private bool lockCamera = false;//keep the camera from moving
    [Tooltip("Runtime Var, Doesn't do anything from editor")]
    public Vector3 originalCameraPosition;//"original camera position": the camera offset (relative to the player) at the last mouse down (or tap down) event

    private int prevScreenWidth;
    private int prevScreenHeight;

    public float ZoomLevel
    {
        get { return scale; }
        set
        {
            float prevScale = scale;
            scale = value;
            if (prevScale != scale)
            {
                scale = Mathf.Clamp(
                    scale,
                    scalePoints[0].absoluteScalePoint(),
                    scalePoints[scalePoints.Count - 1].absoluteScalePoint());
                if (onZoomLevelChanged != null)
                {
                    onZoomLevelChanged(scale, scale - prevScale);
                }
                updateOrthographicSize();
            }
        }
    }
    /// <summary>
    /// Set this to make the scale smoothly move to the new value
    /// </summary>
    public float TargetZoomLevel
    {
        get { return desiredScale; }
        set
        {
            desiredScale = value;
            preTargetZoomLevel = ZoomLevel;
            if (desiredScale == 0)
            {
                preTargetZoomLevel = 0;
            }
        }
    }
    private float preTargetZoomLevel;//used to determine if the targetZoomLevel is above or below the current one

    struct ScalePoint
    {
        private float scalePoint;
        private bool relative;//true if relative to player's range, false if absolute
        private PlayerController plyrController;
        public ScalePoint(float scale, bool relative, PlayerController plyrController)
        {
            scalePoint = scale;
            this.relative = relative;
            this.plyrController = plyrController;
        }
        public float absoluteScalePoint()
        {
            if (relative)
            {
                return scalePoint * plyrController.baseRange;
            }
            return scalePoint;
        }
    }
    List<ScalePoint> scalePoints = new List<ScalePoint>();
    public enum CameraScalePoints
    {
        NONE = -1,//invalid index, used for ActivationTrigger
        MENU = 0,//the index of the main menu
        PORTRAIT = 1,//shows Merky's body close up
        RANGE = 2,//camera size is as large as Merky's teleport range
        DEFAULT = 3,//the index of the default scalepoint
        TIMEREWIND = 4//the index of the time rewind mechanic
    }

    // Use this for initialization
    void Start()
    {
        pinPoint();
        cam = GetComponent<Camera>();
        playerRB2D = player.GetComponent<Rigidbody2D>();
        gm = GameObject.FindGameObjectWithTag("GestureManager").GetComponent<GestureManager>();
        plyrController = player.GetComponent<PlayerController>();
        plyrController.onTeleport += checkForAutoMovement;
        if (planModeCanvas.GetComponent<Canvas>() == null)
        {
            Debug.LogError("Camera " + gameObject.name + "'s planModeCanvas object (" + planModeCanvas.name + ") doesn't have a Canvas component!");
        }
        scale = cam.orthographicSize;
        rotationUp = transform.up;
        //Initialize ScalePoints
        scalePoints.Add(new ScalePoint(0.2f, false, plyrController));//Main Menu zoom level
        scalePoints.Add(new ScalePoint(1, false, plyrController));
        scalePoints.Add(new ScalePoint(1, true, plyrController));
        scalePoints.Add(new ScalePoint(2, true, plyrController));
        scalePoints.Add(new ScalePoint(4, true, plyrController));
        //Set the initialize scale point
        scale = scalePoints[0].absoluteScalePoint();
        //Clean Delegates set up
        SceneManager.sceneUnloaded += cleanDelegates;
    }

    void Update()
    {
        if (prevScreenHeight != Screen.height || prevScreenWidth != Screen.width)
        {
            prevScreenWidth = Screen.width;
            prevScreenHeight = Screen.height;
            updateOrthographicSize();
        }
    }

    // Update is called once per frame, after all other objects have moved that frame
    void LateUpdate()
    {
        if (!gm.cameraDragInProgress)
        {
            if (!lockCamera)
            {
                //Target
                Vector3 target = player.transform.position + offset + (Vector3)autoOffset;
                //Speed
                float speed = (
                        Vector3.Distance(transform.position, target)
                        * cameraMoveFactor
                        + playerRB2D.velocity.magnitude
                    )
                    * Time.deltaTime;
                //Move Transform
                transform.position = Vector3.MoveTowards(transform.position, target, speed);

                if (autoOffsetCancelTime > 0)
                {
                    if (Time.time > autoOffsetCancelTime)
                    {
                        autoOffset = Vector2.zero;
                        previousMoveDir = Vector2.zero;
                        autoOffsetCancelTime = 0;
                    }
                }
            }
            else
            {
                if (!inView(player.transform.position))
                {
                    recenter();
                }
            }

            //Rotate Transform
            if (!rotationFinished())
            {
                float deltaTime = 3 * Time.deltaTime;
                float angle = Vector3.Angle(transform.up, rotationUp) * deltaTime;
                transform.up = Vector3.Lerp(transform.up, rotationUp, deltaTime);
                Offset = Quaternion.AngleAxis(angle, Vector3.forward) * offset;
            }

            //Scale Orthographic Size
            if (TargetZoomLevel > 0)
            {
                //If current zoom is not target zoom,
                if (ZoomLevel != TargetZoomLevel
                    //and current zoom is between starting zoom and target zoom,
                    && Mathf.Clamp(ZoomLevel, preTargetZoomLevel, TargetZoomLevel) == ZoomLevel)
                {
                    //Move current zoom closer to target zoom
                    ZoomLevel = Mathf.MoveTowards(ZoomLevel, TargetZoomLevel, Time.deltaTime);
                    //Close in the zoom area where autozooming will continue
                    preTargetZoomLevel = ZoomLevel;
                }
                else
                {
                    TargetZoomLevel = 0;
                }
            }
        }
    }

    public bool rotationFinished()
    {
        return transform.up == rotationUp;
    }

    /// <summary>
    /// If Merky is on the edge of the screen, discard movement delay
    /// </summary>
    /// <param name="oldPos">Where merky just was</param>
    /// <param name="newPos">Where merky is now</param>
    public void checkForAutoMovement(Vector2 oldPos, Vector2 newPos)
    {
        //If the player is near the edge of the screen upon teleporting, recenter the screen
        Vector2 screenPos = cam.WorldToScreenPoint(newPos);
        Vector2 oldScreenPos = cam.WorldToScreenPoint(oldPos);
        Vector2 centerScreen = new Vector2(Screen.width, Screen.height) / 2;
        Vector2 threshold = getPlayableScreenSize(screenEdgeThreshold);
        //if merky is now on edge of screen
        if (Mathf.Abs(screenPos.x - centerScreen.x) >= threshold.x
            || Mathf.Abs(screenPos.y - centerScreen.y) >= threshold.y)
        {
            //and new pos is further from center than old pos,
            if (Mathf.Abs(screenPos.x - centerScreen.x) > Mathf.Abs(oldScreenPos.x - centerScreen.x)
                || Mathf.Abs(screenPos.y - centerScreen.y) >= Mathf.Abs(oldScreenPos.y - centerScreen.y))
            {
                //zero the offset
                recenter();
            }
        }

        //
        // Auto Offset
        //
        if (!lockCamera)
        {
            Vector2 newBuffer = (newPos - oldPos);
            //If the last teleport direction is similar enough to the most recent teleport direction
            if (Vector2.Angle(previousMoveDir, newBuffer) < autoOffsetAngleThreshold)
            {
                //Update newBuffer in respect to tap speed
                newBuffer *= Mathf.SmoothStep(0, maxTapDelay, 1 - (Time.time - lastTapTime)) / maxTapDelay;
                //Update the auto offset                
                autoOffset += newBuffer;
                //Cap the auto offset
                Vector2 autoScreenPos = cam.WorldToScreenPoint(autoOffset + (Vector2)transform.position);
                Vector2 playableAutoOffsetSize = getPlayableScreenSize(autoOffsetScreenEdgeThreshold);
                //If the auto offset is outside the threshold,
                if (Mathf.Abs(autoScreenPos.x - centerScreen.x) > playableAutoOffsetSize.x)
                {
                    //bring it inside the threshold
                    autoScreenPos.x = centerScreen.x
                        + (
                            playableAutoOffsetSize.x * Mathf.Sign(autoScreenPos.x - centerScreen.x)
                        );
                }
                if (Mathf.Abs(autoScreenPos.y - centerScreen.y) > playableAutoOffsetSize.y)
                {
                    autoScreenPos.y = centerScreen.y
                        + (
                            playableAutoOffsetSize.y * Mathf.Sign(autoScreenPos.y - centerScreen.y)
                        );
                }
                //After fixing autoScreenPos, use it to update autoOffset
                autoOffset = cam.ScreenToWorldPoint(autoScreenPos) - transform.position;
                autoOffsetCancelTime = Time.time + autoOffsetDuration;
            }
            else
            {
                //if prev dir is not similar enough to new dir,
                //remove autoOffset
                autoOffset = Vector2.zero;
            }
        }
        previousMoveDir = (newPos - oldPos);
        lastTapTime = Time.time;
    }

    Vector2 getPlayableScreenSize(float percentage)
    {
        float thresholdBorder = ((1 - percentage) * Mathf.Max(Screen.width, Screen.height) / 2);
        return new Vector2(Screen.width / 2 - thresholdBorder, Screen.height / 2 - thresholdBorder);
    }

    /// <summary>
    /// Sets the camera's offset so it stays at this position relative to the player
    /// </summary>
    public void pinPoint()
    {
        Offset = transform.position - player.transform.position;
        if (offsetOffPlayer())
        {
            lockCamera = true;
            planModeCanvas.SetActive(true);
        }
        else
        {
            lockCamera = false;
            planModeCanvas.SetActive(false);
        }
        autoOffset = Vector2.zero;
        previousMoveDir = Vector2.zero;
    }

    /// <summary>
    /// Recenters on Merky, zeroing the x and y coordinates of the offset
    /// </summary>
    public void recenter()
    {
        Offset = new Vector3(0, 0, offset.z);
        lockCamera = false;
        planModeCanvas.SetActive(false);
    }
    /// <summary>
    /// Moves the camera directly to Merky's position + offset
    /// </summary>
    public void refocus()
    {
        transform.position = player.transform.position + offset;
    }

    /// <summary>
    /// Returns true if the camera is significantly offset from the player
    /// </summary>
    /// <returns></returns>
    public bool offsetOffPlayer()
    {
        return offsetOffPlayerX() || offsetOffPlayerY();
    }
    public bool offsetOffPlayerX()
    {
        Vector2 projection = Vector3.Project(Offset, transform.right);
        return projection.magnitude > cameraOffsetGestureThreshold;
    }
    public bool offsetOffPlayerY()
    {
        Vector2 projection = Vector3.Project(Offset, transform.up);
        return projection.magnitude > cameraOffsetGestureThreshold;
    }

    public delegate void OnOffsetChange();
    public OnOffsetChange onOffsetChange;

    public void setRotation(Vector3 rotationUp)
    {
        this.rotationUp = rotationUp;
    }
    public void processDragGesture(Vector3 origMPWorld, Vector3 newMPWorld)
    {
        //Check to make sure Merky doesn't get dragged off camera
        Vector3 delta = origMPWorld - newMPWorld;
        Vector3 newPos = player.transform.position + originalCameraPosition + delta;
        Vector3 playerUIpos = cam.WorldToViewportPoint(player.transform.position + (new Vector3(cam.transform.position.x, cam.transform.position.y) - newPos));
        if (playerUIpos.x >= 0 && playerUIpos.x <= 1 && playerUIpos.y >= 0 && playerUIpos.y <= 1)
        {
            //Move the camera
            newPos.z = Offset.z;
            transform.position = newPos;
            pinPoint();
        }
    }



    /// <summary>
    /// Called when the zoom level has changed
    /// </summary>
    /// <param name="newZoomLevel">The now current zoom level</param>
    /// <param name="delta">The intended zoom in/out change: negative = in, positive = out</param>
    public delegate void OnZoomLevelChanged(float newZoomLevel, float delta);
    public OnZoomLevelChanged onZoomLevelChanged;

    public void updateOrthographicSize()
    {
        if (Screen.height > Screen.width)//portrait orientation
        {
            cam.orthographicSize = (scale * cam.pixelHeight) / cam.pixelWidth;
        }
        else
        {//landscape orientation
            cam.orthographicSize = scale;
        }
    }

    /// <summary>
    /// Returns whether or not the given position is in the camera's view
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public bool inView(Vector2 position)
    {
        //2017-10-31: copied from an answer by Taylor-Libonati: http://answers.unity3d.com/questions/720447/if-game-object-is-in-cameras-field-of-view.html
        Vector3 screenPoint = cam.WorldToViewportPoint(position);
        return screenPoint.x > 0 && screenPoint.x < 1 && screenPoint.y > 0 && screenPoint.y < 1;
    }
    public float distanceInWorldCoordinates(Vector2 screenPos1, Vector2 screenPos2)
    {
        return Vector2.Distance(cam.ScreenToWorldPoint(screenPos1), cam.ScreenToWorldPoint(screenPos2));
    }
    public float scalePointToZoomLevel(int scalePoint)
    {
        return scalePoints[scalePoint].absoluteScalePoint();
    }

    void cleanDelegates(Scene s)
    {
        if (onZoomLevelChanged != null)
        {
            foreach (OnZoomLevelChanged ozlc in onZoomLevelChanged.GetInvocationList())
            {
                if (ozlc.Target.Equals(null))
                {
                    onZoomLevelChanged -= ozlc;
                }
            }
        }
    }
}
