using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
[ExecuteInEditMode]
public class RulerDisplayer : MonoBehaviour
{//2018-11-27: copied from PolygonColliderPen

    public bool active = true;//true to turn it on, false to turn it off

    GuideLineData markerPos = new GuideLineData(Color.white);
    GuideLineData cursorPos = new GuideLineData(new Color(0.75f,0.75f,0.75f));

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
            markerPos.refreshGuideLines(transform.position);
            transform.up = markerPos.direction;
        }
        else
        {
            cursorPos.refreshGuideLines(currentMousePos);
        }
        HandleUtility.Repaint();

        //Draw teleport range
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 3);
        //Draw first ForceLaunch range
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, 4.7f);

        if (!active)
        {
            cursorPos.drawGuidelines();
        }
        markerPos.drawGuidelines();
    }

    public void refreshGuideLines()
    {
        markerPos.refreshGuideLines(transform.position);
        cursorPos.refreshGuideLines(currentMousePos);
    }

    class GuideLineData
    {
        Vector2 center;
        Vector2 difference;
        public Vector2 direction;
        float magnitude;
        Vector2 endPos;
        GravityZone gz;

        Color color = Color.white;

        public GuideLineData(Color color)
        {
            this.color = color;
        }

        public void refreshGuideLines(Vector2 focalPoint)
        {
            //Get the gravity center
            gz = GravityZone.getGravityZone(focalPoint);
            if (gz)
            {
                center = gz.transform.position;
                difference = focalPoint - center;
                direction = difference.normalized;
                magnitude = difference.magnitude;
                endPos = focalPoint + (direction * 10);
            }
        }

        public void drawGuidelines()
        {
            //Draw world level lines
            Gizmos.color = color;
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
    }
}
#endif
