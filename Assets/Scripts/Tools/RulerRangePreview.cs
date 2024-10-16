﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
[ExecuteInEditMode]
public class RulerRangePreview : MonoBehaviour
{

    [Tooltip("How far Merky can teleport")]
    public float range = 3;

    /// <summary>
    /// True to turn on while the ruler isn't moving
    /// </summary>
    private bool active = true;
    public bool Active
    {
        get { return active; }
        set
        {
            active = value;
            GetComponent<SpriteRenderer>().enabled = active;
        }
    }

    public RulerDisplayer parentRuler;

    private void OnDrawGizmos()
    {
        if (active)
        {
            if (!parentRuler.active)
            {
                Vector2 currentMousePos = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition).origin;
                Vector2 direction = (currentMousePos - (Vector2)parentRuler.transform.position);
                if (direction.sqrMagnitude > range * range)
                {
                    direction = direction.normalized * range;
                }
                transform.position = direction + (Vector2)parentRuler.transform.position;
                HandleUtility.Repaint();
            }
        }
    }

    public void callParentRuler()
    {
        if (!Active)
        {
            Active = true;
            transform.position = parentRuler.transform.position;
        }
        if (parentRuler.active)
        {
            parentRuler.active = false;
            transform.position = parentRuler.transform.position;
        }
        parentRuler.transform.position = transform.position;
        parentRuler.GetComponent<RulerDisplayer>().refreshGuideLines();
    }
}
#endif
