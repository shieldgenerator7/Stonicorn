using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
[ExecuteInEditMode]
public class RulerDisplayer : MonoBehaviour
{//2018-11-27: copied from PolygonColliderPen

    public bool active = true;//true to turn it on, false to turn it off

    /// <summary>
    /// Current mouse position in world coordinates
    /// Used for calling Merky in CustomMenu
    /// </summary>
    public static Vector2 currentMousePos;
    
    private void OnDrawGizmos()
    {
        //2018-11-27: copied from an answer by hazarus: https://answers.unity.com/questions/1361603/unity-editor-mouse-position-to-world-point.html
        currentMousePos = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition).origin;
        if (active)
        {
            transform.position = currentMousePos;
            HandleUtility.Repaint();
        }
    }
}
#endif
