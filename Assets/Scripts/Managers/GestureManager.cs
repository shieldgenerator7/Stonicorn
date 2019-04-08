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
    public bool isDrag = false;
    public bool isTapGesture = true;
    public bool isHoldGesture = false;
    public bool isZoomGesture = false;
    public bool isCameraMovementOnly = false;//true to make only the camera move until the gesture is over

    private const float holdTimeScale = 0.5f;//how fast time moves during a hold gesture (1 = normal, 0.5 = half speed, 2 = double speed)
    private const float holdTimeScaleRecip = 1 / holdTimeScale;
    private InputDeviceMethod lastUsedInputDevice = InputDeviceMethod.NONE;

    // Use this for initialization
    void Start()
    {
        playerInput.Add(new PlayerInputMouse());
        playerInput.Add(new PlayerInputTouch());

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



        PlayerInput.InputData inputData = null;
        foreach (PlayerInput input in playerInput)
        {
            inputData = input.getInput();
            if (inputData.inputState != PlayerInput.InputState.None)
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
        if (inputData.inputState == PlayerInput.InputState.None)
        {
            return;
        }

        if (inputData.inputState == PlayerInput.InputState.Begin)
        {
            //Set all flags = false
            cameraDragInProgress = false;
            isDrag = false;
            isHoldGesture = false;
            isZoomGesture = false;
            if (!isCameraMovementOnly)
            {
                isTapGesture = true;
            }
            else
            {
                isTapGesture = false;
            }
        }
        else if (inputData.inputState == PlayerInput.InputState.Hold)
        {
            //Gesture type scouting
            if (!isHoldGesture && !isDrag)
            {
                if (inputData.zoomMultiplier != 1)
                {
                    isZoomGesture = true;
                    currentGP.processZoomGesture(inputData.zoomMultiplier, PlayerInput.InputState.Begin);
                    isTapGesture = false;
                    isHoldGesture = false;
                    isDrag = false;
                }
            }
            if (!isHoldGesture && !isZoomGesture)
            {
                if (inputData.PositionDelta > dragThreshold
                && Managers.Player.Speed <= playerSpeedThreshold)
                {
                    isDrag = true;
                    currentGP.processDragGesture(inputData.OldWorldPos, inputData.NewWorldPos, PlayerInput.InputState.Begin);
                    isTapGesture = false;
                    cameraDragInProgress = true;
                }
            }
            if (!isDrag && !isZoomGesture && !isCameraMovementOnly)
            {
                if (inputData.holdTime > holdThreshold)
                {
                    isHoldGesture = true;
                    currentGP.processHoldGesture(inputData.NewWorldPos, inputData.holdTime, PlayerInput.InputState.Begin);
                    isTapGesture = false;
                    Time.timeScale = GestureManager.holdTimeScale;
                }
            }
            //Gesture Type Processing
            if (isDrag)
            {
                currentGP.processDragGesture(inputData.OldWorldPos, inputData.NewWorldPos, inputData.inputState);
            }
            else if (isHoldGesture)
            {
                currentGP.processHoldGesture(inputData.NewWorldPos, inputData.holdTime, inputData.inputState);
            }
            else if (isZoomGesture)
            {
                currentGP.processZoomGesture(inputData.zoomMultiplier, inputData.inputState);
            }
        }
        else if (inputData.inputState == PlayerInput.InputState.End)
        {
            if (isDrag)
            {
                //Update Stats
                GameStatistics.addOne("Drag");
                //Process Drag Gesture
                Managers.Camera.pinPoint();
            }
            else if (isHoldGesture)
            {
                currentGP.processHoldGesture(inputData.NewWorldPos, inputData.holdTime, inputData.inputState);
                GameStatistics.addOne("Hold");
            }
            else if (isTapGesture)
            {
                //Update Stats
                GameStatistics.addOne("Tap");
                //Process Tap Gesture                
                currentGP.processTapGesture(inputData.NewWorldPos);
                if (tapGesture != null)
                {
                    tapGesture();
                }
            }

            //Set all flags = false
            cameraDragInProgress = false;
            isDrag = false;
            isTapGesture = false;
            isHoldGesture = false;
            isZoomGesture = false;
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