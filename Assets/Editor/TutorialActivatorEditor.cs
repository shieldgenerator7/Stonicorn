using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TutorialActivator))]
public class TutorialActivatorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        TutorialActivator ta = (TutorialActivator)target;
        if (ta.requiredTriggers == null)
        {
            ta.requiredTriggers = new List<ActivatorTrigger>();
        }
        if (ta)
        {
            //If it doesnt have a Trigger trigger yet,
            if (!ta.requiredTriggers.Any(trigger => trigger is TriggerActivatorTrigger))
            {
                //Add button to do so
                if (GUILayout.Button("Add TriggerActivatorTrigger"))
                {
                    TriggerActivatorTrigger tat = ta.gameObject.AddComponent<TriggerActivatorTrigger>();
                    ta.requiredTriggers.Add(tat);
                }
            }
            //If it doesnt have a CameraZoom trigger yet,
            if (!ta.requiredTriggers.Any(trigger => trigger is CameraZoomActivatorTrigger))
            {
                //Add button to do so
                if (GUILayout.Button("Add CameraZoomActivatorTrigger"))
                {
                    CameraZoomActivatorTrigger czat = ta.gameObject.AddComponent<CameraZoomActivatorTrigger>();
                    ta.requiredTriggers.Add(czat);
                }
            }
            //If it doesnt have a CameraPosition trigger yet,
            if (!ta.requiredTriggers.Any(trigger => trigger is CameraPositionActivatorTrigger))
            {
                //Add button to do so
                if (GUILayout.Button("Add CameraPositionActivatorTrigger"))
                {
                    CameraPositionActivatorTrigger cpat = ta.gameObject.AddComponent<CameraPositionActivatorTrigger>();
                    ta.requiredTriggers.Add(cpat);
                }
            }
        }
    }
}
