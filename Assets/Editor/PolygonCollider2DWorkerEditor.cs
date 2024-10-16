﻿using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.U2D;

[CustomEditor(typeof(PolygonCollider2DWorker))]
public class PolygonCollider2DWorkerEditor : Editor
{
    PolygonCollider2DWorker pc2dw;
    PolygonCollider2D stencil;
    Shape stencilShape;

    static PolygonCollider2DWorker pc2dwCurrent;

    private void OnEnable()
    {
        pc2dw = (PolygonCollider2DWorker)target;
        stencil = pc2dw.GetComponent<PolygonCollider2D>();
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        GUI.enabled = !EditorApplication.isPlaying;
        if (GUILayout.Button("Auto Select PolygonCollider2Ds"))
        {
            pc2dw.autoSelectTargetLists();
        }
        if (GUILayout.Button("Cut PolygonCollider2D"))
        {
            if (EditorApplication.isPlaying)
            {
                throw new UnityException("You must be in Edit Mode to use this function!");
            }
            pc2dwCurrent = pc2dw;
            pc2dw.cleanTargetLists();
            stencilShape = new Shape(stencil);
            //PolygonCollider2D
            int originalCount = pc2dw.pc2dTargets.Count;
            for (int i = 0; i < originalCount; i++)
            {
                PolygonCollider2D pc2d = (PolygonCollider2D)pc2dw.pc2dTargets[i];
                cutCollider(new Shape(pc2d), stencilShape);
            }
            //SpriteShape
            originalCount = pc2dw.spriteShapeTargets.Count;
            for (int i = 0; i < originalCount; i++)
            {
                SpriteShapeController ssc = (SpriteShapeController)pc2dw.spriteShapeTargets[i];
                cutCollider(new Shape(ssc), stencilShape);
            }
        }
    }

    public static void cutCollider(Shape shape, Shape stencil, bool splitFurther = true)
    {
        shape.cutShape(stencil, splitFurther);
        foreach (Shape child in shape.childrenShapes)
        {
            PolygonCollider2D pc2dChild = (PolygonCollider2D)child;
            if (pc2dChild
                && !pc2dwCurrent.pc2dTargets.Contains(pc2dChild))
            {
                pc2dwCurrent.pc2dTargets.Add(pc2dChild);
            }
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




}
