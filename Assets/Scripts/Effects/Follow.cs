using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Used to make one object follow another exactly
/// Made for the purpose of keeping Merky's scout colliders with him
/// without making them disrupt gameplay
/// </summary>
public class Follow : MonoBehaviour
{

    public GameObject followObject;
    public string followObjectTag;
    public bool orientToCamera = false;
    public bool shakeOnStop = true;

    public float bounceBackSpeed = 2;
    private Vector2 prevVelocity;
    private Rigidbody2D rb2dParent;
    private Vector2 offset = Vector2.zero;

    private void Awake()
    {
        if (followObject == null)
        {
            followObject = findFollowObject();
        }
        else
        {
            followObjectTag = followObject.tag;
        }
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
        if (orientToCamera)
        {
            //Camera delegate
            Managers.Camera.onRotated +=
                (up) => transform.up = up;
        }
        //Rewind delegates
        Managers.Rewind.onRewindStarted +=
            (gss, gs) => this.enabled = false;
        Managers.Rewind.onRewindState +=
            (gss, gs) => updateTransform(false, orientToCamera);
        Managers.Rewind.onRewindFinished +=
            (gss, gs) => this.enabled = true;
    }

    private void LateUpdate()
    {
        if (shakeOnStop)
        {
            //Check if needs to shake
            if (rb2dParent.velocity != prevVelocity
                && rb2dParent.velocity.magnitude < 0.1f)
            {
                offset += (prevVelocity - rb2dParent.velocity) * 0.1f;
            }
            prevVelocity = rb2dParent.velocity;
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

    GameObject findFollowObject()
    {
        return GameObject.FindGameObjectWithTag(followObjectTag);
    }
}
