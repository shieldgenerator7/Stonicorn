using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class CameraController : MonoBehaviour
{

    public GameObject player;
    public float zoomSpeed = 0.5f;//how long it takes to fully change to a new zoom level
    public float cameraOffsetThreshold = 2.0f;//how far off the center of the screen Merky must be for the hold gesture to behave differently


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

    /// <summary>
    /// If Merky is on the edge of the screen, discard movement delay
    /// </summary>
    /// <param name="oldPos">Where merky just was</param>
    /// <param name="newPos">Where merky is now</param>
    public void checkForAutoMovement(Vector2 oldPos, Vector2 newPos)
    {
        //If the player is near the edge of the screen upon teleporting, recenter the screen
        float SCREEN_EDGE_THRESHOLD = 0.9f;
        Vector2 screenPos = cam.WorldToScreenPoint(newPos);
        Vector2 oldScreenPos = cam.WorldToScreenPoint(oldPos);
        Vector2 centerScreen = new Vector2(Screen.width, Screen.height) / 2;
        float thresholdBorder = ((1 - SCREEN_EDGE_THRESHOLD) * Mathf.Max(Screen.width, Screen.height) / 2);
        Vector2 threshold = new Vector2(Screen.width / 2 - thresholdBorder, Screen.height / 2 - thresholdBorder);
        //if merky is now on edge of screen
        if (Mathf.Abs(screenPos.x - centerScreen.x) >= threshold.x
            || Mathf.Abs(screenPos.y - centerScreen.y) >= threshold.y)
        {
            //and new pos is further from center than old pos,
            if (Mathf.Abs(screenPos.x - centerScreen.x) > Mathf.Abs(oldScreenPos.x - centerScreen.x))
            {
                //zero the offset
                Vector2 projection = Vector3.Project((Vector2)Offset, transform.right);
                Offset -= (Vector3)projection;
            }
            if (Mathf.Abs(screenPos.y - centerScreen.y) >= Mathf.Abs(oldScreenPos.y - centerScreen.y))
            {
                Vector2 projection = Vector3.Project((Vector2)Offset, transform.up);
                Offset -= (Vector3)projection;
            }
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

    /// <summary>
    /// Returns true if the camera is significantly offset from the player
    /// </summary>
    /// <returns></returns>
    public bool offsetOffPlayer()
    {
        return cameraOffsetThreshold * cameraOffsetThreshold <= ((Vector2)Offset).sqrMagnitude;
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
