using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GestureManager : SavableMonoBehaviour
{
    //Settings
    public float dragThreshold = 50;//how far from the original mouse position the current position has to be to count as a drag
    public float holdThreshold = 0.1f;//how long the tap has to be held to count as a hold (in seconds)
    public float orthoZoomSpeed = 0.5f;

    //Gesture Profiles
    public enum GestureProfileType { MENU, MAIN, REWIND };
    private GestureProfile currentGP;//the current gesture profile
    private Dictionary<GestureProfileType, GestureProfile> gestureProfiles = new Dictionary<GestureProfileType, GestureProfile>();//dict of valid gesture profiles

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
    private float maxMouseMovement = 0f;//how far the mouse has moved since the last mouse down (or tap down) event
    private float holdTime = 0f;//how long the gesture has been held for
    private enum ClickState { Began, InProgress, Ended, None };
    private ClickState clickState = ClickState.None;
    //Flags
    public bool cameraDragInProgress = false;
    private bool isDrag = false;
    private bool isTapGesture = true;
    private bool isHoldGesture = false;
    private bool isPinchGesture = false;
    private bool isCameraMovementOnly = false;//true to make only the camera move until the gesture is over
    public float holdThresholdScale = 1.0f;//the amount to multiply the holdThreshold by
    /// <summary>
    /// The input that is currently providing input or that has most recently provided input
    /// </summary>
    private GestureInput activeInput;
    private List<GestureInput> gestureInputs;

    // Use this for initialization
    void Start()
    {
        gestureProfiles.Add(GestureProfileType.MENU, new MenuGestureProfile());
        gestureProfiles.Add(GestureProfileType.MAIN, new GestureProfile());
        gestureProfiles.Add(GestureProfileType.REWIND, new RewindGestureProfile());
        switchGestureProfile(GestureProfileType.MENU);

        Managers.Camera.onZoomLevelChanged += processZoomLevelChange;
        Managers.Camera.ZoomLevel =
            Managers.Camera.toZoomLevel(CameraController.CameraScalePoints.MENU);

        Input.simulateMouseWithTouches = false;

        //Inputs
        gestureInputs = new List<GestureInput>();
        gestureInputs.Add(new MouseGestureInput());
        gestureInputs.Add(new TouchGestureInput());
        activeInput = gestureInputs[1];//TEST
    }
    public override SavableObject getSavableObject()
    {
        return new SavableObject(this,
            "holdThresholdScale", holdThresholdScale
            );
    }
    public override void acceptSavableObject(SavableObject savObj)
    {
        holdThresholdScale = (float)savObj.data["holdThresholdScale"];
    }

    // Update is called once per frame
    void Update()
    {
        //
        //Input Device Scouting
        //
        if (onInputDeviceSwitched != null)
        {
            InputDeviceMethod idm = activeInput.InputType;
            if (!activeInput.InputOngoing)
            {
                //TODO: Check other inputs for being active
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
        //Input Processing
        //
        bool processed = activeInput.processInput(currentGP);
        if (!processed)
        {
            foreach(GestureInput input in gestureInputs)
            {
                if (input.InputOngoing)
                {
                    activeInput = input;
                    activeInput.processInput(currentGP);
                    break;
                }
            }
        }

        //
        //Opening Main Menu
        //
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (MenuManager.Open)
            {
                Managers.Camera.ZoomScalePoint = CameraController.CameraScalePoints.RANGE;
            }
            else
            {
                Managers.Camera.ZoomScalePoint = CameraController.CameraScalePoints.MENU;
            }
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
            GameStatistics.addOne("Tap");
        }
        int tapCount = GameStatistics.get("Tap");
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
    public float HoldThreshold
    {
        get
        {
            return holdThreshold * holdThresholdScale;
        }
    }
    /// <summary>
    /// Switches the gesture profile to the profile with the given name
    /// </summary>
    /// <param name="gpName">The name of the GestureProfile</param>
    public void switchGestureProfile(GestureProfileType gpt)
    {
        GestureProfile newGP = gestureProfiles[gpt];
        //If the gesture profile is not already active,
        if (newGP != currentGP)
        {
            //Deactivate current
            if (currentGP != null)
            {
                currentGP.deactivate();
            }
            //Switch from current to new
            currentGP = newGP;
            //Activate new
            currentGP.activate();
        }
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
