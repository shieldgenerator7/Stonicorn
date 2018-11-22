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

            //Replace line segments inside the stencil
            if (!changed)
            {
                int count = intersectionData.Count;
                //Search for start of vein of changes
                int veinStart = -1;//the id of the segment where the vein starts
                int veinEnd = -1;//the id of the segment where the vein ends
                int dataStart = -1;//the index of the interdata where the vein starts
                int dataEnd = -1;//the index of the interdata where the vein ends
                for (int iData = 0; iData < count; iData++)
                {
                    IntersectionData interdata = intersectionData[iData];
                    //if this segment starts outside and ends inside the stencil
                    if (!interdata.startsInStencil && interdata.endsInStencil)
                    {
                        IntersectionData nextData = intersectionData[nextIndex(iData, count)];
                        //and if this segment only traverses a stencil segment once,
                        if (interdata.targetLineSegmentID != nextData.targetLineSegmentID)
                        {
                            //then it's a vein start
                            veinStart = interdata.targetLineSegmentID;
                            dataStart = iData;
                            break;
                        }
                    }
                }
                //Find the end of the vein of changes
                for (int iData = dataStart; iData < dataStart + count; iData++)
                {
                    IntersectionData interdata = intersectionData[iData % count];
                    if (interdata.startsInStencil && interdata.segmentIntersection)
                    {
                        veinEnd = interdata.targetLineSegmentID;
                        dataEnd = iData % count;
                        break;
                    }
                }
                if (veinStart > -1 && veinEnd > -1)
                {
                    int startIndex = intersectionData[dataStart].stencilLineSegmentID;
                    int endIndex = intersectionData[dataEnd].stencilLineSegmentID;
                    int stencilCount = stencilPoints.Length;
                    Vector2[] newPath = new Vector2[(startIndex - endIndex + stencilCount) % stencilCount + 2];
                    //Get the new path data
                    newPath[0] = intersectionData[dataStart].intersectionPoint;
                    int writeIndex = 1;
                    for (int i = startIndex; i > endIndex; i--, writeIndex++)
                    {
                        newPath[writeIndex] = stencilPoints[i];
                    }
                    newPath[newPath.Length - 1] = intersectionData[dataEnd].intersectionPoint;
                    //Replace vein with stencil path
                    int removeCount = (veinEnd - veinStart + points.Count) % points.Count;
                    replacePoints(ref points, newPath, veinStart, removeCount);
                }
            }
        }

        //
        // Finish up
        //
        pc2d.SetPath(0, points.ToArray());
    }

    public static int nextIndex(int index, int listCount, int delta = 1)
    {
        return (index + delta + listCount) % listCount;
    }

    public static bool LineIntersection(LineSegment l1, LineSegment l2, ref Vector2 intersection)
    {
        return LineIntersection(l1.startPos, l1.endPos, l2.startPos, l2.endPos, ref intersection);
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

    static void replacePoints(ref List<Vector2> points, Vector2[] newVectors, int index, int removeCount)
    {
        insertPoints(ref points, newVectors, index);
        index += newVectors.Length;
        removePoints(ref points, index, removeCount);
    }
    static void insertPoints(ref List<Vector2> points, Vector2[] vectors, int index)
    {
        points.InsertRange(index, vectors);
    }
    static void removePoints(ref List<Vector2> points, int index, int count)
    {
        for (int i = 0; i < count; i++)
        {
            if (index >= points.Count)
            {
                index = 0;
            }
            points.RemoveAt(index);
        }
    }
}
