using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CameraController : MonoBehaviour
{

    public GameObject player;
    public float zoomSpeed = 0.5f;//how long it takes to fully change to a new zoom level

    private Vector3 offset;
    public Vector3 Offset
    {
        get { return offset; }
        private set {
            offset = value;
            if (onOffsetChange != null)
            {
                onOffsetChange();
            }
        }
    }
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
    private float moveTime = 0f;//used to delay the camera refocusing on the player
    private bool wasDelayed = false;
    private GestureManager gm;
    private PlayerController plyrController;
    private float zoomStartTime = 0.0f;//when the zoom last started
    private float startZoomScale;//the orthographicsize at the start and end of a zoom

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
        //transform.position = player.transform.position + offset;
        if (moveTime <= Time.time && !gm.cameraDragInProgress)
        {
            if (wasDelayed)
            {
                wasDelayed = false;
                recenter();
            }
            transform.position = Vector3.MoveTowards(
                transform.position,
                player.transform.position + offset,
                (Vector3.Distance(
                    transform.position + offset,
                    player.transform.position) * 1.5f + playerRB2D.velocity.magnitude)
                    * Time.deltaTime);
            if (transform.rotation != rotation)
            {
                float deltaTime = 3 * Time.deltaTime;
                float angle = Quaternion.Angle(transform.rotation, rotation) * deltaTime;
                transform.rotation = Quaternion.Lerp(transform.rotation, rotation, deltaTime);
                Offset = Quaternion.AngleAxis(angle, Vector3.forward) * offset;
            }
        }
    }

    /**
    * Makes sure that the current camera movement delay is at least the given delay amount in seconds
    * @param delayAmount How much to delay camera movement by in seconds
    */
    public void delayMovement(float delayAmount)
    {
        if (moveTime < Time.time + delayAmount)
        {
            moveTime = Time.time + delayAmount;
        }
        wasDelayed = true;
    }

    public void discardMovementDelay()
    {
        moveTime = Time.time;
        wasDelayed = false;
    }
    /// <summary>
    /// If Merky is between the tap pos and the camera pos, discard movement delay
    /// </summary>
    /// <param name="tapPos">The world-sapce coordinate where the player tapped</param>
    /// <param name="playerPos">The world-space coordiante where the player is after teleporting</param>
    public void checkForAutoMovement(Vector3 tapPos, Vector3 playerPos)
    {
        //If the player is near the edge of the screen upon teleporting, recenter the screen
        float DISCARD_DELAY_DISTANCE_SENSITIVITY = 0.9f;
        float DISCARD_DELAY_ANGLE_SENSITIVITY = 140;//in degrees
        //Get the min of screen width and height in world distance
        float distance = Mathf.Abs(cam.ScreenToWorldPoint(new Vector2(0, Mathf.Min(prevScreenWidth, prevScreenHeight))).y - cam.ScreenToWorldPoint(new Vector2(0, 0)).y);
        float threshold = DISCARD_DELAY_DISTANCE_SENSITIVITY * distance / 2;
        if (Vector2.Distance((Vector2)transform.position, playerPos) >= threshold)
        {
            Vector2 plyV = (playerPos - transform.position);
            Vector2 tapVd = (tapPos - transform.position);
            float angled = Vector2.Angle(tapVd.normalized, plyV.normalized);
            if (angled < DISCARD_DELAY_ANGLE_SENSITIVITY)
            {//unless the camera is mostly between Merky and the tap pos
                recenter();
                discardMovementDelay();
                return;//dont need to test the other case
            }
        }
        //Test the angle tap-player-cam
        Vector2 tapV = (tapPos - playerPos);
        Vector2 camV = (transform.position - playerPos);
        float angle = Vector2.Angle(tapV.normalized, camV.normalized);
        if (angle >= DISCARD_DELAY_ANGLE_SENSITIVITY)
        {
            recenter();
            discardMovementDelay();
        }
    }

    /// <summary>
    /// Sets the camera's offset so it stays at this position relative to the player
    /// </summary>
    public void pinPoint()
    {
        Offset = transform.position - player.transform.position;
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
}
