using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntersectionData
{
    public Vector2 intersectionPoint;
    public int targetLineSegmentID;
    public int stencilLineSegmentID;
    public bool segmentIntersection = true;//true if the target line segment intersects a stencil line segment
    public bool startsInStencil = false;//true if the segment start point is in the stencil
    public bool endsInStencil = false;//true if the segment end point is in the stencil
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
        if (type == IntersectionType.INSIDE)
        {
            segmentIntersection = false;
            startsInStencil = true;
            endsInStencil = true;
        }
        Debug.Log("Added InterData point: " + this);
    }
    public IntersectionData(Vector2 intersection, int targetLS, int stencilLS, bool segmentIntersection, bool startsInStencil, bool endsInStencil)
    {
        this.intersectionPoint = intersection;
        this.targetLineSegmentID = targetLS;
        this.stencilLineSegmentID = stencilLS;
        this.segmentIntersection = segmentIntersection;
        this.startsInStencil = startsInStencil;
        this.endsInStencil = endsInStencil;
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
