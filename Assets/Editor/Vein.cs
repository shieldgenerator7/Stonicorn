using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vein
{

    int dataStartIndex;
    int dataEndIndex;//potentially greater than the size of the array

    IntersectionData interdataStart;
    IntersectionData interdataEnd;

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
    }
    public Vein(int dataStartIndex, IntersectionData interdataStart)
    {
        this.dataStartIndex = dataStartIndex;
        this.interdataStart = interdataStart;
    }
    public void update(int dataEndIndex, IntersectionData interdataEnd)
    {
        this.dataEndIndex = dataEndIndex;
        this.interdataEnd = interdataEnd;
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
                break;
            }
        }
    }

    public void updateIndexes(IndexOffset.IndexOffsetContainer ioc)
    {
        interdataStart.targetLineSegmentID = ioc.getNewIndex(interdataStart.targetLineSegmentID);
        interdataEnd.targetLineSegmentID = ioc.getNewIndex(interdataEnd.targetLineSegmentID);
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

}
