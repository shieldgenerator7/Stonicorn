using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vein
{

    int dataStartIndex;
    int dataEndIndex;//potentially greater than the size of the array

    IntersectionData interdataStart;
    IntersectionData interdataEnd;

    int origVeinStart;//used for calculating index offsets
    int origVeinEnd;//used for calculating index offsets

    public int VeinStart
    {
        get
        {
            return interdataStart.targetLineSegmentID;
        }
        private set { }
    }
    public int VeinEnd
    {
        get
        {
            return interdataEnd.targetLineSegmentID;
        }
        private set { }
    }

    public Vein(int dataStartIndex, IntersectionData interdataStart, int dataEndIndex, IntersectionData interdataEnd)
    {
        this.dataStartIndex = dataStartIndex;
        this.interdataStart = interdataStart;
        this.dataEndIndex = dataEndIndex;
        this.interdataEnd = interdataEnd;
        this.origVeinStart = VeinStart;
        this.origVeinEnd = VeinEnd;
    }
    public Vein(int dataStartIndex, IntersectionData interdataStart)
    {
        this.dataStartIndex = dataStartIndex;
        this.interdataStart = interdataStart;
        this.origVeinStart = VeinStart;
    }
    public void update(int dataEndIndex, IntersectionData interdataEnd)
    {
        this.dataEndIndex = dataEndIndex;
        this.interdataEnd = interdataEnd;
        this.origVeinEnd = VeinEnd;
    }
    /// <summary>
    /// Give it the start of the vein and the list to look through and it'll find its own end point
    /// </summary>
    /// <param name="dataStartIndex"></param>
    /// <param name="interdataStart"></param>
    /// <param name="intersectionData"></param>
    public Vein(int dataStartIndex, IntersectionData interdataStart, List<IntersectionData> intersectionData)
        :this(dataStartIndex, interdataStart)
    {
        int dataCount = intersectionData.Count;
        //Find the end of the vein of changes
        for (int iData = dataStartIndex; iData < dataStartIndex + dataCount; iData++)
        {
            IntersectionData interdata = intersectionData[iData % dataCount];
            if (interdata.type == IntersectionData.IntersectionType.EXIT)
            {
                this.dataEndIndex = iData;
                this.interdataEnd = interdata;
                this.origVeinEnd = VeinEnd;
                break;
            }
        }
    }

    public void updateIndexes(IndexOffset.IndexOffsetContainer ioc)
    {
        interdataStart.targetLineSegmentID = ioc.getNewIndex(origVeinStart);
        interdataEnd.targetLineSegmentID = ioc.getNewIndex(origVeinEnd);
    }

    public Vector2[] getStencilPath(List<Vector2> stencilPoints)
    {
        int startIndex = interdataStart.stencilLineSegmentID;
        int endIndex = interdataEnd.stencilLineSegmentID;
        int stencilCount = stencilPoints.Count;
        Vector2[] newPath = new Vector2[(startIndex - endIndex + stencilCount) % stencilCount + 2];
        //Get the new path data
        newPath[0] = interdataStart.intersectionPoint;
        int writeIndex = 1;
        if (startIndex < endIndex)
        {
            startIndex += stencilCount;
        }
        for (int i = startIndex; i > endIndex; i--, writeIndex++)
        {
            newPath[writeIndex] = stencilPoints[(i + stencilCount) % stencilCount];
        }
        newPath[newPath.Length - 1] = interdataEnd.intersectionPoint;
        return newPath;
    }

    /// <summary>
    /// Returns how many target points need to be removed
    /// </summary>
    /// <param name="listCount"></param>
    /// <returns></returns>
    public int getRemoveCount(int listCount)
    {
        return (
            interdataEnd.targetLineSegmentID
            - interdataStart.targetLineSegmentID
            + listCount
            ) 
            % listCount;
    }

    /// <summary>
    /// Returns whether or not this vein and the given vein cut the target in two
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public bool formsSlice(Vein other, int stencilCount)
    {
        int start = interdataStart.stencilLineSegmentID;
        for (int i = start; i < start + stencilCount; i++)
        {
            if (i % stencilCount == other.interdataStart.stencilLineSegmentID % stencilCount)
            {
                Debug.Log("formsSLice: is slice");
                return true;
            }
            if (i % stencilCount == other.interdataEnd.stencilLineSegmentID % stencilCount)
            {
                Debug.Log("formsSLice: is bumps");
                return false;
            }
        }
        Debug.Log("formsSLice: default");
        return false;
    }

}
