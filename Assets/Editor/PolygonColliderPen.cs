using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PolygonCollider2D))]
public class PolygonColliderPen : Editor
{//2018-11-21: referenced https://docs.unity3d.com/ScriptReference/DrawGizmo.html

    PolygonCollider2D pc2d;
    public static Camera cam;

    public void OnEnable()
    {
        pc2d = (PolygonCollider2D)target;
        if (cam == null)
        {
            SceneView.onSceneGUIDelegate = grabSceneCamera;
        }
    }

    void grabSceneCamera(SceneView sceneview)
    {
        cam = sceneview.camera;
        SceneView.onSceneGUIDelegate -= grabSceneCamera;
    }

    [DrawGizmo(GizmoType.Selected | GizmoType.Active)]
    static void drawReticle(PolygonCollider2D pc2d, GizmoType gizmoType)
    {
        //2018-11-21: mousePos setting copied from https://answers.unity.com/questions/829071/get-mouse-position-in-editor-based-on-screen-coord.html
        Vector2 preMousePos = Event.current.mousePosition;
        preMousePos = new Vector2(preMousePos.x, Screen.height - preMousePos.y);
        Vector2 mousePos = cam.ScreenToWorldPoint(preMousePos);
        //mousePos = new Vector2(mousePos.x, Screen.height - mousePos.y);
        Gizmos.DrawCube(mousePos, Vector3.one*10);
        Debug.Log("Drawing cube at " + mousePos + ", size: " + Vector3.one);
    }

    private void Update()
    {
        drawReticle(pc2d, GizmoType.Active);
    }
}
