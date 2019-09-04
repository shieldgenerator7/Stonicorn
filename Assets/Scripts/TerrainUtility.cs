using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public static class TerrainUtility
{
    public static float MIN_POINT_DISTANCE = 0.05f;//the closest two adjacent points can be in a sprite shape spline

    public static TerrainData convertToVectorPath(this Ferr2DT_PathTerrain ferrTerrain)
    {
        return new TerrainData(
            ferrTerrain.PathData.GetPoints(1).ToArray(),
            ferrTerrain.gameObject
            );
    }

    private static TerrainData convertToVectorPath(this SpriteShapeController ssc)
    {
        Vector2[] vectorPath = new Vector2[ssc.spline.GetPointCount()];
        for (int i = 0; i < vectorPath.Length; i++)
        {
            vectorPath[i] = ssc.spline.GetPosition(i);
        }
        return new TerrainData(
            vectorPath,
            ssc.gameObject
            );
    }

    public static void setPoints(this Spline spline, List<Vector2> points,
        bool reverseDirection = false, bool flipX = false, bool flipY = false)
    {
        spline.Clear();
        if (reverseDirection)
        {
            points.Reverse();
        }
        int skippedIndices = 0;
        for (int i = 0; i < points.Count; i++)
        {
            Vector2 v = points[i];
            Vector2 prevV = (i > 0)
                ? points[i - 1]
                : v + Vector2.one * MIN_POINT_DISTANCE * 2;
            if (Vector2.Distance(v, prevV) >= MIN_POINT_DISTANCE)
            {
                //Flip if necessary
                v.x *= (flipX) ? -1 : 1;
                v.y *= (flipY) ? -1 : 1;
                //Insert point
                try
                {
                    spline.InsertPointAt(i - skippedIndices, v);
                }
                catch (System.ArgumentException)
                {
                    spline.RemovePointAt(i - skippedIndices);
                    skippedIndices++;
                }
            }
            else
            {
                skippedIndices++;
            }
        }
    }
}
