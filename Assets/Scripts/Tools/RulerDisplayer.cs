using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class RulerDisplayer : MonoBehaviour
{//2018-11-27: copied from PolygonColliderPen

    public bool active = true;//true to turn it on, false to turn it off


    private void OnDrawGizmos()
    {
        //2018-11-27: copied from an answer by hazarus: https://answers.unity.com/questions/1361603/unity-editor-mouse-position-to-world-point.html
        if (active)
        {
            Vector2 currentMousePos = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition).origin;
            transform.position = currentMousePos;
        }
    }
}
