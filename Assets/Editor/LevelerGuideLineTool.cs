using System;
using UnityEngine;
using UnityEditor;
using UnityEditor.EditorTools;
using System.Collections.Generic;

//2019-08-29: copied from PlatformTool
// Tagging a class with the EditorTool attribute and no target type registers a global tool. Global tools are valid for any selection, and are accessible through the top left toolbar in the editor.
[EditorTool("Leveler Guide Line Tool")]
class LevelerGuideLineTool : EditorTool
{
    // Serialize this value to set a default value in the Inspector.
    [SerializeField]
    Texture2D m_ToolIcon;

    GUIContent m_IconContent;

    Dictionary<Transform, Vector2> positions = new Dictionary<Transform, Vector2>();
    Vector2 center;
    Vector2 xPos;
    Vector2 yPos;
    Vector2 clickPos;
    Vector2 xPointDir = Vector2.right;
    Vector2 yPointDir = Vector2.up;

    void OnEnable()
    {
        m_IconContent = new GUIContent()
        {
            image = m_ToolIcon,
            text = "Leveler Guide Line Tool",
            tooltip = "Leveler Guide Line Tool"
        };
    }

    public override GUIContent toolbarIcon
    {
        get { return m_IconContent; }
    }

    // This is called for each window that your tool is active in. Put the functionality of your tool here.
    public override void OnToolGUI(EditorWindow window)
    {
        if (clickPos == Vector2.zero)
        {
            xPos = Tools.handlePosition;
            yPos = Tools.handlePosition;
            clickPos = Tools.handlePosition;
            yPointDir = (clickPos - center).normalized;
            xPointDir = yPointDir.PerpendicularRight();
        }
        bool mouseDown = Event.current.type == EventType.MouseDown;
        bool mouseUp = Event.current.type == EventType.MouseUp;
        if (mouseDown)
        {
            Debug.Log("CLICK");
            positions.Clear();
            foreach (Transform t in Selection.transforms)
            {
                positions.Add(t, t.position);
            }
            center = Vector2.zero;
            GravityZone gz = GravityZone.getGravityZone(Tools.handlePosition);
            if (gz)
            {
                center = gz.transform.position;
            }
            xPos = Tools.handlePosition;
            yPos = Tools.handlePosition;
            clickPos = Tools.handlePosition;
            yPointDir = (clickPos - center).normalized;
            xPointDir = yPointDir.PerpendicularRight();
        }
        if (mouseUp)
        {
            Debug.Log("UNCLICK");
            positions.Clear();
            xPos = Tools.handlePosition;
            yPos = Tools.handlePosition;
            clickPos = Tools.handlePosition;
            yPointDir = (clickPos - center).normalized;
            xPointDir = yPointDir.PerpendicularRight();
        }

        EditorGUI.BeginChangeCheck();

        using (new Handles.DrawingScope(Handles.xAxisColor))
        {
            xPos = Handles.Slider(xPos, xPointDir);
        }
        using (new Handles.DrawingScope(Handles.yAxisColor))
        {
            yPos = Handles.Slider(yPos, yPointDir);
        }

        if (EditorGUI.EndChangeCheck())
        {
            Vector2 xDir = xPos - clickPos;
            Vector2 yDir = yPos - clickPos;
            float xMag = xDir.magnitude;
            float yMag = yDir.magnitude;
            Vector2 delta = new Vector2(
                 xMag * Mathf.Sign(Vector2.Dot(xDir, xPointDir)),
                 yMag * Mathf.Sign(Vector2.Dot(yDir, yPointDir))
                );
            Debug.Log("delta: " + delta);

            Undo.RecordObjects(
                Selection.transforms,
                "Move w/ Radial Gravity"
                );

            foreach (Transform transform in Selection.transforms)
            {
                Vector2 direction = (positions[transform] - center).normalized;
                float distance = Vector2.Distance(positions[transform], center);

                //delta x
                transform.position = positions[transform].travelAlongCircle(
                    center,
                    delta.x
                    );
                direction = ((Vector2)transform.position - center).normalized;

                //delta y
                distance += delta.y;

                //composite
                transform.up = direction;
                transform.position = direction * distance + center;
            }
        }
    }   
}