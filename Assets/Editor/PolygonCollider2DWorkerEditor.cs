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
        Vector2 stud = pc2d.transform.position;
        int count = 0;
        bool changes = true;
        while (changes && count < 100)
        {
            count++;
            changes = false;
            Debug.Log("BOUNDS: " + b);
            List<Vector2> points = new List<Vector2>(pc2d.GetPath(0));
            
            for (int i = 0; i < points.Count; i++)
            {
                int i2 = (i + 1) % points.Count;
                int i0 = ((i - 1) + points.Count) % points.Count;
                Debug.Log("i0: " + i0);
                //Point Checking
                if (b.Contains(points[i] + stud))
                {
                    Debug.Log("Contains point: " + points[i] + " >>" + i);
                    changes = true;
                    //add new point forward
                    Vector2 newPoint = (points[i2] - points[i])/2 + points[i];
                    points.Insert(i + 1, newPoint);
                    //move existing point backward
                    points[i] = (points[i0] - points[i]) / 2 + points[i];
                    pc2d.points = points.ToArray();
                    break;
                }
                //Line Checking
                //Debug.Log("...checking: " + (points[i] + stud) + ", " + (points[i2] + stud) + ", >>" + i);
                //Ray ray = new Ray(points[i] + stud, points[i2] - points[i] + stud);
                //if (b.IntersectRay(ray))
                //{
                //    Debug.Log("intersection: " + (points[i] + stud) + ", " + (points[i2] + stud) + ", >>" + i);
                //    //changes = true;
                //}
            }
            Debug.Log("====");
        }
    }
}
