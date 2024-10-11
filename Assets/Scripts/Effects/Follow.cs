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
    public bool orientToCamera = false;
    public bool shakeOnStop = true;

    public float bounceBackSpeed = 2;
    private Vector2 prevVelocity;
    private Rigidbody2D rb2dParent;
    private Vector2 offset = Vector2.zero;

    private void Awake()
    {
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
            Managers.Camera.onRotated +=
                (up) => transform.up = up;
        }

        //Rewind delegates
        Managers.Rewind.onRewindStarted += rewindStarted;
        Managers.Rewind.onRewindState += rewindState;
        Managers.Rewind.onRewindFinished += rewindFinished;
    }

    private void OnDestroy()
    {
        Managers.Rewind.onRewindStarted -= rewindStarted;
        Managers.Rewind.onRewindState -= rewindState;
        Managers.Rewind.onRewindFinished -= rewindFinished;
    }

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
        //Position
        transform.position = followObject.transform.position
            + ((useOffset) ? (Vector3)offset : Vector3.zero);
        //Rotation
        if (!useCameraUp)
        {
            transform.up = followObject.transform.up;
        }
        //Scale
        transform.localScale = followObject.transform.localScale;
    }
}
