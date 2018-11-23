using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Used to keep track off the target's line segments while they're being moved around
/// It basically keeps track of changes made to the points array
/// </summary>
public class IndexOffset
{
    int index;//the index where things were inserted or deleted
    int count;//the amount of points inserted or deleted
    bool inserted = true;//true = points inserted, false = points deleted

    /// <summary>
    /// Makes a new IndexOffset
    /// </summary>
    /// <param name="index">the index where things were inserted or deleted</param>
    /// <param name="count">The net insertion or deletion of items. Positive = net insertion, Negative = new deletion</param>
    public IndexOffset(int index, int count)
    {
        this.index = index;
        this.inserted = (count < 0) ? false : true;
        this.count = Mathf.Abs(count);
    }

    public static int getNewIndex(List<IndexOffset> offsets, int index, int originalCount)
    {
        foreach (IndexOffset io in offsets)
        {
            if (io.inserted)
            {
                if (io.index < index)
                {
                    index += io.count;
                }
                originalCount += io.count;
            }
            else
            {
                if (io.index < index)
                {
                    index -= io.count;
                }
                else if ((io.index + io.count - 1) % originalCount < index)
                {
                    index -= (io.index + io.count - 1) % originalCount;
                }
                originalCount -= io.count;
            }
        }
        return index;
    }
}
