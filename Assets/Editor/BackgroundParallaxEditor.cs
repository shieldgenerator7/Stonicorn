using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BackgroundParallax))]
public class BackgroundParallaxEditor : Editor
{

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        BackgroundParallax bp = (BackgroundParallax)target;
        if (GUILayout.Button("Set starting point"))
        {
            bp.startPos = bp.transform.position;
        }
        if (GUILayout.Button("Reset position to starting point"))
        {
            bp.transform.position = bp.startPos;
        }
        if (GUILayout.Button("Set TOP limit"))
        {
            bp.maxBounds.y = bp.transform.position.y;
        }
        if (GUILayout.Button("Set BOTTOM limit"))
        {
            bp.minBounds.y = bp.transform.position.y;
        }
        if (GUILayout.Button("Set LEFT limit"))
        {
            bp.minBounds.x = bp.transform.position.x;
        }
        if (GUILayout.Button("Set RIGHT limit"))
        {
            bp.maxBounds.x = bp.transform.position.x;
        }
        if (GUILayout.Button("Setup Default Camera Bound Collider"))
        {
            //Setup bc2d
            BoxCollider2D bc2d = bp.cameraContainerCollider;
            if (bc2d == null)
            {
                bc2d = bp.GetComponent<BoxCollider2D>();
                if (bc2d == null)
                {
                    bc2d = bp.gameObject.AddComponent<BoxCollider2D>();
                }
            }
            bc2d.isTrigger = true;
            bp.cameraContainerCollider = bc2d;
            //Find Bounds of Sprites
            Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);
            foreach (Transform t in bp.transform)
            {
                foreach (Transform tr in t.gameObject.transform)
                {
                    SpriteRenderer sr = tr.GetComponent<SpriteRenderer>();
                    if (sr != null)
                    {
                        if (bounds.size == Vector3.zero)
                        {
                            bounds = sr.bounds;
                        }
                        else
                        {
                            bounds.min = Vector3.Min(bounds.min, sr.bounds.min);
                            bounds.max = Vector3.Max(bounds.max, sr.bounds.max);
                        }
                    }
                }
            }
            //Set bc2d offset
            bc2d.offset = bounds.center - bp.transform.position;
            //Set bc2d size
            bounds.size += new Vector3(
                bp.maxBounds.x - bp.minBounds.x,
                bp.maxBounds.y - bp.minBounds.y
                );
            bc2d.size = bounds.size;
        }
    }

}
