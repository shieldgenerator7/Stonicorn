using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraController : MonoBehaviour
{
    public GameObject player;
    public float zoomSpeed = 0.5f;//how long it takes to fully change to a new zoom level
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
        private set
        {
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
    private Quaternion rotation;//the rotation the camera should be rotated towards
    private float scale = 1;//scale used to determine orthographicSize, independent of (landscape or portrait) orientation
    private Camera cam;
    private Rigidbody2D playerRB2D;
    private GestureManager gm;
    private PlayerController plyrController;
    private float zoomStartTime = 0.0f;//when the zoom last started
    private float startZoomScale;//the orthographicsize at the start and end of a zoom

    private bool lockCamera = false;//keep the camera from moving

    private int prevScreenWidth;
    private int prevScreenHeight;

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
    int scalePointIndex = 1;//the index of the current scalePoint in scalePoints
    public static int SCALEPOINT_DEFAULT = 2;//the index of the default scalepoint
    public static int SCALEPOINT_TIMEREWIND = 3;//the index of the time rewind mechanic

    // Use this for initialization
    void Start()
    {
        pinPoint();
        cam = GetComponent<Camera>();
        playerRB2D = player.GetComponent<Rigidbody2D>();
        gm = GameObject.FindGameObjectWithTag("GestureManager").GetComponent<GestureManager>();
        plyrController = player.GetComponent<PlayerController>();
        plyrController.onTeleport += checkForAutoMovement;
        scale = cam.orthographicSize;
        rotation = transform.rotation;
        //Initialize ScalePoints
        scalePoints.Add(new ScalePoint(1, false, plyrController));
        scalePoints.Add(new ScalePoint(1, true, plyrController));
        scalePoints.Add(new ScalePoint(2, true, plyrController));
        scalePoints.Add(new ScalePoint(4, true, plyrController));
        //Set the initialize scale point
        setScalePoint(1);
        scale = scalePoints[scalePointIndex].absoluteScalePoint();
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
        if (zoomStartTime != 0)
        {
            zoomToScalePoint();
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

            //Rotate Transform
            if (transform.rotation != rotation)
            {
                float deltaTime = 3 * Time.deltaTime;
                float angle = Quaternion.Angle(transform.rotation, rotation) * deltaTime;
                transform.rotation = Quaternion.Lerp(transform.rotation, rotation, deltaTime);
                Offset = Quaternion.AngleAxis(angle, Vector3.forward) * offset;
            }
        }
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
                //zero the offset,
                //but keep the z coordinate
                Offset -= (Vector3)(Vector2)Offset;
                lockCamera = false;
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
        }
        autoOffset = Vector2.zero;
        previousMoveDir = Vector2.zero;
    }

    /// <summary>
    /// Recenters on Merky
    /// </summary>
    public void recenter()
    {
        Offset = new Vector3(0, 0, offset.z);
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

    public void setRotation(Quaternion rotation)
    {
        this.rotation = rotation;
    }

    public void zoomToScalePoint()
    {
        float absSP = scalePoints[scalePointIndex].absoluteScalePoint();
        scale = Mathf.Lerp(
            startZoomScale,
            absSP,
            (Time.time - zoomStartTime) / zoomSpeed);
        updateOrthographicSize();
        if (scale == absSP)
        {
            zoomStartTime = startZoomScale = 0.0f;
        }

        //Make sure player is still in view
        float width = Vector3.Distance(cam.ScreenToWorldPoint(new Vector3(cam.pixelWidth, 0)), cam.ScreenToWorldPoint(new Vector3(0, 0)));
        float height = Vector3.Distance(cam.ScreenToWorldPoint(new Vector3(0, cam.pixelHeight)), cam.ScreenToWorldPoint(new Vector3(0, 0)));
        float radius = Mathf.Min(width, height) / 2;
        float curDistance = Vector3.Distance(player.transform.position, transform.position);
        if (curDistance > radius)
        {
            float prevZ = offset.z;
            Offset = new Vector2(offset.x, offset.y).normalized * radius;
            offset.z = prevZ;
            refocus();
        }
    }

    public void setScalePoint(int scalePointIndex)
    {
        int spDelta = scalePointIndex - this.scalePointIndex;//"scale point delta"
        //Start the zoom-over-time process
        if (startZoomScale == 0)
        {
            startZoomScale = scalePoints[this.scalePointIndex].absoluteScalePoint();
        }
        else
        {
            startZoomScale = scale;
        }
        zoomStartTime = Time.time;
        //Set the new scale point index
        if (scalePointIndex < 0)
        {
            scalePointIndex = 0;
        }
        else if (scalePointIndex > scalePoints.Count - 1)
        {
            scalePointIndex = scalePoints.Count - 1;
        }
        this.scalePointIndex = scalePointIndex;
        if (onZoomLevelChanged != null)
        {
            onZoomLevelChanged(this.scalePointIndex, spDelta);
        }
    }
    public void adjustScalePoint(int addend)
    {
        setScalePoint(scalePointIndex + addend);
    }
    public int getScalePointIndex()
    {
        return scalePointIndex;
    }
    /// <summary>
    /// Called when the zoom level has changed
    /// </summary>
    /// <param name="newScalePoint">The now current scale point</param>
    /// <param name="delta">The intended zoom in/out change: negative = in, positive = out</param>
    public delegate void OnZoomLevelChanged(int newScalePoint, int delta);
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
