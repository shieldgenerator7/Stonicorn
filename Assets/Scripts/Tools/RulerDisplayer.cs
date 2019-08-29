using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
[ExecuteInEditMode]
public class RulerDisplayer : MonoBehaviour
{//2018-11-27: copied from PolygonColliderPen

    public bool active = true;//true to turn it on, false to turn it off

    private Vector2 center;
    private Vector2 difference;
    private Vector2 direction;
    private float magnitude;
    private Vector2 endPos;
    private GravityZone gz = null;

    public SpriteRenderer sr;

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
            refreshGuideLines();
            HandleUtility.Repaint();
        }

        //Draw teleport range
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 3);

        //Draw world level lines
        Gizmos.color = Color.white;
        if (gz)
        {
            //draw leveler guide line
            Gizmos.DrawWireSphere(
                center,
                magnitude
                );
            //draw vertical guide line
            Gizmos.DrawLine(
                center,
                endPos
                );
        }
    }

    public void refreshGuideLines()
    {
        //Get the gravity center
        gz = GravityZone.getGravityZone(transform.position);
        if (gz)
        {
            sr.color = Color.green;
            center = gz.transform.position;
            difference = (Vector2)transform.position - center;
            direction = difference.normalized;
            magnitude = difference.magnitude;
            endPos = (Vector2)transform.position + (direction * 10);
            transform.up = direction;
        }
        else
        {
            sr.color = Color.red;
        }
    }
}
#endif
