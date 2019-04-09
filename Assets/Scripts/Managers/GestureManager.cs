using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GestureManager : MonoBehaviour
{
    //Settings
    public float dragThreshold = 50;//how far from the original mouse position the current position has to be to count as a drag
    public float playerSpeedThreshold = 1;//the maximum speed a player can be going and still be able to do a drag gesture
    public float holdThreshold = 0.1f;//how long the tap has to be held to count as a hold (in seconds)
    public float orthoZoomSpeed = 0.5f;

    //Gesture Profiles
    public enum GestureProfileType { MENU, MAIN, REWIND };
    private GestureProfile currentGP;//the current gesture profile
    private Dictionary<GestureProfileType, GestureProfile> gestureProfiles = new Dictionary<GestureProfileType, GestureProfile>();//dict of valid gesture profiles

    //Gesture Event Methods
    public TapGesture tapGesture;
    public OnInputDeviceSwitched onInputDeviceSwitched;

    //Player Input Data
    public List<PlayerInput> playerInput = new List<PlayerInput>();

    //Flags
    public bool cameraDragInProgress = false;
    public bool isCameraMovementOnly = false;//true to make only the camera move until the gesture is over
    public enum GestureType { TAP, HOLD, DRAG, ZOOM, UNKNOWN };
    private GestureType gestureType = GestureType.UNKNOWN;
    //
    private const float holdTimeScale = 0.5f;//how fast time moves during a hold gesture (1 = normal, 0.5 = half speed, 2 = double speed)
    private const float holdTimeScaleRecip = 1 / holdTimeScale;
    private InputDeviceMethod lastUsedInputDevice = InputDeviceMethod.NONE;

    // Use this for initialization
    void Start()
    {
        playerInput.Add(new PlayerInputMouse());
        playerInput.Add(new PlayerInputTouch());
        playerInput.Add(new PlayerInputKeyboard());

        gestureProfiles.Add(GestureProfileType.MENU, new MenuGestureProfile());
        gestureProfiles.Add(GestureProfileType.MAIN, new GestureProfile());
        gestureProfiles.Add(GestureProfileType.REWIND, new RewindGestureProfile());
        switchGestureProfile(GestureProfileType.MENU);

        Managers.Camera.onZoomLevelChanged += onZoomLevelChange;
        Managers.Camera.ZoomLevel =
            Managers.Camera.toZoomLevel(CameraController.CameraScalePoints.MENU);


        Input.simulateMouseWithTouches = false;
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



        InputData inputData = null;
        foreach (PlayerInput input in playerInput)
        {
            inputData = input.getInput();
            if (inputData.inputState != InputData.InputState.None)
            {
                break;
            }
        }
        if (inputData == null)
        {
            foreach (PlayerInput input in playerInput)
            {
                Debug.Log("/!\\ inputData: " + inputData + " (probably null), input.gI().inputState: " + input.getInput().inputState);
            }
        }
        if (inputData.inputState == InputData.InputState.None)
        {
            return;
        }

        if (inputData.inputState == InputData.InputState.Begin)
        {
            cameraDragInProgress = false;
            gestureType = GestureType.UNKNOWN;
            if (inputData.zoomMultiplier != 1)
            {
                gestureType = GestureType.ZOOM;
                currentGP.processZoomGesture(inputData.zoomMultiplier, InputData.InputState.Begin);
            }
        }
        else if (inputData.inputState == InputData.InputState.Hold)
        {
            //Gesture type scouting
            if (gestureType == GestureType.UNKNOWN)
            {
                if (inputData.zoomMultiplier != 1)
                {
                    gestureType = GestureType.ZOOM;
                    currentGP.processZoomGesture(inputData.zoomMultiplier, InputData.InputState.Begin);
                }
                if (inputData.PositionDelta > dragThreshold
                    && Managers.Player.Speed <= playerSpeedThreshold)
                {
                    gestureType = GestureType.DRAG;
                    currentGP.processDragGesture(inputData.OldWorldPos, inputData.NewWorldPos, InputData.InputState.Begin);
                    cameraDragInProgress = true;
                }
                if (inputData.HoldTime > holdThreshold && !isCameraMovementOnly)
                {
                    gestureType = GestureType.HOLD;
                    currentGP.processHoldGesture(inputData.NewWorldPos, inputData.HoldTime, InputData.InputState.Begin);
                    Time.timeScale = GestureManager.holdTimeScale;
                }
            }
            else
            {
                //Gesture Type Processing
                switch (gestureType)
                {
                    case GestureType.DRAG:
                        currentGP.processDragGesture(inputData.OldWorldPos, inputData.NewWorldPos, inputData.inputState);
                        break;
                    case GestureType.HOLD:
                        currentGP.processHoldGesture(inputData.NewWorldPos, inputData.HoldTime, inputData.inputState);
                        break;
                    case GestureType.ZOOM:
                        currentGP.processZoomGesture(inputData.zoomMultiplier, inputData.inputState);
                        break;
                }
            }
        }
        else if (inputData.inputState == InputData.InputState.End)
        {
            switch (gestureType)
            {
                case GestureType.DRAG:
                    //Update Stats
                    GameStatistics.addOne("Drag");
                    //Process Drag Gesture
                    Managers.Camera.pinPoint();
                    break;
                case GestureType.HOLD:
                    currentGP.processHoldGesture(inputData.NewWorldPos, inputData.HoldTime, inputData.inputState);
                    GameStatistics.addOne("Hold");
                    break;
                case GestureType.ZOOM: break;
                case GestureType.UNKNOWN: //do nothing, proceed to TAP
                case GestureType.TAP:
                    //Update Stats
                    GameStatistics.addOne("Tap");
                    //Process Tap Gesture                
                    currentGP.processTapGesture(inputData.NewWorldPos);
                    if (tapGesture != null)
                    {
                        tapGesture();
                    }
                    break;
                default: throw new System.NotSupportedException("GestureManager.gestureType is an invalid value: " + gestureType);
            }

            //Clear the input data
            inputData.clear();

            //Set all flags = false
            cameraDragInProgress = false;
            isCameraMovementOnly = false;
            Time.timeScale = 1;
        }
        else
        {
            throw new System.Exception("Input State of wrong type, or type not processed! (Input Processing) inputState: " + inputData.inputState);
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
    /// Returns the hold threshold
    /// </summary>
    /// <returns></returns>
    public float HoldThreshold
    {
        get { return holdThreshold; }
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

    public void onZoomLevelChange(float newZoomLevel, float delta)
    {
        currentGP.onZoomLevelChange(newZoomLevel);
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