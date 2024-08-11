using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MenuManager))]
public class MenuManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (GUILayout.Button("Init"))
        {
            targets.ToList().ForEach(target =>
            {
                MenuManager mm = target as MenuManager;
                mm.init();
                EditorUtility.SetDirty(mm);
                EditorUtility.SetDirty(mm.gameObject);
            });
        }
    }
}
