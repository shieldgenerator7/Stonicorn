using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CinematicCameraController : MonoBehaviour
{
    public float moveSpeed = 3;
    public List<GameObject> objectsToHide = new List<GameObject>();

    private bool active = false;
    public bool Active
    {
        get { return active; }
        set
        {
            active = value;
            //Other camera controller
            camCntr.enabled = !active;
            //Other objects
            foreach (GameObject go in objectsToHide)
            {
                go.SetActive(!active);
            }
            //Scene Loader Explorer Object
            if (active)
            {
                SceneLoader.ExplorerObject = gameObject;
            }
            else
            {
                //Reset explorer object (resets back to player)
                SceneLoader.ExplorerObject = null;
            }
        }
    }

    private Vector2 targetUp;//used to smoothly rotate the camera between gravity zones

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
            Active = !Active;
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

            //Rotation
            GravityZone gz = GravityZone.getGravityZone(transform.position);
            if (gz)
            {
                if (gz.radialGravity)
                {
                    targetUp = (transform.position - gz.transform.position).normalized;
                }
                else
                {
                    targetUp = gz.transform.up;
                }
            }
            if ((Vector2)transform.up != targetUp)
            {
                transform.up = Vector2.Lerp(transform.up, targetUp, Time.deltaTime);
            }
            //Cheats
            if (Input.GetKeyDown(KeyCode.M))
            {
                Managers.Player.transform.position = (Vector2)transform.position;
            }
            if (Input.GetKeyDown(KeyCode.H))
            {
                foreach (HiddenArea ha in FindObjectsByType<HiddenArea>(FindObjectsSortMode.None))
                {
                    Destroy(ha.gameObject);
                }
            }
        }
    }
}
