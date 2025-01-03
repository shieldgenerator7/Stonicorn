using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CreaturePodAttacher))]
public class CreaturePodAttacherEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (GUILayout.Button("Convert all"))
        {
            ((CreaturePodAttacher)target).convertAll();
        }
    }
}
