using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialSwitcher : MonoBehaviour
{
    
    public InputDeviceMethod inputDevice;

    // Start is called before the first frame update
    void Start()
    {
        GameManager.GestureManager.onInputDeviceSwitched += updateLastUsedInputDevice;
    }

    void updateLastUsedInputDevice(InputDeviceMethod inputDevice)
    {
        gameObject.SetActive(inputDevice == this.inputDevice);
    }
}
