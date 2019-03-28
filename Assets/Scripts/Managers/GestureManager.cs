using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GestureManager : SavableMonoBehaviour
{

    public GameObject player;
    private PlayerController plrController;
    private Rigidbody2D rb2dPlayer;
    public Camera cam;
    private CameraController camController;

    //Settings
    public float dragThreshold = 50;//how far from the original mouse position the current position has to be to count as a drag
    public float playerSpeedThreshold = 1;//the maximum speed a player can be going and still be able to do a drag gesture
    public float holdThreshold = 0.1f;//how long the tap has to be held to count as a hold (in seconds)
    public float orthoZoomSpeed = 0.5f;

    //Gesture Profiles
    private GestureProfile currentGP;//the current gesture profile
    private Dictionary<string, GestureProfile> gestureProfiles = new Dictionary<string, GestureProfile>();//dict of valid gesture profiles

    //Gesture Event Methods
    public TapGesture tapGesture;
    public OnInputDeviceSwitched onInputDeviceSwitched;

    //Original Positions
    private Vector3 origMP;//"original mouse position": the mouse position at the last mouse down (or tap down) event
    private Vector3 origMP2;//second orginal "mouse position" for second touch
    private Vector3 origMPWorld;//"original mouse position world" - the original mouse coordinate in the world
    private float origTime = 0f;//"original time": the clock time at the last mouse down (or tap down) event
    private float origOrthoSize = 1f;//"original orthographic size"
    //Current Positions
    private Vector3 curMP;//"current mouse position"
    private Vector3 curMP2;//"current mouse position" for second touch
    private Vector3 curMPWorld;//"current mouse position world" - the mouse coordinates in the world
    private float curTime = 0f;
    //Stats
    private int touchCount = 0;//how many touches to process, usually only 0 or 1, only 2 if zoom
    private float maxMouseMovement = 0f;//how far the mouse has moved since the last mouse down (or tap down) event
    private float holdTime = 0f;//how long the gesture has been held for
    private enum ClickState { Began, InProgress, Ended, None };
    private ClickState clickState = ClickState.None;
    //
    public int tapCount = 0;//how many taps have ever been made, including tap+holds that were sent back as taps
    //Flags
    public bool cameraDragInProgress = false;
    private bool isDrag = false;
    private bool isTapGesture = true;
    private bool isHoldGesture = false;
    private bool isPinchGesture = false;
    private bool isCameraMovementOnly = false;//true to make only the camera move until the gesture is over
    public const float holdTimeScale = 0.5f;//how fast time moves during a hold gesture (1 = normal, 0.5 = half speed, 2 = double speed)
    public const float holdTimeScaleRecip = 1 / holdTimeScale;
    public float holdThresholdScale = 1.0f;//the amount to multiply the holdThreshold by
    private InputDeviceMethod lastUsedInputDevice = InputDeviceMethod.NONE;
    //Cheats
    public const bool CHEATS_ALLOWED = true;//whether or not cheats are allowed (turned off for final version)
    private int cheatTaps = 0;//how many taps have been put in for the cheat
    private float cheatTapsTime = 0f;//the time at which the cheat taps will expire
    private int cheatTapsThreshold = 3;//how many taps it takes to activate cheats
    public bool cheatsEnabled = false;//whether or not the cheats are enabled


    // Use this for initialization
    void Start()
    {
        plrController = player.GetComponent<PlayerController>();
        rb2dPlayer = player.GetComponent<Rigidbody2D>();
        camController = cam.GetComponent<CameraController>();
        camController.onZoomLevelChanged += processZoomLevelChange;

        gestureProfiles.Add("Menu", new MenuGestureProfile());
        gestureProfiles.Add("Main", new GestureProfile());
        gestureProfiles.Add("Rewind", new RewindGestureProfile());
        currentGP = gestureProfiles["Menu"];
        currentGP.activate();

        Input.simulateMouseWithTouches = false;
    }
    public override SavableObject getSavableObject()
    {
        return new SavableObject(this,
            "holdThresholdScale", holdThresholdScale,
            "tapCount", tapCount
            );
    }
    public override void acceptSavableObject(SavableObject savObj)
    {
        holdThresholdScale = (float)savObj.data["holdThresholdScale"];
        tapCount = (int)savObj.data["tapCount"];
    }

    // Update is called once per frame
    void Update()
    {
        //
        //Input Device Scouting
        //
        if (onInputDeviceSwitched != null)
        {
            InputDeviceMethod idm = lastUsedInputDevice;
            if (Input.anyKey && !Input.GetMouseButton(0))
            {
                //idm = InputDeviceMethod.KEYBOARD;
            }
            if (Input.mousePresent
                    && (Input.GetMouseButton(0) || Input.GetAxis("Mouse X") != 0 || Input.GetAxis("Mouse Y") != 0))
            {
                idm = InputDeviceMethod.MOUSE;
            }
            if (Input.touchSupported && Input.touchCount > 0)
            {
                idm = InputDeviceMethod.TOUCH;
            }
            //
            if (idm != lastUsedInputDevice)
            {
                lastUsedInputDevice = idm;
                onInputDeviceSwitched(idm);
            }
        }
        //
        //Threshold updating
        //
        float newDT = Mathf.Min(Screen.width, Screen.height) / 20;
        if (dragThreshold != newDT)
        {
            dragThreshold = newDT;
        }
        //
        //Input scouting
        //
        if (Input.touchCount > 2)
        {
            touchCount = 0;
        }
        else if (Input.touchCount >= 1)
        {
            {
                if (Input.GetTouch(0).phase == TouchPhase.Began)
                {
                    beginSingleTapGesture();
                }
                else if (Input.GetTouch(0).phase == TouchPhase.Ended)
                {
                    clickState = ClickState.Ended;
                    if (touchCount == 2)
                    {
                        if (Input.GetTouch(1).phase != TouchPhase.Ended)
                        {
                            beginSingleTapGesture(1);
                        }
                    }
                }
                else
                {
                    clickState = ClickState.InProgress;
                    curMP = Input.GetTouch(0).position;
                }
            }
            if (Input.touchCount == 2)
            {
                if (Input.GetTouch(1).phase == TouchPhase.Began)
                {
                    isPinchGesture = true;
                    isCameraMovementOnly = true;
                    touchCount = 2;
                    clickState = ClickState.Began;
                    origMP2 = Input.GetTouch(1).position;
                    origOrthoSize = camController.ZoomLevel;
                    //Update origMP
                    origMP = Input.GetTouch(0).position;
                }
                else if (Input.GetTouch(1).phase == TouchPhase.Ended)
                {
                    if (Input.GetTouch(0).phase != TouchPhase.Ended)
                    {
                        beginSingleTapGesture();
                    }
                }
                else
                {
                    clickState = ClickState.InProgress;
                    curMP2 = Input.GetTouch(1).position;
                }
            }
        }
        else if (Input.GetMouseButton(0))
        {
            touchCount = 1;
            if (Input.GetMouseButtonDown(0))
            {
                clickState = ClickState.Began;
                origMP = Input.mousePosition;
            }
            else
            {
                clickState = ClickState.InProgress;
                curMP = Input.mousePosition;
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            clickState = ClickState.Ended;
        }
        else if (Input.GetAxis("Mouse ScrollWheel") != 0)
        {
            isPinchGesture = true;
            clickState = ClickState.InProgress;
        }
        else if (Input.touchCount == 0 && !Input.GetMouseButton(0))
        {
            touchCount = 0;
            clickState = ClickState.None;
            //
            isDrag = false;
            isPinchGesture = false;
            isCameraMovementOnly = false;
        }

        //
        //Preliminary Processing
        //Stats are processed here
        //
        switch (clickState)
        {
            case ClickState.Began:
                curMP = origMP;
                maxMouseMovement = 0;
                camController.originalCameraPosition = cam.transform.position - player.transform.position;
                origTime = Time.time;
                curTime = origTime;
                curMP2 = origMP2;
                origMPWorld = (Vector2)cam.ScreenToWorldPoint(origMP);
                break;
            case ClickState.Ended: //do the same thing you would for "in progress"
            case ClickState.InProgress:
                float mm = Vector3.Distance(curMP, origMP);
                if (mm > maxMouseMovement)
                {
                    maxMouseMovement = mm;
                }
                curTime = Time.time;
                holdTime = curTime - origTime;
                break;
            case ClickState.None: break;
            default:
                throw new System.Exception("Click State of wrong type, or type not processed! (Stat Processing) clickState: " + clickState);
        }
        curMPWorld = (Vector2)cam.ScreenToWorldPoint(curMP);//cast to Vector2 to force z to 0


        //
        //Input Processing
        //
        if (touchCount == 1)
        {
            if (clickState == ClickState.Began)
            {
                //Set all flags = true
                cameraDragInProgress = false;
                isDrag = false;
                if (!isCameraMovementOnly)
                {
                    isTapGesture = true;
                }
                else
                {
                    isTapGesture = false;
                }
                isHoldGesture = false;
                isPinchGesture = touchCount == 2;
                if (CHEATS_ALLOWED && curMP.x < 20 && curMP.y < 20)
                {
                    cheatTaps++;
                    cheatTapsTime = Time.time + 1;//give one more second to enter taps
                    if (cheatTaps >= cheatTapsThreshold)
                    {
                        cheatsEnabled = !cheatsEnabled;
                        cheatTaps = 0;
                        cheatTapsTime = 0;
                    }
                }
            }
            else if (clickState == ClickState.InProgress)
            {
                if (maxMouseMovement > dragThreshold
                    && rb2dPlayer.velocity.sqrMagnitude <= playerSpeedThreshold * playerSpeedThreshold)
                {
                    if (!isHoldGesture && !isPinchGesture)
                    {
                        isTapGesture = false;
                        isDrag = true;
                        cameraDragInProgress = true;
                    }
                }
                if (holdTime > holdThreshold * holdThresholdScale)
                {
                    if (!isDrag && !isPinchGesture && !isCameraMovementOnly)
                    {
                        isTapGesture = false;
                        isHoldGesture = true;
                        Time.timeScale = holdTimeScale;
                    }
                }
                if (isDrag)
                {
                    currentGP.processDragGesture(cam.ScreenToWorldPoint(origMP), curMPWorld);
                }
                else if (isHoldGesture)
                {
                    currentGP.processHoldGesture(curMPWorld, holdTime, false);
                }
            }
            else if (clickState == ClickState.Ended)
            {
                if (isDrag)
                {
                    camController.pinPoint();
                }
                else if (isHoldGesture)
                {
                    currentGP.processHoldGesture(curMPWorld, holdTime, true);
                }
                else if (isTapGesture)
                {
                    tapCount++;
                    adjustHoldThreshold(holdTime, false);
                    bool checkPointPort = false;//Merky is in a checkpoint teleporting to another checkpoint
                    if (plrController.InCheckPoint)
                    {
                        foreach (GameObject go in GameObject.FindGameObjectsWithTag("Checkpoint_Root"))
                        {
                            if (go.GetComponent<CheckPointChecker>().checkGhostActivation(curMPWorld))
                            {
                                checkPointPort = true;
                                currentGP.processTapGesture(go);
                                if (tapGesture != null)
                                {
                                    tapGesture();
                                }
                                break;
                            }
                        }
                    }
                    if (!checkPointPort)
                    {
                        currentGP.processTapGesture(curMPWorld);
                        if (tapGesture != null)
                        {
                            tapGesture();
                        }
                    }
                }

                //Set all flags = false
                cameraDragInProgress = false;
                isDrag = false;
                isTapGesture = false;
                isHoldGesture = false;
                isPinchGesture = false;
                isCameraMovementOnly = false;
                Time.timeScale = 1;
            }
            else
            {
                throw new System.Exception("Click State of wrong type, or type not processed! (Input Processing) clickState: " + clickState);
            }

        }
        if (isPinchGesture)
        {//touchCount == 0 || touchCount >= 2
            if (clickState == ClickState.Began)
            {
            }
            else if (clickState == ClickState.InProgress)
            {
                //
                //Zoom Processing
                //
                //
                //Mouse Scrolling Zoom
                //
                if (Input.GetAxis("Mouse ScrollWheel") < 0)
                {
                    camController.ZoomLevel = camController.ZoomLevel + 1;
                }
                else if (Input.GetAxis("Mouse ScrollWheel") > 0)
                {
                    camController.ZoomLevel = camController.ZoomLevel - 1;
                }
                //
                //Pinch Touch Zoom
                //2015-12-31 (1:23am): copied from https://unity3d.com/learn/tutorials/modules/beginner/platform-specific/pinch-zoom
                //

                // If there are two touches on the device...
                if (touchCount == 2)
                {
                    // Store both touches.
                    Touch touchZero = Input.GetTouch(0);
                    Touch touchOne = Input.GetTouch(1);

                    // Find the position in the previous frame of each touch.
                    Vector2 touchZeroPrevPos = origMP;
                    Vector2 touchOnePrevPos = origMP2;

                    // Find the magnitude of the vector (the distance) between the touches in each frame.
                    float prevTouchDeltaMag = camController.distanceInWorldCoordinates(touchZeroPrevPos, touchOnePrevPos);
                    float touchDeltaMag = camController.distanceInWorldCoordinates(touchZero.position, touchOne.position);

                    float newZoomLevel = origOrthoSize * prevTouchDeltaMag / touchDeltaMag;

                    camController.ZoomLevel = newZoomLevel;
                }
            }
            else if (clickState == ClickState.Ended)
            {
                origOrthoSize = camController.ZoomLevel;
            }
        }

        //
        //Opening Main Menu
        //
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (MenuManager.isMenuOpen())
            {
                camController.ZoomScalePoint = CameraController.CameraScalePoints.RANGE;
            }
            else
            {
                camController.ZoomScalePoint = CameraController.CameraScalePoints.MENU;
            }
        }
        //
        //Cheats
        //
        if (cheatTapsTime <= Time.time)
        {
            //Reset cheat taps
            cheatTaps = 0;
        }
    }

    /// <summary>
    /// Used in Update() to convey that the Input
    /// indicates the beginning of a new single-tap gesture,
    /// used often to transition between gestures with continuous input
    /// </summary>
    /// <param name="tapIndex">The index of the tap in Input.GetTouch()</param>
    void beginSingleTapGesture(int tapIndex = 0)
    {
        touchCount = 1;
        clickState = ClickState.Began;
        origMP = Input.GetTouch(tapIndex).position;
        if (isPinchGesture)
        {
            isDrag = true;
        }
        else { 
            isCameraMovementOnly = false;
        }
    }

    /// <summary>
    /// Accepts the given holdTime as not a hold but a tap and adjusts holdThresholdScale
    /// Used by outside classes to indicate that a tap gesture was incorrectly classified as a hold gesture
    /// </summary>
    /// <param name="holdTime"></param>
    public void adjustHoldThreshold(float holdTime)
    {
        adjustHoldThreshold(holdTime, true);
    }
    /// <summary>
    /// Used by the GestureManager to adapt hold threshold even when gestures are being classified correctly
    /// Expects tapCount to never be 0 when called directly from GestureManager
    /// </summary>
    /// <param name="holdTime"></param>
    /// <param name="incrementTapCount"></param>
    private void adjustHoldThreshold(float holdTime, bool incrementTapCount)
    {
        if (incrementTapCount)
        {
            tapCount++;
        }
        holdThresholdScale = (holdThresholdScale * (tapCount - 1) + (holdTime / holdThreshold)) / tapCount;
        if (holdThresholdScale < 1)
        {
            holdThresholdScale = 1.0f;//keep it from going lower than the default holdThreshold
        }
    }
    /// <summary>
    /// Returns the absolute hold threshold, including its scale
    /// </summary>
    /// <returns></returns>
    public float getHoldThreshold()
    {
        return holdThreshold * holdThresholdScale;
    }
    /// <summary>
    /// Switches the gesture profile to the profile with the given name
    /// </summary>
    /// <param name="gpName">The name of the GestureProfile</param>
    public void switchGestureProfile(string gpName)
    {
        //Deactivate current
        currentGP.deactivate();
        //Switch from current to new
        currentGP = gestureProfiles[gpName];
        //Activate new
        currentGP.activate();
    }

    public void processZoomLevelChange(float newZoomLevel, float delta)
    {
        currentGP.processZoomLevelChange(newZoomLevel);
    }

    /// <summary>
    /// Gets called when a tap gesture is processed
    /// </summary>
    public delegate void TapGesture();

    /// <summary>
    /// Gets called when the currently used input device is different than the last used input device
    /// </summary>
    /// <param name="inputDevice"></param>
    public delegate void OnInputDeviceSwitched(InputDeviceMethod inputDevice);
}