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
                mm.compile();
                EditorUtility.SetDirty(mm);
                EditorUtility.SetDirty(mm.gameObject);
                foreach (Transform t in mm.transform)
                {
                    EditorUtility.SetDirty(t.gameObject);
                }
                mm.GetComponentsInChildren<MenuFrame>().ToList()
                    .ForEach(mf => EditorUtility.SetDirty(mf));
                mm.GetComponentsInChildren<MenuButton>().ToList()
                    .ForEach(mb => EditorUtility.SetDirty(mb));
            });
        }
    }
}
