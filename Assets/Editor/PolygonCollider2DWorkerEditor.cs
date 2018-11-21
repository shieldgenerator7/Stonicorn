using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PolygonCollider2DWorker))]
public class PolygonCollider2DWorkerEditor : Editor
{

    PolygonCollider2DWorker pc2dw;
    SpriteRenderer sr;

    private void OnEnable()
    {
        pc2dw = (PolygonCollider2DWorker)target;
        sr = pc2dw.GetComponent<SpriteRenderer>();
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        //GUI.enabled = !EditorApplication.isPlaying;
        if (GUILayout.Button("Cut PolygonCollider2D"))
        {
            //if (EditorApplication.isPlaying)
            //{
            //    throw new UnityException("You must be in Edit Mode to use this function!");
            //}
            Debug.Log("Will now cut the pc2d " + pc2dw.editTarget.name);
            cutCollider(pc2dw.editTarget, sr.bounds);
        }
    }

    public static void cutCollider(PolygonCollider2D pc2d, Bounds b)
    {
        Debug.Log("BOUNDS: " + b);
        Vector2[] points = pc2d.GetPath(0);
        Vector2 stud = pc2d.transform.position;
        for (int i = 0; i < points.Length; i++)
        {
            int i2 = (i + 1) % points.Length;
            Debug.Log("...checking: " + (points[i] + stud) + ", " + (points[i2] + stud) + ", >>" + i);
            Ray ray = new Ray(points[i] + stud, points[i2] - points[i] + stud);
            if (b.IntersectRay(ray))
            {
                Debug.Log("intersection: " + (points[i] + stud) + ", " + (points[i2] + stud) + ", >>" + i);
            }
        }
        Debug.Log("====");
    }
}
