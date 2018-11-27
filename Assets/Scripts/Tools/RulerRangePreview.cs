using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class RulerRangePreview : MonoBehaviour
{

    [Tooltip("How far Merky can teleport")]
    public float range = 3;

    [Tooltip("True to turn on while the ruler isn't moving")]
    public bool active = true;

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
            }
        }
    }
}
