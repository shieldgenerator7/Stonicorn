using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PolygonCollider2DWorker : MonoBehaviour
{

    public List<PolygonCollider2D> editTargets;

    public void cleanTargetList()
    {
        int lastEmpty = -1;
        for (int i = 0; i < editTargets.Count; i++)
        {
            if (editTargets[i] == null)
            {
                lastEmpty = i;
            }
            else if (lastEmpty >= 0)
            {
                editTargets[lastEmpty] = editTargets[i];
                editTargets[i] = null;
                lastEmpty = -1;
                i = -1;
            }
        }
        int firstEmpty = -1;
        for (int i = 0; i < editTargets.Count; i++)
        {
            if (editTargets[i] == null)
            {
                firstEmpty = i;
                break;
            }
        }
        if (firstEmpty >= 0)
        {
            editTargets.RemoveRange(firstEmpty, editTargets.Count - firstEmpty);
            editTargets.TrimExcess();
        }
    }
}
