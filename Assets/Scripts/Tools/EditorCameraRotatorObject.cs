using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditorCameraRotatorObject : MonoBehaviour
{
    [Range(0, 360)]
    public float rotZ = 0;
    public bool autoRotate = true;

    public void toggle()
    {
        if (rotZ != 0)
        {
            autoRotate = false;
            rotZ = 0;
        }
        else
        {
            autoRotate = true;
        }
    }
}
