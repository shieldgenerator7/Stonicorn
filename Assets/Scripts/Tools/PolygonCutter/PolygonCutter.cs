using Ludiq.OdinSerializer.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

/// <summary>
/// Takes 2 polygons and cuts one with the other
/// </summary>
public class PolygonCutter
{
    struct IntersectionData
    {
        //Line segments are named for their first point,
        //so segment 0 is the segment between point 0 and point 1
        public int shapeSegmentIndex { get; private set; }
        public int cutterSegmentIndex { get; private set; }
        public Vector2 point { get; private set; }

        public IntersectionData(int shapeSegmentIndex, int cutterSegmentIndex, Vector2 point)
        {
            this.shapeSegmentIndex = shapeSegmentIndex;
            this.cutterSegmentIndex = cutterSegmentIndex;
            this.point = point;
        }

        public static bool operator ==(IntersectionData id1, IntersectionData id2)
            => id1.shapeSegmentIndex == id2.shapeSegmentIndex
            && id1.cutterSegmentIndex == id2.cutterSegmentIndex;
        public static bool operator !=(IntersectionData id1, IntersectionData id2)
            => id1.shapeSegmentIndex != id2.shapeSegmentIndex
            || id1.cutterSegmentIndex != id2.cutterSegmentIndex;
    }
    class IntersectionDataComparer : Comparer<IntersectionData>
    {
        private bool useShapeSegmentFirst;
        public IntersectionDataComparer(bool useShapeSegmentFirst)
        {
            this.useShapeSegmentFirst = useShapeSegmentFirst;
        }
        public override int Compare(IntersectionData id1, IntersectionData id2)
        {
            if (useShapeSegmentFirst)
            {
                int diff = id2.shapeSegmentIndex - id1.shapeSegmentIndex;
                if (diff != 0)
                {
                    //Forward Sort
                    return (int)Mathf.Sign(diff);
                }
                else
                {
                    //Reverse Sort
                    int diff2 = id2.cutterSegmentIndex - id1.cutterSegmentIndex;
                    return -(int)Mathf.Sign(diff2);
                }
            }
            else
            {
                int diff = id2.cutterSegmentIndex - id1.cutterSegmentIndex;
                if (diff != 0)
                {
                    //Forward Sort
                    return (int)Mathf.Sign(diff);
                }
                else
                {
                    //Reverse sort
                    int diff2 = id2.shapeSegmentIndex - id1.shapeSegmentIndex;
                    return -(int)Mathf.Sign(diff2);
                }
            }
        }
    }

    Comparer<IntersectionData> shapeSegmentComparer = new IntersectionDataComparer(true);
    Comparer<IntersectionData> cutterSegmentComparer = new IntersectionDataComparer(false);

    public PolygonCutter() { }

    public List<Polygon> cutPolygon(Polygon shape, Polygon cutter)
    {
        //If the two shape bounds do not overlap,
        if (!shape.bounds.Intersects(cutter.bounds))
        {
            //Nothing will be cut,
            //so just return the original shape
            return new List<Polygon>() { shape };
        }
        //Get a list of all the intersections
        List<IntersectionData> intersections = new List<IntersectionData>();
        for (int i = 0; i < shape.segments.Count; i++)
        {
            LineSegment segment = shape.segments[i];
            for (int j = 0; j < shape.segments.Count; j++)
            {
                LineSegment cutterSegment = cutter.segments[j];
                Vector2 point = Vector2.zero;
                if (segment.Intersects(cutterSegment, ref point))
                {
                    intersections.Add(
                        new IntersectionData(i, j, point)
                        );
                }
            }
        }
        intersections.Sort(shapeSegmentComparer);
        //If there are no intersections,
        if (intersections.Count == 0)
        {
            //Find first shape point not in the cutter
            int firstPointIndex = -1;
            for (int i = 0; i < shape.points.Count; i++)
            {
                Vector2 v = shape.points[i];
                if (!cutter.containsPoint(v))
                {
                    firstPointIndex = i;
                    break;
                }
            }
            //If all points are contained in the cutter,
            if (firstPointIndex < 0)
            {
                //there's nothing left after cutting,
                //return empty list
                return new List<Polygon>();
            }
            //If no points are contained in the cutter (and there's no intersections),
            else
            {
                //Nothing will be cut,
                //so just return the original shape
                return new List<Polygon>() { shape };
            }
        }

        //Now actually cut the shape
        List<IntersectionData> processedIntersections = new List<IntersectionData>();
        while(intersections.Count > 0)
        {
            //Find first intersection at beginning of cut
            IntersectionData id = intersections[0];
            foreach(IntersectionData idTest in intersections)
            {
                LineSegment segment = shape.segments[idTest.shapeSegmentIndex];
                if (!cutter.containsPoint(segment.startPos))
                {
                    id = idTest;
                    break;
                }
            }
            intersections.Remove(id);
            //Get intersection at other end of cut
            IntersectionData endID = getPrevCutterSegment(
                intersections,
                id,
                shape.points.Count,
                cutter.points.Count
                );

        }
        foreach(IntersectionData id in intersections)
        {
            if (!processedIntersections.Contains(id))
            {
                processedIntersections.Add(id);

            }
        }

        List<Polygon> polygonList = new List<Polygon>();
        return polygonList;
    }

    private IntersectionData getPrevCutterSegment(List<IntersectionData> data, IntersectionData current, int shapePointCount, int cutterPointCount)
    {
        int count = data.Count;
        int curIndex = current.cutterSegmentIndex;
        int curIndexShape = current.shapeSegmentIndex;
        return data.Aggregate<IntersectionData>(
            (curMax, id) => {
                if (id != current)
                {
                    int index = id.cutterSegmentIndex;
                    index += (index <= curIndex) ? cutterPointCount : 0;
                    int maxIndex = curMax.cutterSegmentIndex;
                    maxIndex += (maxIndex <= curIndex) ? cutterPointCount : 0;
                    if (index > maxIndex)
                    {
                        curMax = id;
                    }
                    else if (index == maxIndex)
                    {
                        int indexShape = id.shapeSegmentIndex;
                        indexShape += (indexShape < curIndexShape) ? shapePointCount : 0;
                        int maxIndexShape = curMax.shapeSegmentIndex;
                        maxIndexShape += (maxIndexShape < curIndexShape) ? shapePointCount : 0;
                        if (indexShape < maxIndexShape && indexShape >= curIndexShape)
                        {
                            curMax = id;
                        }
                    }
                }
                return curMax;
            }
            );
    }

    private IntersectionData getNextShapeSegment(List<IntersectionData> data, IntersectionData current)
    {
        int count = data.Count;
        int curIndex = current.shapeSegmentIndex;
        int minNextIndex = (curIndex + count);
        int selectIndex = -1;
        for (int i = 0; i < count; i++)
        {
            IntersectionData id = data[i];
            if (id == current)
            {
                continue;
            }
            int index = id.shapeSegmentIndex;
            index += (index < curIndex) ? count : 0;
            if (index < minNextIndex)
            {
                minNextIndex = index;
                selectIndex = i;
            }
        }
        return data[selectIndex];
    }
}
