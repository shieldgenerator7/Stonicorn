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
    public float distanceToPoint = float.MaxValue;//how far away it is from its target segment's first point
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
    public IntersectionData(Vector2 intersection, int targetLS, int stencilLS, bool segmentIntersection, bool startsInStencil, bool endsInStencil, float distanceToPoint)
    {
        this.intersectionPoint = intersection;
        this.targetLineSegmentID = targetLS;
        this.stencilLineSegmentID = stencilLS;
        this.segmentIntersection = segmentIntersection;
        this.startsInStencil = startsInStencil;
        this.endsInStencil = endsInStencil;
        this.distanceToPoint = distanceToPoint;
        Debug.Log("Added InterData point: " + this);
    }

    public override string ToString()
    {
        return "tID: " + targetLineSegmentID + ", "
            + "sID: " + stencilLineSegmentID + ", "
            + "type: " + type + ", "
            + "inter: " + intersectionPoint;
    }

    public static void reverseDataInList(ref List<IntersectionData> intersectionData, int start, int end, Vector2 point, List<Vector2> points)
    {
        if (start < end
                && start >= 0 && start < intersectionData.Count)
        {
            float firstDistSqr = (
                intersectionData[start].intersectionPoint
                - point
                ).sqrMagnitude;
            float lastDistSqr = (
                intersectionData[end].intersectionPoint
                - point
                ).sqrMagnitude;
            //If the first data point is further from the start of the segment than the last one,
            if (firstDistSqr > lastDistSqr)
            {
                //reverse the entries
                for (int i = start, j = end; i < end; i++, j++)
                {
                    IntersectionData tempData = intersectionData[start];
                    intersectionData[start] = intersectionData[end];
                    intersectionData[end] = tempData;
                }
            }
        }
    }

    public static void printDataList(List<IntersectionData> intersectionData, List<Vector2> points)
    {
        //Print out the data order to confirm it's correct (debug)
        foreach (IntersectionData data in intersectionData)
        {
            float dist = (
                    data.intersectionPoint
                    - points[data.targetLineSegmentID]
                    ).magnitude;
            Debug.Log("Data: " + data + "; Distance: " + dist);
        }
    }
}
