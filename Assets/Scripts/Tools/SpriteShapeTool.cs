using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.U2D;

[ExecuteInEditMode]
public class SpriteShapeTool : MonoBehaviour
{
    public SpriteShapeController ssc;
    public Color lineColor = Color.green;

    public int startIndex = 0;
    private int endIndex = 5;
    public int length = 5;

    public void setSSC(Object obj)
    {
        if (obj is GameObject)
        {
            SpriteShapeController ssc = ((GameObject)obj).GetComponent<SpriteShapeController>();
            if (ssc)
            {
                this.ssc = ssc;
            }
        }
    }

    public void levelPoints()
    {
        checkEndIndex();
        if (!ssc)
        {
            Debug.LogError("SpriteShapeTool needs a SpriteShapeController to level");
            return;
        }
        GravityZone gz = GravityZone.getGravityZone(ssc.transform.position);
        if (!gz)
        {
            Debug.LogError("SpriteShapeTool: This SpriteShapeController needs to be in a GravityZone in order to be leveled");
            return;
        }
        //Radial Gravity
        if (gz.radialGravity)
        {
            int pointCount = ssc.spline.GetPointCount();
            float rValue = Vector2.Distance(
                ssc.transform.TransformPoint(ssc.spline.GetPosition(startIndex % pointCount)),
                gz.transform.position
                );
            for (int i = startIndex + 1; i <= endIndex; i++)
            {
                Vector2 newPoint = (ssc.transform.TransformPoint(ssc.spline.GetPosition(i % pointCount)) - gz.transform.position)
                    .normalized * rValue + gz.transform.position;
                ssc.spline.SetPosition(i % pointCount, ssc.transform.InverseTransformPoint(newPoint));
            }
        }
        //Straight Gravity
        else
        {
            //TODO: Make leveler for Straight Gravity 2020-04-08a
        }
    }

    private void checkEndIndex()
    {
        length = Mathf.Max(length, 1);
        endIndex = startIndex + length;
        if (endIndex < startIndex)
        {
            endIndex += ssc.spline.GetPointCount();
        }
    }

    private void OnDrawGizmos()
    {
        if (!ssc)
        {
            return;
        }
        checkEndIndex();

        //HandleUtility.Repaint();

        //Draw lien thru points to be leveled
        Gizmos.color = lineColor;

        int pointCount = ssc.spline.GetPointCount();
        for (int i = startIndex; i < endIndex; i++)
        {
            Gizmos.DrawLine(
                ssc.transform.TransformPoint(ssc.spline.GetPosition(i % pointCount)),
                ssc.transform.TransformPoint(ssc.spline.GetPosition((i + 1) % pointCount))
                );
        }
    }
}
