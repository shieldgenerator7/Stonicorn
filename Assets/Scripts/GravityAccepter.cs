using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityAccepter : MonoBehaviour {
    //used for objects that need to know their gravity direction

    private Vector2 gravityVector;
    public Vector2 Gravity
    {
        get {
            if (gravityVector == Vector2.zero)
            {
                return prevGravityVector;
            }
            return gravityVector; }
        private set
        {
            if (value == prevGravityVector)
            {
                gravityVector = prevGravityVector;
                sideVector = prevSideVector;
            }
            // 2017-06-02: moved here from PlayerController.setGravityVector(.)
            else if (value.x != gravityVector.x || value.y != gravityVector.y)
            {
                gravityVector = value;
                //v = P2 - P1    //2016-01-10: copied from an answer by cjdev: http://answers.unity3d.com/questions/564166/how-to-find-perpendicular-line-in-2d.html
                //P3 = (-v.y, v.x) / Sqrt(v.x ^ 2 + v.y ^ 2) * h
                sideVector = new Vector3(-gravityVector.y, gravityVector.x) / Mathf.Sqrt(gravityVector.x * gravityVector.x + gravityVector.y * gravityVector.y);
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
        prevGravityVector = gravityVector;
        prevSideVector = sideVector;
        gravityVector = Vector2.zero;
        sideVector = Vector2.zero;
    }
}
