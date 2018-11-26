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

    public class IntersectionDataComparer: IComparer<IntersectionData>
    {
        public int Compare(IntersectionData interdata1, IntersectionData interdata2)
        {
            //Sort by target segment first
            if (interdata1.targetLineSegmentID == interdata2.targetLineSegmentID)
            {
                //Then check to see if first data is inside
                if (interdata1.type == IntersectionType.INSIDE)
                {
                    switch (interdata2.type)
                    {
                        case IntersectionType.ENTER:
                            return 1;
                        case IntersectionType.EXIT:
                            return -1;
                    }
                }
                //Then second data
                if (interdata2.type == IntersectionType.INSIDE)
                {
                    switch (interdata1.type)
                    {
                        case IntersectionType.ENTER:
                            return -1;
                        case IntersectionType.EXIT:
                            return 1;
                    }
                }
                //And finally by distance to target segment first point
                return (int)Mathf.Sign(interdata1.distanceToPoint - interdata2.distanceToPoint);
            }
            else
            {
                //If they're on separate line segments, sort by target line segment index
                return (int)Mathf.Sign(interdata1.targetLineSegmentID - interdata2.targetLineSegmentID);
            }
        }
    }
}
