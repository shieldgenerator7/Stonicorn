using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PolygonCollider2DWorker))]
public class PolygonCollider2DWorkerEditor : Editor
{

    PolygonCollider2DWorker pc2dw;
    PolygonCollider2D stencil;

    private void OnEnable()
    {
        pc2dw = (PolygonCollider2DWorker)target;
        stencil = pc2dw.GetComponent<PolygonCollider2D>();
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        //GUI.enabled = !EditorApplication.isPlaying;
        if (GUILayout.Button("Cut PolygonCollider2D"))
        {
            //if (EditorApplication.isPlaying)
            //{
            //    throw new UnityException("You must be in Edit Mode to use this function!");
            //}
            Debug.Log("Will now cut the pc2d " + pc2dw.editTarget.name);
            cutCollider(pc2dw.editTarget, stencil);
        }
    }

    public static void cutCollider(PolygonCollider2D pc2d, PolygonCollider2D stencil)
    {
        Vector2 stud = pc2d.transform.position;
        Vector2 pc2dScale = pc2d.transform.localScale;
        List<Vector2> points = new List<Vector2>(pc2d.GetPath(0));
        Vector2 stencilStud = stencil.transform.position;
        Vector2 stencilScale = stencil.transform.localScale;
        Vector2[] stencilPoints = stencil.GetPath(0);

        bool changed = true;
        while (changed)
        {
            changed = false;
            //Gather overlap info
            List<IntersectionData> intersectionData = new List<IntersectionData>();
            for (int i = 0; i < points.Count; i++)
            {
                int i2 = (i + 1) % points.Count;

                //Line Checking
                LineSegment targetLine = new LineSegment(points, i, stud, pc2dScale);
                //Check to see if the bounds overlap
                if (stencil.bounds.Intersects(targetLine.Bounds))
                {
                    bool startInStencil = stencil.OverlapPoint(targetLine.startPos);
                    bool endInStencil = stencil.OverlapPoint(targetLine.endPos);
                    //Check which stencil edges intersect the line segment
                    bool intersectsSegment = false;
                    for (int j = 0; j < stencilPoints.Length; j++)
                    {
                        LineSegment stencilLine = new LineSegment(stencilPoints, j, stencilStud, stencilScale);
                        Vector2 intersection = Vector2.zero;
                        bool intersects = LineIntersection(targetLine, stencilLine, ref intersection);
                        //If it intersects,
                        if (intersects)
                        {
                            //Record a data point
                            intersectsSegment = true;
                            IntersectionData interdata = new IntersectionData(intersection, i, j, true, startInStencil, endInStencil);
                            intersectionData.Add(interdata);
                        }
                    }
                    //If no line segment intersections were found,
                    if (!intersectsSegment)
                    {
                        //but one or more end points are in the stencil,
                        if (startInStencil || endInStencil)
                        {
                            //Make an intersection data point anyway, with slightly different arguments
                            IntersectionData interdata = new IntersectionData(Vector2.zero, i, -1, false, startInStencil, endInStencil);
                            intersectionData.Add(interdata);
                        }
                    }
                    //else,
                    else
                    {
                        //do nothing because the bounds lied about the line segment and stencil colliding
                        //don't worry, it's a known thing that can happen:
                        //bounds checking is quick but liable to give false positives
                    }
                }

            }

            //
            //Start cutting
            //

            //Remove line segments engulfed by stencil
            if (!changed)
            {
                for (int i = intersectionData.Count - 1; i >= 0; i--)
                {
                    IntersectionData interdata = intersectionData[i];
                    //If the line segment is completely in the stencil,
                    if (interdata.startsInStencil
                        && interdata.endsInStencil
                        && !interdata.segmentIntersection)
                    {
                        int i0 = ((interdata.targetLineSegmentID - 1) + points.Count) % points.Count;
                        //and the point before is also in the stencil,
                        if (stencil.OverlapPoint(points[i0]))
                        {
                            //remove the point that starts the segment
                            points.RemoveAt(interdata.targetLineSegmentID);
                            changed = true;
                        }
                    }
                }
            }
        }

        //
        // Finish up
        //
        pc2d.SetPath(0, points.ToArray());
    }

    public static bool LineIntersection(LineSegment l1, LineSegment l2, ref Vector2 intersection)
    {
        return LineIntersection(l1.startPos, l1.Direction, l2.startPos, l2.Direction, ref intersection);
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
        }
    }
}
