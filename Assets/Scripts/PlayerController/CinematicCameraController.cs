using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CinematicCameraController : MonoBehaviour
{
    public float moveSpeed = 3;

    private bool active = false;
    private Camera cam;
    private CameraController camCntr;

    // Start is called before the first frame update
    void Start()
    {
        cam = GetComponent<Camera>();
        camCntr = GetComponent<CameraController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            active = !active;
            camCntr.enabled = !active;
        }
        if (active)
        {
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");
            transform.position += ((cam.transform.right * horizontal) + (cam.transform.up * vertical)) * moveSpeed * Time.deltaTime;
            if (Input.GetKeyDown(KeyCode.LeftBracket))
            {
                if (moveSpeed > 1)
                {
                    moveSpeed = Mathf.Floor(moveSpeed - 1);
                }
                else
                {
                    moveSpeed = moveSpeed / 2;
                }
            }
            if (Input.GetKeyDown(KeyCode.RightBracket))
            {
                if (moveSpeed > 1)
                {
                    moveSpeed = Mathf.Floor(moveSpeed + 1);
                }
                else
                {
                    moveSpeed = moveSpeed * 2;
                }
            }
            if (Input.GetKey(KeyCode.Minus))//Zoom out
            {
                float factor = (cam.fieldOfView < 1 * 11) ? 0.1f : 1;
                cam.fieldOfView += Time.deltaTime * factor;
            }
            if (Input.GetKey(KeyCode.Equals))//Zoom in
            {
                float factor = (cam.fieldOfView < 1 * 11) ? 0.1f : 1;
                cam.fieldOfView -= Time.deltaTime * factor;
            }
        }
    }
}
