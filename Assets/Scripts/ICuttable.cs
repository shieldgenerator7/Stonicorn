using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICuttable
{
    bool Cuttable
    {
        get;
    }

    /// <summary>
    /// Cut this object with the line between these two points
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    void cut(Vector2 start, Vector2 end);
}
