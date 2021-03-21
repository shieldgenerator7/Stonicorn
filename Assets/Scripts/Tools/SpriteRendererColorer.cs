using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpriteRendererColorer : MonoBehaviour
{
    public Color color = Color.white;
    public TeleportRangeSegment segment;
    public bool colorParent = true;
    public bool colorChildren = true;
    public List<GameObject> selectedObjects = new List<GameObject>();

    private int coloredCount = 0;

    public void colorRenderers()
    {
        coloredCount = 0;
        if (segment)
        {
            color = segment.color;
        }
        if (colorParent)
        {
            selectedObjects.ForEach(go => colorRenderer(go));
        }
        if (colorChildren)
        {
            selectedObjects.ForEach(go =>
                Utility.doForGameObjectAndChildren(
                    go,
                    go2 =>
                    {
                        if (go2 != go)
                        {
                            colorRenderer(go2);
                        }
                    }
                )
            );
        }
        Debug.Log("Colored " + coloredCount + " SpriteRenderers: color: " + color);
    }

    public void colorRenderer(GameObject go)
    {
        SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
        if (sr)
        {
            sr.color = color.adjustAlpha(sr.color.a);
            coloredCount++;
        }
    }

    public void setSelectedObjs(List<GameObject> gos)
    {
        Debug.Log("selectedObjects count: " + selectedObjects.Count);
        //If any of the selected objects has a sprite renderer colorer
        if (gos.Any(go => go.GetComponent<SpriteRendererColorer>() != null))
        {
            //Don't do anything
            return;
        }
        if (gos.Count == 0)
        {
            return;
        }
        selectedObjects.Clear();
        selectedObjects.AddRange(gos);
    }
}
