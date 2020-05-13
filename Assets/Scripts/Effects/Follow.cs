using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Used to make one object follow another exactly
/// Made for the purpose of keeping Merky's scout colliders with him
/// without making them disrupt gameplay
/// </summary>
public class Follow : MonoBehaviour {

    public GameObject followObject;
    public string followObjectTag;

    private void Start()
    {
        if (followObject == null)
        {
            followObject = findFollowObject();
        }
        else{
            followObjectTag = followObject.tag;
        }
    }

    private void LateUpdate()
    {
        if (followObject == null)
        {
            followObject = findFollowObject();
        }
        transform.position = followObject.transform.position;
        transform.rotation = followObject.transform.rotation;
        transform.localScale = followObject.transform.localScale;
    }

    GameObject findFollowObject()
    {
        return GameObject.FindGameObjectWithTag(followObjectTag);
    }
}
