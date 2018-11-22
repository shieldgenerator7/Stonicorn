using System.Collections.Generic;
using UnityEngine;

public class LineSegment
{
    public Vector2 startPos;
    public Vector2 endPos;

    public LineSegment(Vector2 startPos, Vector2 endPos)
    {
        this.startPos = startPos;
        this.endPos = endPos;
    }
    /// <summary>
    /// Retrieves a line segment from a path
    /// </summary>
    /// <param name="path"></param>
    /// <param name="index">The index of the first point in the line segment. EX: Index 7 is the line segment between 7 and 8</param>
    public LineSegment(Vector2[] path, int index, Vector2 center)
    {
        int i2 = (index + 1) % path.Length;
        this.startPos = path[index] + center;
        this.endPos = path[i2] + center;
    }
    public LineSegment(List<Vector2> path, int index, Vector2 center)
        : this(path.ToArray(), index, center) { }

    public static LineSegment rayToLineSegment(Ray ray)
    {
        return rayToLineSegment(ray.origin, ray.direction);
    }
    public static LineSegment rayToLineSegment(Vector2 startPos, Vector2 dir)
    {
        return new LineSegment(startPos, startPos + dir);
    }

    public Ray ToRay()
    {
        return new Ray(this.startPos, this.endPos - this.startPos);
    }

    public Bounds Bounds
    {
        get
        {
            return new Bounds(
                (startPos + endPos) / 2,
                (endPos - startPos)
            );
        }
        private set { }
    }
    public Vector2 Direction
    {
        get
        {
            return endPos - startPos;
        }
        private set { }
    }
}
