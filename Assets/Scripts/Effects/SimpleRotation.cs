using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleRotation : MonoBehaviour
{

    public float turnSpeed = 250;
    public bool useUnscaledTime = true;

    // Update is called once per frame
    void Update()
    {
        if (useUnscaledTime)
        {
            transform.Rotate(Vector3.forward * turnSpeed * Time.unscaledDeltaTime);
        }
        else
        {
            transform.Rotate(Vector3.forward * turnSpeed * Time.deltaTime);
        }
    }
}
