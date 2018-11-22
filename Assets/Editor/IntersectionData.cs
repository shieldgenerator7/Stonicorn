using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntersectionData
{
    public Vector2 intersectionPoint;
    public int targetLineSegmentID;
    public int stencilLineSegmentID;
    public bool segmentIntersection;//true if the target line segment intersects a stencil line segment
    public bool startsInStencil;//true if the segment start point is in the stencil
    public bool endsInStencil;//true if the segment end point is in the stencil

    public IntersectionData(Vector2 intersection, int targetLS, int stencilLS, bool segmentIntersection = true, bool startsInStencil = false, bool endsInStencil = false)
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
            + "(segment: " + segmentIntersection + ") "
            + "(start: " + startsInStencil + ") "
            + "(end: " + endsInStencil + ") "
            + "inter: " + intersectionPoint + ", ";
    }
}
