using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Polygon
{
    public List<Vector2> points { get; private set; }
    public List<LineSegment> segments { get; private set; }

    public Bounds bounds { get; private set; }

    public Polygon(List<Vector2> points = null)
    {
        if (points == null)
        {
            points = new List<Vector2>();
            segments = new List<LineSegment>();
            bounds = new Bounds();
        }
        else
        {
            //Calculate segments
            for (int i = 0; i < points.Count; i++)
            {
                segments.Add(new LineSegment(points, i));
            }
            //Set bounds
            float minX = points.Min(v => v.x);
            float maxX = points.Max(v => v.x);
            float minY = points.Min(v => v.y);
            float maxY = points.Max(v => v.y);
            bounds = new Bounds(
                new Vector2(
                    (minX + maxX) / 2,
                    (minY + maxY) / 2
                    ),
                new Vector2(
                    maxX - minX,
                    maxY - minY
                    )
                );
        }
    }

    public bool containsPoint(Vector2 point)
    {
        //Check bounds first
        //If the point is not inside the bounding rectangle,
        if (!bounds.Contains(point))
        {
            //the point is not contained within the polygon
            return false;
        }
        //Check raycast
        //Check each line segment's intersection with an infinite horizontal ray
        //that comes from the left side and ends at the given point
        //if the intersection count is odd, the point is contained
        //if the intersection count is even, the point is not contained
        int intersectCount = 0;
        foreach(LineSegment segment in segments)
        {
            Vector2 p1 = segment.startPos;
            Vector2 p2 = segment.endPos;
            bool p1Above = p1.y > point.y;
            bool p2Above = p2.y > point.y;
            //if p1 and p2 are on the same side of the ray,
            if (p1Above == p2Above)
            {
                //there is no intersection
                continue;
            }
            bool p1Left = p1.x < point.x;
            bool p2Left = p2.x < point.x;
            //if p1 and p2 are both right of the point,
            if (!p1Left && !p2Left)
            {
                //there is no intersection
                continue;
            }
            //If both are left of it,
            if (p1Left && p2Left)
            {
                //then there is definitely an intersection
                intersectCount++;
                continue;
            }
            //Else if one is left and the other is right,
            //they might not intersect
            //Get the line segment's formula
            //Find the x of the line segment at the point's y
            float x = segment.getX(point.y);
            //If the x is left of the point,
            if (x < point.x)
            {
                //there is an intersection
                intersectCount++;
                continue;
            }
            //Else there is no intersection
        }
        //Check intersection count
        return intersectCount % 2 == 0;
    }
}
