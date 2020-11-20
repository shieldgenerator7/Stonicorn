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
    public Color circleColor = Color.blue;

    public int startIndex = 0;
    private int endIndex = 5;
    public int length = 5;
    public int correctHeightIndex = 0;//the index that has the correct height

    private Vector2 dragStartPosition;

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
                ssc.transform.TransformPoint(
                    ssc.spline.GetPosition(correctHeightIndex % pointCount)
                    ),
                gz.transform.position
                );
            for (int i = startIndex; i <= endIndex; i++)
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
        length = Mathf.Clamp(length, 1, ssc.spline.GetPointCount());
        endIndex = startIndex + length;
        if (endIndex < startIndex)
        {
            endIndex += ssc.spline.GetPointCount();
        }
        correctHeightIndex = Mathf.Clamp(correctHeightIndex, startIndex, endIndex);
    }

    private void OnDrawGizmos()
    {
        if (!ssc)
        {
            return;
        }
        checkEndIndex();

        //HandleUtility.Repaint();

        //Draw line thru points to be leveled
        Gizmos.color = lineColor;

        int pointCount = ssc.spline.GetPointCount();
        for (int i = startIndex; i < endIndex; i++)
        {
            Gizmos.DrawLine(
                ssc.transform.TransformPoint(ssc.spline.GetPosition(i % pointCount)),
                ssc.transform.TransformPoint(ssc.spline.GetPosition((i + 1) % pointCount))
                );
        }
        //Draw circle over correct height point
        Gizmos.color = circleColor;
        Gizmos.DrawWireSphere(
            ssc.transform.TransformPoint(
                ssc.spline.GetPosition(correctHeightIndex % pointCount)
                ),
            1
            );
        ////Mouse Selecting
        //if (Event.current.type == EventType.MouseDown)
        //{
        //    dragStartPosition = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition).origin;
        //    startIndex = int.MaxValue;
        //    endIndex = -1;
        //}
        //else if (Event.current.type == EventType.MouseUp)
        //{
        //    dragStartPosition = Vector2.zero;
        //}
        //if (dragStartPosition != Vector2.zero)
        //{
        //    startIndex = int.MaxValue;
        //    endIndex = -1;
        //    Vector2 dragPos = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition).origin;
        //    for (int i = 0; i < ssc.spline.GetPointCount(); i++)
        //    {
        //        Vector2 p = ssc.transform.TransformPoint(ssc.spline.GetPosition(i % pointCount));
        //        if (Utility.between(p.x, dragStartPosition.x, dragPos.x)
        //            && Utility.between(p.y, dragStartPosition.y, dragPos.y))
        //        {
        //            if (i < startIndex)
        //            {
        //                startIndex = i;
        //            }
        //            if (i > endIndex)
        //            {
        //                endIndex = i;
        //            }
        //        }
        //        length = endIndex - startIndex;
        //    }
        //}
    }
}
