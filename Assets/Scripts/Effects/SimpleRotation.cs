using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleRotation : MonoBehaviour {

    public float turnSpeed = 250;

	// Update is called once per frame
	void Update () {
        transform.Rotate(Vector3.forward * turnSpeed * Time.deltaTime);
	}
}
