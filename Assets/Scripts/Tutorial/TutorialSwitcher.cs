using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialSwitcher : MonoBehaviour
{

    public InputDeviceMethod inputDevice;

    // Start is called before the first frame update
    void Start()
    {
        if (inputDevice == InputDeviceMethod.NONE)
        {
            throw new UnityException("TutorialSwitcher (" + name + ") has invalid InputDeviceMethod: " + inputDevice + "!");
        }
        Managers.Gesture.onInputDeviceSwitched += updateLastUsedInputDevice;
        updateLastUsedInputDevice(Managers.Gesture.ActiveInput.InputType);
    }
    private void OnDestroy()
    {
        Managers.Gesture.onInputDeviceSwitched -= updateLastUsedInputDevice;
    }

    void updateLastUsedInputDevice(InputDeviceMethod inputDevice)
    {
        gameObject.SetActive(inputDevice == this.inputDevice);
    }
}