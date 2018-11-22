using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntersectionData
{
    public Vector2 intersectionPoint;
    public int targetLineSegmentID;
    public int stencilLineSegmentID;
    public enum IntersectionType
    {
        ENTER,//when the line enters the stencil
        EXIT,//when the line exits the stencil
        INSIDE,//when the line segment is entirely within the stencil
        OUTSIDE//not used bc if it's outside no IntersectionData object will be created
    }
    public IntersectionType type = IntersectionType.OUTSIDE;

    public IntersectionData(Vector2 intersection, int targetLS, int stencilLS, IntersectionType type)
    {
        this.intersectionPoint = intersection;
        this.targetLineSegmentID = targetLS;
        this.stencilLineSegmentID = stencilLS;
        this.type = type;
        Debug.Log("Added InterData point: " + this);
    }
    public IntersectionData(Vector2 intersection, int targetLS, int stencilLS)
    {
        this.intersectionPoint = intersection;
        this.targetLineSegmentID = targetLS;
        this.stencilLineSegmentID = stencilLS;
        Debug.Log("Added InterData point: " + this);
    }

    public override string ToString()
    {
        return "tID: " + targetLineSegmentID + ", "
            + "sID: " + stencilLineSegmentID + ", "
            + "type: " + type + ", "
            + "inter: " + intersectionPoint;
    }
}
