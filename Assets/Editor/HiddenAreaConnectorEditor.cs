using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(HiddenAreaConnector))]
public class HiddenAreaConnectorEditor : Editor
{
    HiddenAreaConnector hac;

    private void OnEnable()
    {
        hac = (HiddenAreaConnector)target;
        Selection.selectionChanged += populateFields;
    }

    private void populateFields()
    {
        GameObject go = Selection.activeGameObject;
        //Hidden Area
        HiddenArea ha = go.GetComponent<HiddenArea>();
        if (!ha)
        {
            ha = go.GetComponentInParent<HiddenArea>();
        }
        if (ha)
        {
            hac.hiddenArea = ha;
        }
        //Lantern Activator
        LanternActivator la = go.GetComponent<LanternActivator>();
        if (!la)
        {
            la = go.GetComponentInParent<LanternActivator>();
        }
        if (la)
        {
            hac.lanternActivator = la;
        }
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GUI.enabled = !EditorApplication.isPlaying;
        if (GUILayout.Button("Connect (Edit Mode)"))
        {
            hac.connect();
        }
    }
}
