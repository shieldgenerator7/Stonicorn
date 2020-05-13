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

    Dictionary<Transform, float> distances = new Dictionary<Transform, float>();
    Dictionary<Transform, Vector2> directions = new Dictionary<Transform, Vector2>();

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
        EditorGUI.BeginChangeCheck();

        Vector2 center = GravityZone.getGravityZone(Tools.handlePosition).transform.position;

        //Distances
        distances.Clear();
        foreach (Transform transform in Selection.transforms)
        {
            distances.Add(transform, Vector2.Distance(transform.position, center));
        }
        //Directions
        directions.Clear();
        foreach (Transform transform in Selection.transforms)
        {
            directions.Add(transform, ((Vector2)transform.position - center).normalized);
        }

        Vector3 position = Tools.handlePosition;
        Vector3 verticalPos = Tools.handlePosition;

        Transform targetTransform = ((GameObject)target).transform;

        using (new Handles.DrawingScope(Color.green))
        {
            position = Handles.Slider(
                position,
                targetTransform.right
                );
        }
        using (new Handles.DrawingScope(Color.red))
        {
            verticalPos = Handles.Slider(
                verticalPos,
                targetTransform.transform.up
                );
        }

        if (EditorGUI.EndChangeCheck())
        {
            Vector3 delta = position - Tools.handlePosition;
            Vector3 verticalDelta = verticalPos - Tools.handlePosition;
            float verticalMag = verticalDelta.magnitude;
            float sign =
                (
                    Vector2.Distance(targetTransform.position + verticalDelta, center)
                    >
                    Vector2.Distance(targetTransform.position, center)
                )
                ? 1
                : -1;

            Undo.RecordObjects(Selection.transforms, "Move Object along Leveler Guide Lines");

            foreach (var transform in Selection.transforms)
            {
                if (delta.magnitude != 0)
                {
                    transform.position += delta;
                    Vector2 direction = ((Vector2)transform.position - center).normalized;
                    transform.up = direction;
                    transform.position = direction * distances[transform] + center;
                }
                if (verticalDelta.magnitude != 0)
                {
                    //transform.position += verticalDelta;
                    transform.position =
                        directions[transform]
                        * (
                            distances[transform]
                            + (verticalMag * sign)
                        )
                        + center
                        ;
                    transform.up = directions[transform];
                }
            }
        }
    }
}