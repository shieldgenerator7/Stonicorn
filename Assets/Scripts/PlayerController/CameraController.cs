using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float zoomSpeed = 0.5f;//how long it takes to fully change to a new zoom level
    public float cameraOffsetGestureThreshold = 2.0f;//how far off the center of the screen Merky must be for the hold gesture to behave differently
    public float screenEdgeThreshold = 0.9f;//the percentage of half the screen that is in the middle, the rest is the edge
    public float autoOffsetScreenEdgeThreshold = 0.7f;//same as screenEdgeThreshold, but used for the purposes of autoOffset
    public float cameraMoveFactor = 1.5f;
    public float autoOffsetDuration = 1;//how long autoOffset lasts after the latest teleport
    public float autoOffsetAngleThreshold = 15f;//how close two teleport directions have to be to activate auto offset
    public float maxTapDelay = 1;//the maximum amount of time (sec) between two taps that can activate auto offset
    public GameObject planModeCanvas;//the canvas that has the UI for plan mode
    public float defaultOffsetZ = -10;


    private Vector3 offset;
    public Vector3 Offset
    {
        get => offset;
        set
        {
            if (value.z == 0)
            {
                value.z = offset.z;
                //If the z offset is still 0,
                if (value.z == 0)
                {
                    //Use the default value
                    value.z = defaultOffsetZ;
                }
            }
            offset = value;
            onOffsetChange?.Invoke(offset);
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
        => transform.position - Managers.Player.transform.position + offset;

    /// <summary>
    /// The up direction that the camera should be rotated towards
    /// </summary>
    public Vector3 Up
    {
        get => rotationUp;
        set
        {
            if (!Locked)
            {
                rotationUp = value;
            }
        }
    }
    private Vector3 rotationUp;

    private float scale = 1;//scale used to determine fieldOfView, independent of (landscape or portrait) orientation
    private float desiredScale = 0;//the value that scale should move towards
    private new Camera camera;
    private Camera Cam
    {
        get
        {
            if (camera == null)
            {
                camera = GetComponent<Camera>();
            }
            return camera;
        }
    }

    /// <summary>
    /// While true, the camera cannot move or rotate
    /// </summary>
    public bool Locked
    {
        get => lockCamera;
        set
        {
            lockCamera = value;
            planModeCanvas.SetActive(lockCamera);
        }
    }
    private bool lockCamera = false;

    [Tooltip("Runtime Var, Doesn't do anything from editor")]
    public Vector2 originalCameraPosition;//"original camera position": the camera offset (relative to the player) at the last mouse down (or tap down) event

    private int prevScreenWidth;
    private int prevScreenHeight;

    public float ZoomLevel
    {
        get => scale;
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
                onZoomLevelChanged?.Invoke(scale, scale - prevScale);
                updateFieldOfView();
            }
        }
    }
    public CameraScalePoints ZoomScalePoint
    {
        set { ZoomLevel = scalePoints[(int)value].absoluteScalePoint(); }
    }
    /// <summary>
    /// Set this to make the scale smoothly move to the new value
    /// </summary>
    public float TargetZoomLevel
    {
        get => desiredScale;
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
    /// <summary>
    /// Used to set the target zoom level using scale points
    /// </summary>
    public CameraScalePoints TargetScalePoint
    {
        set
        {
            TargetZoomLevel = scalePoints[(int)value].absoluteScalePoint();
        }
    }
    struct ScalePoint
    {
        private float scalePoint;
        private bool relative;//true if relative to player's range, false if absolute
        public ScalePoint(float scale, bool relative)
        {
            scalePoint = scale;
            this.relative = relative;
        }
        public float absoluteScalePoint()
        {
            if (relative)
            {
                return scalePoint * Managers.Player.Teleport.baseRange;
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
    public void init()
    {
        Managers.Player.Teleport.onTeleport += checkForAutoMovement;
        if (planModeCanvas.GetComponent<Canvas>() == null)
        {
            Debug.LogError("Camera " + gameObject.name + "'s planModeCanvas object (" + planModeCanvas.name + ") doesn't have a Canvas component!");
        }
        scale = Cam.fieldOfView;
        Up = transform.up;
        //Initialize ScalePoints
        scalePoints.Add(new ScalePoint(0.2f * 11, false));//Main Menu zoom level
        scalePoints.Add(new ScalePoint(1 * 11, false));
        scalePoints.Add(new ScalePoint(1 * 11, true));
        scalePoints.Add(new ScalePoint(2 * 11, true));
        scalePoints.Add(new ScalePoint(4 * 11, true));
        //Set the initialize scale point
        scale = scalePoints[0].absoluteScalePoint();
        //Position initialization
        pinPoint();
        recenter();
        refocus();
    }

    public void checkScreenDimensions()
    {
        if (prevScreenHeight != Screen.height || prevScreenWidth != Screen.width)
        {
            prevScreenWidth = Screen.width;
            prevScreenHeight = Screen.height;
            updateFieldOfView();
        }
    }

    // Update is called once per frame, after all other objects have moved that frame
    public void updateCameraPosition()
    {
        if (true)//!Managers.Gesture.cameraDragInProgress)
        {
            if (!Locked)
            {
                //Target
                Vector3 target = Managers.Player.transform.position + offset + (Vector3)autoOffset;
                //Speed
                float speed = (
                        Vector3.Distance(transform.position, target)
                        * cameraMoveFactor
                        + Managers.Player.Speed
                    )
                    * Time.unscaledDeltaTime;
                //Move Transform
                transform.position = Vector3.Lerp(transform.position, target, speed);

                if (autoOffsetCancelTime > 0)
                {
                    if (Time.time > autoOffsetCancelTime
                        && !Managers.Time.SlowTime)
                    {
                        autoOffset = Vector2.zero;
                        previousMoveDir = Vector2.zero;
                        autoOffsetCancelTime = 0;
                    }
                }
            }
            else
            {
                if (!inView(Managers.Player.transform.position))
                {
                    recenter();
                }
            }

            //Rotate Transform
            if (!RotationFinished)
            {
                float deltaTime = 3 * Time.unscaledDeltaTime;
                transform.up = Vector3.Lerp(transform.up, Up, deltaTime);
            }

            //Scale Orthographic Size
            if (TargetZoomLevel > 0)
            {
                //If current zoom is not target zoom,
                if (ZoomLevel != TargetZoomLevel
                    //and current zoom is between starting zoom and target zoom,
                    && (Mathf.Clamp(ZoomLevel, preTargetZoomLevel, TargetZoomLevel) == ZoomLevel
                    || Mathf.Clamp(ZoomLevel, TargetZoomLevel, preTargetZoomLevel) == ZoomLevel))
                {
                    //Move current zoom closer to target zoom
                    ZoomLevel = Mathf.Lerp(ZoomLevel, TargetZoomLevel, Time.unscaledDeltaTime);
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

    public bool RotationFinished
        => transform.up == Up;


    /// <summary>
    /// If Merky is on the edge of the screen, discard movement delay
    /// </summary>
    /// <param name="oldPos">Where merky just was</param>
    /// <param name="newPos">Where merky is now</param>
    public void checkForAutoMovement(Vector2 oldPos, Vector2 newPos)
    {
        //If the player is near the edge of the screen upon teleporting, recenter the screen
        Vector2 screenPos = Cam.WorldToScreenPoint(newPos);
        Vector2 oldScreenPos = Cam.WorldToScreenPoint(oldPos);
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
        if (!Locked)
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
                Vector2 autoScreenPos = Cam.WorldToScreenPoint(autoOffset + (Vector2)transform.position);
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
                autoOffset = Utility.ScreenToWorldPoint(autoScreenPos) - (Vector2)transform.position;
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
        Offset = transform.position - Managers.Player.transform.position;
        Locked = offsetOffPlayer();
        autoOffset = Vector2.zero;
        previousMoveDir = Vector2.zero;
    }

    /// <summary>
    /// Recenters on Merky, zeroing the x and y coordinates of the offset
    /// </summary>
    public void recenter()
    {
        Offset = Vector3.zero;
        Locked = false;
    }
    /// <summary>
    /// Moves the camera directly to Merky's position + offset
    /// </summary>
    public void refocus()
    {
        transform.position = Managers.Player.transform.position + offset;
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

    public delegate void OnOffsetChange(Vector3 offset);
    public event OnOffsetChange onOffsetChange;

    public void processDragGesture(Vector2 origMPWorld, Vector2 newMPWorld, bool finished)
    {
        bool canMove = false;
        Vector2 delta = origMPWorld - newMPWorld;
        Vector2 playerPos = Managers.Player.transform.position;
        Vector3 newPos = playerPos + originalCameraPosition + delta;
        //If the camera is not zoomed into the menu,
        if (ZoomLevel > toZoomLevel(CameraScalePoints.MENU))
        {
            //Check to make sure Merky doesn't get dragged off camera
            Vector2 playerUIpos = Cam.WorldToViewportPoint(playerPos + (Vector2)Cam.transform.position - (Vector2)newPos);
            if (playerUIpos.x >= 0 && playerUIpos.x <= 1 && playerUIpos.y >= 0 && playerUIpos.y <= 1)
            {
                canMove = true;
            }
        }
        else
        {
            canMove = true;
        }
        if (canMove)
        {
            //Move the camera
            newPos.z = Offset.z;
            transform.position = newPos;
            pinPoint();
        }
        if (finished)
        {
            originalCameraPosition = Offset;
        }
    }



    /// <summary>
    /// Called when the zoom level has changed
    /// </summary>
    /// <param name="newZoomLevel">The now current zoom level</param>
    /// <param name="delta">The intended zoom in/out change: negative = in, positive = out</param>
    public delegate void OnZoomLevelChanged(float newZoomLevel, float delta);
    public event OnZoomLevelChanged onZoomLevelChanged;

    public void updateFieldOfView()
    {
        //portrait orientation
        if (Screen.height > Screen.width)
        {
            Cam.fieldOfView = (scale * Cam.pixelHeight) / Cam.pixelWidth;
        }
        //landscape orientation
        else
        {
            Cam.fieldOfView = scale;
        }
    }
    public Vector2 CamSizeWorld
        => new Vector2(
            CameraWidthWorld,
            CameraHeightWorld
            );

    public float CameraWidthWorld
        => Vector2.Distance(
            Utility.ScreenToWorldPoint(Vector2.zero),
            Utility.ScreenToWorldPoint(
                new Vector2(Cam.pixelWidth, 0)
                )
            );

    public float CameraHeightWorld
        => Vector2.Distance(
            Utility.ScreenToWorldPoint(Vector2.zero),
            Utility.ScreenToWorldPoint(
                new Vector2(0, Cam.pixelHeight)
                )
            );

    /// <summary>
    /// Returns whether or not the given position is in the camera's view
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public bool inView(Vector2 position)
    {
        //2017-10-31: copied from an answer by Taylor-Libonati: http://answers.unity3d.com/questions/720447/if-game-object-is-in-cameras-field-of-view.html
        Vector3 screenPoint = Cam.WorldToViewportPoint(position);
        return screenPoint.x > 0 && screenPoint.x < 1 && screenPoint.y > 0 && screenPoint.y < 1;
    }
    public Vector2 getInViewPosition(Vector2 position, float distanceFactor)
    {
        //Convert to viewport space
        Vector2 vpPos = Cam.WorldToViewportPoint(position);
        Vector2 middlePos = new Vector2(0.5f, 0.5f);
        //Find direction
        Vector2 direction = vpPos - middlePos;
        Vector2 newPos = middlePos + (direction.normalized * 0.5f * distanceFactor);
        //Convert back to world space
        return Utility.ScreenToWorldPoint(Cam.ViewportToScreenPoint(newPos));
    }
    public float distanceInWorldCoordinates(Vector2 screenPos1, Vector2 screenPos2)
    {
        return Vector2.Distance(Utility.ScreenToWorldPoint(screenPos1), Utility.ScreenToWorldPoint(screenPos2));
    }
    public float toZoomLevel(CameraScalePoints csp)
    {
        return scalePointToZoomLevel((int)csp);
    }
    private float scalePointToZoomLevel(int scalePoint)
    {
        if (scalePoint < 0 || scalePoint >= scalePoints.Count)
        {
            throw new System.ArgumentOutOfRangeException("scalePoint", scalePoint,
                "scalePoint should be between " + 0 + " and " + (scalePoints.Count - 1) + ", inclusive. scalePoint: " + scalePoint);
        }
        return scalePoints[scalePoint].absoluteScalePoint();
    }
}
