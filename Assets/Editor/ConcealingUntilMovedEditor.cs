using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ConcealingUntilMoved))]
public class ConcealingUntilMovedEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GUI.enabled = !EditorApplication.isPlaying;

        if (GUILayout.Button("Autofind HiddenAreas (Editor Only)"))
        {
            foreach (Object obj in targets)
            {
                ConcealingUntilMoved cum = (ConcealingUntilMoved)obj;
                cum.haListToUncover.Clear();
                foreach (Collider2D c2d in cum.GetComponents<Collider2D>())
                {
                    Utility.RaycastAnswer rca = c2d.CastAnswer(Vector2.zero, 0, true);
                    for (int i = 0; i < rca.count; i++)
                    {
                        RaycastHit2D rch2d = rca.rch2ds[i];
                        HiddenArea ha = rch2d.collider.gameObject.GetComponentInParent<HiddenArea>();
                        if (ha)
                        {
                            if (!cum.haListToUncover.Contains(ha))
                            {
                                cum.haListToUncover.Add(ha);
                            }
                        }
                    }
                }
                EditorUtility.SetDirty(cum);
            }
        }
    }
}
