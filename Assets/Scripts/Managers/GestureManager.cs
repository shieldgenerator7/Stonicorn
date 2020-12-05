using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GestureManager : MonoBehaviour
{
    //Gesture Profiles
    public enum GestureProfileType { MENU, MAIN, REWIND };
    private GestureProfile currentGP;//the current gesture profile
    private Dictionary<GestureProfileType, GestureProfile> gestureProfiles = new Dictionary<GestureProfileType, GestureProfile>();//dict of valid gesture profiles

    //Gesture Event Methods
    //public TapGesture tapGesture;
    public event OnInputDeviceSwitched onInputDeviceSwitched;

    /// <summary>
    /// The input that is currently providing input or that has most recently provided input
    /// </summary>
    private GestureInput activeInput;
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
        activeInput = gestureInputs[0];
    }

    // Update is called once per frame
    public void processGestures()
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
        //Input Processing
        //
        bool processed = activeInput.processInput(currentGP);
        if (!processed)
        {
            foreach (GestureInput input in gestureInputs)
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
    //public delegate void TapGesture();

    /// <summary>
    /// Gets called when the currently used input device is different than the last used input device
    /// </summary>
    /// <param name="inputDevice"></param>
    public delegate void OnInputDeviceSwitched(InputDeviceMethod inputDevice);
}
