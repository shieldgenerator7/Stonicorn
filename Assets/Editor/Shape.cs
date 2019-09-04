using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shape
{
    public Transform transform { get; private set; }
    public Vector2 position { get; private set; }
    public Vector2 scale { get; private set; }
    public Bounds bounds { get; private set; }

    private List<Vector2> points;
    private bool isInLocalSpace = true;

    public List<Vector2> LocalPoints
    {
        get
        {
            if (!isInLocalSpace)
            {
                isInLocalSpace = true;
                convertPathToLocalSpace();
            }
            return points;
        }
    }

    public List<Vector2> GlobalPoints
    {
        get
        {
            if (isInLocalSpace)
            {
                isInLocalSpace = false;
                convertPathToWorldSpace();
            }
            return points;
        }
    }

    public List<Shape> childrenShapes { get; private set; } = new List<Shape>();

    private PolygonCollider2D pc2d = null;

    public Shape(PolygonCollider2D pc2d)
    {
        this.pc2d = pc2d;
        this.transform = pc2d.transform;
        this.position = pc2d.transform.position;
        this.scale = pc2d.transform.localScale;
        this.bounds = pc2d.bounds;
        this.points = new List<Vector2>(pc2d.GetPath(0));
    }

    public void cutShape(Shape stencil, bool splitFurther = true)
    {
        //Show paths (for debugging)
        //showPath(points);
        //showPath(stencilPoints);

        //Gather overlap info
        List<IntersectionData> intersectionData = new List<IntersectionData>();
        for (int i = 0; i < GlobalPoints.Count; i++)
        {
            int i2 = (i + 1) % GlobalPoints.Count;

            //Line Checking
            LineSegment targetLine = new LineSegment(GlobalPoints, i);
            //Check to see if the bounds overlap
            if (stencil.bounds.Intersects(targetLine.Bounds))
            {
                bool startInStencil = stencil.OverlapPoint(targetLine.startPos);
                bool endInStencil = stencil.OverlapPoint(targetLine.endPos);
                //Check which stencil edges intersect the line segment
                bool intersectsSegment = false;
                for (int j = 0; j < stencil.GlobalPoints.Count; j++)
                {
                    LineSegment stencilLine = new LineSegment(stencil.GlobalPoints, j);
                    Vector2 intersection = Vector2.zero;
                    bool intersects = targetLine.Intersects(stencilLine, ref intersection);
                    //If it intersects,
                    if (intersects)
                    {
                        //Record a data point
                        intersectsSegment = true;
                        float distanceToPoint = (intersection - targetLine.startPos).magnitude;
                        IntersectionData interdata = new IntersectionData(intersection, i, j, intersects, startInStencil, endInStencil, distanceToPoint);
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

        //Sort the data entries
        intersectionData.Sort(new IntersectionData.IntersectionDataComparer());

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
        IntersectionData.printDataList(intersectionData, GlobalPoints);

        //
        //Start cutting
        //

        //Replace line segments inside the stencil
        int dataCount = intersectionData.Count;
        //Search for start of vein of changes
        List<Vein> veins = new List<Vein>();
        //only need to go through the loop once,
        //because the veins will find their own end points:
        //here we just need to find the start of each vein
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
            Vector2[] newPath = veins[0].getStencilPath(stencil.GlobalPoints);
            //Replace vein with stencil path
            int removeCount = veins[0].getRemoveCount(GlobalPoints.Count);
            replacePoints(newPath, veins[0].VeinStart + 1, removeCount);
        }
        else
        {
            //Process all the veins
            IndexOffset.IndexOffsetContainer offsets = new IndexOffset.IndexOffsetContainer(GlobalPoints.Count);
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
                    slices = vein.formsSlice(vein2, stencil.GlobalPoints.Count);
                    Debug.Log("slices: " + slices);
                    if (slices)
                    {
                        vein.slice(vein2);
                        if (splitFurther)
                        {
                            if (this.pc2d)
                            {
                                //make a new collider to make the new piece
                                PolygonCollider2D pc2dNew = GameObject.Instantiate(pc2d.gameObject)
                                    .GetComponent<PolygonCollider2D>();
                                Shape newShape = new Shape(pc2dNew);
                                newShape.rotatePoints(vein2.VeinStart);
                                newShape.finalize();
                                childrenShapes.Add(newShape);
                                pc2dNew.transform.parent = pc2d.transform.parent;
                                pc2dNew.transform.position = pc2d.transform.position;
                                newShape.cutShape(stencil, false);
                            }
                        }
                        //skip the next vein
                        i++;
                    }
                }
                if (true || !slices)
                {
                    //Replace vein with stencil path
                    Vector2[] newPath = vein.getStencilPath(stencil.GlobalPoints);
                    int removeCount = vein.getRemoveCount(GlobalPoints.Count);
                    replacePoints(newPath, vein.VeinStart + 1, removeCount);
                    //Add offset to the collection
                    IndexOffset offset = new IndexOffset(vein.VeinStart, newPath.Length - removeCount);
                    offsets.Add(offset);
                }
            }
        }

        //
        // Finish up
        //
        finalize();
    }

    public bool OverlapPoint(Vector2 point)
    {
        if (this.pc2d)
        {
            return this.pc2d.OverlapPoint(point);
        }
        PolygonCollider2D pc2d = new PolygonCollider2D();
        pc2d.SetPath(0, LocalPoints);
        return pc2d.OverlapPoint(point);
    }

    //
    // Private methods
    //

    private void finalize()
    {
        if (this.pc2d)
        {
            this.pc2d.SetPath(0, LocalPoints.ToArray());
        }
    }

    private void convertPathToWorldSpace()
    {
        for (int i = 0; i < points.Count; i++)
        {
            Vector2 v = points[i];
            v = transform.TransformPoint(v);
            points[i] = v;
        }
        isInLocalSpace = false;
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
    private void convertPathToLocalSpace()
    {
        for (int i = 0; i < points.Count; i++)
        {
            Vector2 v = points[i];
            v = transform.InverseTransformPoint(v);
            points[i] = v;
        }
        isInLocalSpace = true;
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

    /// <summary>
    /// Replaces vectors in its Global Points list for the ones in the newVectors array at the specified index
    /// </summary>
    /// <param name="newVectors"></param>
    /// <param name="index"></param>
    /// <param name="removeCount"></param>
    private void replacePoints(Vector2[] newVectors, int index, int removeCount)
    {
        insertPoints(newVectors, index);
        index += newVectors.Length;
        removePoints(index, removeCount);
    }
    /// <summary>
    /// Inserts the points given in the vectors array into its Global Points list
    /// </summary>
    /// <param name="vectors"></param>
    /// <param name="index"></param>
    private void insertPoints(Vector2[] vectors, int index)
    {
        GlobalPoints.InsertRange(index, vectors);
    }
    /// <summary>
    /// Removes the quantified vectors in its Global Points list at the given index
    /// </summary>
    /// <param name="index"></param>
    /// <param name="count"></param>
    private void removePoints(int index, int count)
    {
        for (int i = 0; i < count; i++)
        {
            if (index >= points.Count)
            {
                index = 0;
            }
            GlobalPoints.RemoveAt(index);
        }
    }

    /// <summary>
    /// Rotates the points forward or backward in the array to change the anchor point
    /// </summary>
    /// <param name="points"></param>
    /// <param name="offset"></param>
    private void rotatePoints(int newAnchor)
    {
        List<Vector2> originalPoints = new List<Vector2>(GlobalPoints.ToArray());
        int count = GlobalPoints.Count;
        for (int i = 0; i < GlobalPoints.Count; i++)
        {
            Vector2 v = originalPoints[(i + newAnchor) % count];
            GlobalPoints[i] = v;
        }
    }

    public static implicit operator PolygonCollider2D(Shape shape)
        => shape.pc2d;
}
