using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleRotation : MonoBehaviour
{

    public float turnSpeed = 250;
    public bool useUnscaledTime = true;
    public bool resetOnDisable = false;

    private Vector2 origUp;

    private void Start()
    {
        origUp = transform.up;
    }

    private void OnDisable()
    {
        if (resetOnDisable)
        {
            transform.up = origUp;
        }
    }

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
