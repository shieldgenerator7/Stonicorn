using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityAccepter : SavableMonoBehaviour
{
    //used for objects that need to know their gravity direction

    //Settings
    public bool usesSideVector = false;//whether or not this use case needs to use the side vector

    public float gravityScale = 1;

    private Vector2 gravityVector;
    public Vector2 Gravity
    {
        get
        {
            if (gravityVector == Vector2.zero)
            {
                return prevGravityVector;
            }
            return gravityVector;
        }
        private set
        {
            if (value == prevGravityVector)
            {
                gravityVector = prevGravityVector;
                if (usesSideVector)
                {
                    sideVector = prevSideVector;
                }
            }
            // 2017-06-02: moved here from PlayerController.setGravityVector(.)
            else if (value.x != gravityVector.x || value.y != gravityVector.y)
            {
                gravityVector = value;
                if (usesSideVector)
                {
                    //v = P2 - P1    //2016-01-10: copied from an answer by cjdev: http://answers.unity3d.com/questions/564166/how-to-find-perpendicular-line-in-2d.html
                    //P3 = (-v.y, v.x) / Sqrt(v.x ^ 2 + v.y ^ 2) * h
                    sideVector = new Vector3(-gravityVector.y, gravityVector.x) / Mathf.Sqrt(gravityVector.x * gravityVector.x + gravityVector.y * gravityVector.y);
                }
            }
        }
    }
    private Vector2 sideVector;
    public Vector2 SideVector
    {
        get
        {
            if (sideVector == Vector2.zero)
            {
                return prevSideVector;
            }
            return sideVector;
        }
        private set { sideVector = value; }
    }
    [SerializeField]
    private bool acceptsGravity = true;
    public bool AcceptsGravity
    {
        get { return acceptsGravity; }
        set { acceptsGravity = value; }
    }

    public void addGravity(Vector2 newGravity)
    {
        Gravity = gravityVector + newGravity;
    }
    Vector2 prevGravityVector;
    Vector2 prevSideVector;
    private void LateUpdate()
    {
        if (prevGravityVector != gravityVector
            && gravityVector != Vector2.zero)
        {
            if (onGravityChanged != null)
            {
                onGravityChanged(gravityVector);
            }
            prevGravityVector = gravityVector;
        }
        gravityVector = Vector2.zero;
        if (usesSideVector)
        {
            prevSideVector = sideVector;
            sideVector = Vector2.zero;
        }
    }

    public delegate void OnGravityChanged(Vector2 newGravity);
    public OnGravityChanged onGravityChanged;

    public override SavableObject getSavableObject()
    {
        return new SavableObject(this,
            "acceptsGravity", AcceptsGravity,
            "gravityScale", gravityScale
            );
    }

    public override void acceptSavableObject(SavableObject savObj)
    {
        AcceptsGravity = (bool)savObj.data["acceptsGravity"];
        gravityScale = (float)savObj.data["gravityScale"];
    }
}
