using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

public class LineSegment
{
    public Vector2 startPos { get; private set; }
    public Vector2 endPos { get; private set; }
    private float slope;
    private float offset;

    public LineSegment(Vector2 startPos, Vector2 endPos)
    {
        this.startPos = startPos;
        this.endPos = endPos;

        float run = endPos.x - startPos.x;
        if (run == 0)
        {
            slope = float.NaN;
        }
        else
        {
            slope = (endPos.y - startPos.y) / run;
        }
        offset = (-startPos.x * slope) + startPos.y;
    }
    /// <summary>
    /// Retrieves a line segment from a path
    /// </summary>
    /// <param name="path"></param>
    /// <param name="index">The index of the first point in the line segment. EX: Index 7 is the line segment between 7 and 8</param>
    public LineSegment(Vector2[] path, int index)
        : this(
             path[index],
             path[(index + 1) % path.Length]
             )
    { }
    public LineSegment(List<Vector2> path, int index)
        : this(path.ToArray(), index) { }


    public Bounds Bounds
    {
        get
        {
            return new Bounds(
                abs((startPos + endPos) / 2),
                abs((endPos - startPos))
            );
        }
        private set { }
    }

    public float getY(float x)
    {
        //y = mx + b
        return (slope * x) + offset;
    }

    public float getX(float y)
    {
        //x = (y - b) / m
        return (y - offset) / slope;
    }

    Vector2 abs(Vector2 v)
    {
        return new Vector2(Mathf.Abs(v.x), Mathf.Abs(v.y));
    }
    public Vector2 Direction
    {
        get
        {
            return endPos - startPos;
        }
        private set { }
    }

    public bool Intersects(LineSegment l2, ref Vector2 intersection)
    {
        return LineIntersection(this.startPos, this.endPos, l2.startPos, l2.endPos, ref intersection);
    }
    /// <summary>
    /// Returns true if the two lines intersect
    /// 2018-11-21: copied from a post by Minassian: https://forum.unity.com/threads/line-intersection.17384/
    /// </summary>
    /// <param name="p1"></param>
    /// <param name="p2"></param>
    /// <param name="p3"></param>
    /// <param name="p4"></param>
    /// <param name="intersection"></param>
    /// <returns></returns>
    public static bool LineIntersection(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, ref Vector2 intersection)
    {
        float Ax, Bx, Cx, Ay, By, Cy, d, e, f, num;
        float x1lo, x1hi, y1lo, y1hi;
        Ax = p2.x - p1.x;
        Bx = p3.x - p4.x;
        // X bound box test/
        if (Ax < 0)
        {
            x1lo = p2.x; x1hi = p1.x;
        }
        else
        {
            x1hi = p2.x; x1lo = p1.x;
        }
        if (Bx > 0)
        {
            if (x1hi < p4.x || p3.x < x1lo) return false;
        }
        else
        {
            if (x1hi < p3.x || p4.x < x1lo) return false;
        }
        Ay = p2.y - p1.y;
        By = p3.y - p4.y;
        // Y bound box test//
        if (Ay < 0)
        {
            y1lo = p2.y; y1hi = p1.y;
        }
        else
        {
            y1hi = p2.y; y1lo = p1.y;
        }
        if (By > 0)
        {
            if (y1hi < p4.y || p3.y < y1lo) return false;
        }
        else
        {
            if (y1hi < p3.y || p4.y < y1lo) return false;
        }
        Cx = p1.x - p3.x;
        Cy = p1.y - p3.y;
        d = By * Cx - Bx * Cy;  // alpha numerator//
        f = Ay * Bx - Ax * By;  // both denominator//
                                // alpha tests//
        if (f > 0)
        {
            if (d < 0 || d > f) return false;
        }
        else
        {
            if (d > 0 || d < f) return false;
        }
        e = Ax * Cy - Ay * Cx;  // beta numerator//
                                // beta tests //
        if (f > 0)
        {
            if (e < 0 || e > f) return false;
        }
        else
        {
            if (e > 0 || e < f) return false;
        }
        // check if they are parallel
        if (f == 0) return false;
        // compute intersection coordinates //
        num = d * Ax; // numerator //
        intersection.x = p1.x + num / f;
        num = d * Ay;
        intersection.y = p1.y + num / f;
        return true;
    }

    public override string ToString()
    {
        return "Line Segment: " + startPos + " -> " + endPos
            + ", Bounds: " + Bounds;
    }
}
