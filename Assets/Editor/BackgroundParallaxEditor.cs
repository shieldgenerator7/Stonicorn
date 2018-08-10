using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BackgroundParallax))]
public class BackgroundParallaxEditor : Editor {

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
    }

}
