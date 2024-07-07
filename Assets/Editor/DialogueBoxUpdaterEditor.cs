using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DialogueBoxUpdater))]
public class DialogueBoxUpdaterEditor : Editor
{
    string testText = "";
    GUILayoutOption[] options = new GUILayoutOption[0];

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GUI.enabled = !EditorApplication.isPlaying;

        EditorGUILayout.Separator();

        string newText = EditorGUILayout.TextField("Test Text", testText, options);

        if (newText != testText)
        {
            Debug.Log($"text changed! {testText} -> {newText}");
            testText = newText;
            updateText((DialogueBoxUpdater)target);
        }
    }

    private void updateText(DialogueBoxUpdater dbu)
    {
        dbu.setText(testText);
    }
}
