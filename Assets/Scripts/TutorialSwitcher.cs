using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialSwitcher : MonoBehaviour
{
    
    public InputDeviceMethod inputDevice;

    // Start is called before the first frame update
    void Start()
    {
        Managers.Gesture.onInputDeviceSwitched += updateLastUsedInputDevice;
        if (inputDevice == InputDeviceMethod.NONE)
        {
            throw new UnityException("TutorialSwitcher (" + name + ") has invalid InputDeviceMethod: " + inputDevice+"!");
        }
    }

    void updateLastUsedInputDevice(InputDeviceMethod inputDevice)
    {
        gameObject.SetActive(inputDevice == this.inputDevice);
    }
}