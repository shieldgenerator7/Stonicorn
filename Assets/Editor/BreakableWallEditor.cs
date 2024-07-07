using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CanEditMultipleObjects]
[CustomEditor(typeof(BreakableWall))]
public class BreakableWallEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GUI.enabled = !EditorApplication.isPlaying;

        if (GUILayout.Button("Autofind HiddenAreas (Editor Only)"))
        {
            foreach (Object obj in targets)
            {
                BreakableWall bw = (BreakableWall)obj;
                bw.secretHiders?.Clear();
                foreach (Collider2D c2d in bw.GetComponents<Collider2D>())
                {
                    Utility.RaycastAnswer rca = c2d.CastAnswer(Vector2.zero, 0, true);
                    for (int i = 0; i < rca.count; i++)
                    {
                        RaycastHit2D rch2d = rca.rch2ds[i];
                        HiddenArea ha = rch2d.collider.gameObject.GetComponentInParent<HiddenArea>();
                        if (ha)
                        {
                            if (!bw.secretHiders.Contains(ha))
                            {
                                bw.secretHiders.Add(ha);
                            }
                        }
                    }
                }
                EditorUtility.SetDirty(bw);
            }
        }
    }
}
