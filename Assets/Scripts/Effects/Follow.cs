using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Used to make one object follow another exactly
/// Made for the purpose of keeping Merky's scout colliders with him
/// without making them disrupt gameplay
/// </summary>
public class Follow : MonoBehaviour
{

    public string followName = "";
    public GameObject followObject;
    public Vector2 positionOffset = Vector2.zero;
    public float scaleFactor = 1f;
    public bool orientToCamera = false;
    public bool scaleToCameraZoomLevel = false;
    public bool shakeOnStop = true;
    [SerializeField]
    private bool inScreenSpace = false;
    [AutoInitialize(AllowUnfound =true), SerializeField]
    private RectTransform rectTransform;

    public float bounceBackSpeed = 2;
    private Vector2 prevVelocity;
    private Rigidbody2D rb2dParent;
    private Vector2 offset = Vector2.zero;

    private void Awake()
    {
        //DO NOT AutoInitialize OR ISetupable this Awake(),
        //it's very likely that this class and its follow object will NOT be in the same scene!

        //Follow Object
        if (!followObject)
        {
            followObject = GameObject.Find(followName);
        }
        if (!followObject)
        {
            Debug.LogError($"Can't find followObject {followName}");
            this.enabled = false;
            return;
        }

        //Shake on Stop
        if (shakeOnStop)
        {
            rb2dParent = followObject.GetComponent<Rigidbody2D>();
            if (!rb2dParent)
            {
                Debug.LogError("Follow has shakeOnStop, " +
                    "but its follow object does not have a RigidBody2D!",
                    gameObject
                    );
            }
        }

        //Orient to Camera
        if (orientToCamera)
        {
            //Camera delegate
            Managers.Camera.onRotated -= orientToCameraDel;
            Managers.Camera.onRotated += orientToCameraDel;
        }

        //Rewind delegates
        Managers.Rewind.onRewindStarted += rewindStarted;
        Managers.Rewind.onRewindState += rewindState;
        Managers.Rewind.onRewindFinished += rewindFinished;
    }

    private void OnDestroy()
    {
        Managers.Camera.onRotated -= orientToCameraDel;
        Managers.Rewind.onRewindStarted -= rewindStarted;
        Managers.Rewind.onRewindState -= rewindState;
        Managers.Rewind.onRewindFinished -= rewindFinished;
    }

    void orientToCameraDel(Vector2 up)
        => transform.up = up;

    void rewindStarted(int gs)
        => this.enabled = false;
    void rewindState(int gs)
        => updateTransform(false, orientToCamera);
    void rewindFinished(int gs)
        => this.enabled = true;

    private void LateUpdate()
    {
        if (shakeOnStop)
        {
            //Check if needs to shake
            if (rb2dParent.linearVelocity != prevVelocity
                && !rb2dParent.isMoving())
            {
                offset += (prevVelocity - rb2dParent.linearVelocity) * 0.1f;
            }
            prevVelocity = rb2dParent.linearVelocity;
            //Update transform
            updateTransform(true, orientToCamera);
            //Decrease offset
            offset = Vector2.Lerp(
                offset,
                Vector2.zero,
                bounceBackSpeed * Time.deltaTime
                );
        }
        else
        {
            //Update transform
            updateTransform(false, orientToCamera);
        }
    }

    void updateTransform(bool useOffset, bool useCameraUp)
    {
        Transform tf = (useCameraUp) ? Managers.Camera.transform : followObject.transform;
        Vector3 startOffsetTransformed = tf.TransformDirection(positionOffset);
        float camScale = Managers.Camera.ZoomLevel / Managers.Camera.toZoomLevel(CameraController.CameraScalePoints.DEFAULT);
        //Position
        Vector2 position = followObject.transform.position
            + ((useOffset) ? (Vector3)offset : Vector3.zero)
            + startOffsetTransformed * ((scaleToCameraZoomLevel)?camScale:1);
        if (!inScreenSpace)
        {
            transform.position = position;
        }
        else
        {
            rectTransform.position = Utility.WorldToScreenPoint(position);
        }
        //Rotation
        if (!inScreenSpace)
        {
            transform.up = tf.up;
        }
        else
        {
            Vector2 upPos = position + (Vector2)tf.up;
            rectTransform.up = Utility.WorldToScreenPoint(upPos) - (Vector2)rectTransform.position;
        }
        //Scale
        if (!inScreenSpace) { 
        if (scaleToCameraZoomLevel) {
            transform.localScale = Vector3.one * scaleFactor * camScale;
        }
        else {
            transform.localScale = followObject.transform.localScale * scaleFactor;
        }
        }
    }
}
