using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GestureManager : MonoBehaviour
{
    //Gesture Profiles
    public enum GestureProfileType
    {
        MENU,//for the menu
        MAIN,//for normal playing of the game
        REWIND,//for activating a rewind and during a rewind
    };
    private GestureProfile currentGP;//the current gesture profile
    private Dictionary<GestureProfileType, GestureProfile> gestureProfiles = new Dictionary<GestureProfileType, GestureProfile>();//dict of valid gesture profiles

    /// <summary>
    /// The input that is currently providing input or that has most recently provided input
    /// </summary>
    private GestureInput activeInput;
    public GestureInput ActiveInput
    {
        get => activeInput;
        set
        {
            GestureInput prevInput = activeInput;
            activeInput = value ?? prevInput;
            if (activeInput != prevInput)
            {
                onInputDeviceSwitched?.Invoke(activeInput.InputType);
                Debug.Log("ActiveInput is now: " + activeInput.InputType);
            }
        }
    }
    public delegate void OnInputDeviceSwitched(InputDeviceMethod inputDevice);
    public event OnInputDeviceSwitched onInputDeviceSwitched;
    private List<GestureInput> gestureInputs;

    // Use this for initialization
    public void init()
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
        gestureInputs.Add(new TouchGestureInput());
        gestureInputs.Add(new MouseGestureInput());
        gestureInputs.Add(new KeyboardGestureInput());
        //Default active input
        ActiveInput = gestureInputs.Find(input => input.InputSupported);
    }

    // Update is called once per frame
    public void processGestures()
    {
        //
        //Input Processing
        //
        bool processed = activeInput.processInput(currentGP);
        if (!processed)
        {
            GestureInput prevInput = activeInput;
            ActiveInput = gestureInputs.Find(input => input.InputOngoing);
            if (activeInput != prevInput)
            {
                activeInput.processInput(currentGP);
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
}
