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
        GUI.enabled = !EditorApplication.isPlaying;
        if (GUILayout.Button("Cut PolygonCollider2D"))
        {
            if (EditorApplication.isPlaying)
            {
                throw new UnityException("You must be in Edit Mode to use this function!");
            }
            Debug.Log("Will now cut the pc2d " + pc2dw.editTarget.name);
            cutCollider(pc2dw.editTarget, stencil);
        }
    }

    public static void cutCollider(PolygonCollider2D pc2d, PolygonCollider2D stencil)
    {
        Vector2 stud = pc2d.transform.position;
        Vector2 pc2dScale = pc2d.transform.localScale;
        List<Vector2> points = new List<Vector2>(pc2d.GetPath(0));
        convertPathToWorldSpace(ref points, pc2d.transform);
        Vector2 stencilStud = stencil.transform.position;
        Vector2 stencilScale = stencil.transform.localScale;
        List<Vector2> stencilPoints = new List<Vector2>(stencil.GetPath(0));
        convertPathToWorldSpace(ref stencilPoints, stencil.transform);

        //Show paths (for debugging)
        //showPath(points);
        //showPath(stencilPoints);

        //Gather overlap info
        List<IntersectionData> intersectionData = new List<IntersectionData>();
        for (int i = 0; i < points.Count; i++)
        {
            int i2 = (i + 1) % points.Count;

            //Line Checking
            LineSegment targetLine = new LineSegment(points, i);
            //Check to see if the bounds overlap
            if (stencil.bounds.Intersects(targetLine.Bounds))
            {
                bool startInStencil = stencil.OverlapPoint(targetLine.startPos);
                bool endInStencil = stencil.OverlapPoint(targetLine.endPos);
                //Check which stencil edges intersect the line segment
                bool intersectsSegment = false;
                for (int j = 0; j < stencilPoints.Count; j++)
                {
                    LineSegment stencilLine = new LineSegment(stencilPoints, j);
                    Vector2 intersection = Vector2.zero;
                    bool intersects = LineIntersection(targetLine, stencilLine, ref intersection);
                    //If it intersects,
                    if (intersects)
                    {
                        //Record a data point
                        intersectsSegment = true;
                        IntersectionData interdata = new IntersectionData(intersection, i, j, intersects, startInStencil, endInStencil);
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
                        IntersectionData interdata = new IntersectionData(Vector2.zero, i, -1, IntersectionData.IntersectionType.INSIDE);
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
        // Refine intersection data entries
        //

        //Correct reversed data entries
        int lastPoint = 0;//the index of the point from the last interdata
        int streakFirstIndex = 0;//if there's a streak of data with same line segment, this stores the index of the first data that has it
        int streakEndIndex = 0;//the last data index in the streak
        bool listChanged = true;
        while (listChanged)
        {
            listChanged = false;
            for (int i = 0; i < intersectionData.Count; i++)
            {
                IntersectionData interdata = intersectionData[i];
                //If this data's line segment is not the last data's segment,
                if (lastPoint != interdata.targetLineSegmentID)
                {
                    //Check last streak
                    Vector2 point = points[lastPoint];
                    IntersectionData.reverseDataInList(ref intersectionData, streakFirstIndex, streakEndIndex, point, points);
                    //Start next streak
                    lastPoint = interdata.targetLineSegmentID;
                    streakFirstIndex = i;
                    streakEndIndex = i;
                }
                else
                {
                    streakEndIndex = i;
                }
            }

        }
        //Set the intersection type of the data
        int side = 0;//0 =not set, 1 =inside, -1 =outside
        foreach (IntersectionData interdata in intersectionData)
        {
            if (side == 0)
            {
                side = (interdata.startsInStencil) ? 1 : -1;
            }
            if (interdata.segmentIntersection)
            {
                side *= -1;
                interdata.type = (side > 0) ? IntersectionData.IntersectionType.ENTER : IntersectionData.IntersectionType.EXIT;
            }
            else
            {
                interdata.type = (side > 0) ? IntersectionData.IntersectionType.INSIDE : IntersectionData.IntersectionType.OUTSIDE;
            }
        }

        //
        //Start cutting
        //

        //Replace line segments inside the stencil
        int dataCount = intersectionData.Count;
        //Search for start of vein of changes
        List<Vein> veins = new List<Vein>();
        for (int iData = 0; iData < dataCount; iData++)
        {
            IntersectionData interdata = intersectionData[iData];
            //if this segment enters the stencil at this data point,
            if (interdata.type == IntersectionData.IntersectionType.ENTER)
            {
                //then it's a vein start
                Vein vein = new Vein(iData, interdata, intersectionData);
                veins.Add(vein);
            }
        }
        //Process found veins
        if (veins.Count == 1)
        {
            Vector2[] newPath = veins[0].getStencilPath(stencilPoints);
            //Replace vein with stencil path
            int removeCount = veins[0].getRemoveCount(points.Count);
            replacePoints(ref points, newPath, veins[0].VeinStart + 1, removeCount);
        }
        else
        {
            //Process all the veins
            IndexOffset.IndexOffsetContainer offsets = new IndexOffset.IndexOffsetContainer(points.Count);
            for (int i = 0; i < veins.Count; i++)
            {
                Vein vein = veins[i];
                //Update vein with new offsets
                vein.updateIndexes(offsets);
                //Check next vein
                bool slices = false;
                if (i < veins.Count - 1)
                {
                    Vein vein2 = veins[i + 1];
                    vein.updateIndexes(offsets);
                    slices = vein.formsSlice(vein2, stencilPoints.Count);
                    Debug.Log("slices: " + slices);
                }
                if (!slices)
                {
                    //Replace vein with stencil path
                    Vector2[] newPath = vein.getStencilPath(stencilPoints);
                    int removeCount = vein.getRemoveCount(points.Count);
                    replacePoints(ref points, newPath, vein.VeinStart + 1, removeCount);
                    //Add offset to the collection
                    IndexOffset offset = new IndexOffset(vein.VeinStart, newPath.Length - removeCount);
                    offsets.Add(offset);
                }
            }
        }

        //
        // Finish up
        //
        convertPathToLocalSpace(ref points, pc2d.transform);
        pc2d.SetPath(0, points.ToArray());
    }
    static void convertPathToWorldSpace(ref List<Vector2> points, Transform t)
    {
        for (int i = 0; i < points.Count; i++)
        {
            Vector2 v = points[i];
            v = t.TransformPoint(v);
            points[i] = v;
        }
    }
    static void convertPathToWorldSpace(ref List<Vector2> points, Vector2 center, Vector2 scale)
    {
        for (int i = 0; i < points.Count; i++)
        {
            Vector2 v = points[i];
            v = (v * scale) + center;
            points[i] = v;
        }
    }
    static void convertPathToLocalSpace(ref List<Vector2> points, Transform t)
    {
        for (int i = 0; i < points.Count; i++)
        {
            Vector2 v = points[i];
            v = t.InverseTransformPoint(v);
            points[i] = v;
        }
    }
    static void convertPathToLocalSpace(ref List<Vector2> points, Vector2 center, Vector2 scale)
    {
        for (int i = 0; i < points.Count; i++)
        {
            Vector2 v = points[i];
            v = (v - center) / scale;
            points[i] = v;
        }
    }
    static void showPath(List<Vector2> points)
    {
        for (int i = 0; i < points.Count; i++)
        {
            Vector2 v = points[i];
            int i2 = (i + 1) % points.Count;
            Vector2 v2 = points[i2];
            Debug.DrawLine(v, v2, Color.yellow, 10);
        }
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
