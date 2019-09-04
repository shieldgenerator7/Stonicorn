using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class PolygonCollider2DWorker : MonoBehaviour
{

    public List<Behaviour> pc2dTargets;
    public List<Behaviour> spriteShapeTargets;

    public void cleanTargetLists()
    {
        cleanTargetList(pc2dTargets);
        cleanTargetList(spriteShapeTargets);
    }
    public void cleanTargetList(List<Behaviour> list)
    {
        int lastEmpty = -1;
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i] == null)
            {
                lastEmpty = i;
            }
            else if (lastEmpty >= 0)
            {
                list[lastEmpty] = list[i];
                list[i] = null;
                lastEmpty = -1;
                i = -1;
            }
        }
        int firstEmpty = -1;
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i] == null)
            {
                firstEmpty = i;
                break;
            }
        }
        if (firstEmpty >= 0)
        {
            list.RemoveRange(firstEmpty, list.Count - firstEmpty);
            list.TrimExcess();
        }
    }

    public void autoSelectTargetLists()
    {
        //PolygonCollider2D
        pc2dTargets.Clear();
        PolygonCollider2D pc2d = GetComponent<PolygonCollider2D>();

        foreach (PolygonCollider2D pc2dOther in GameObject.FindObjectsOfType<PolygonCollider2D>())
        {
            if (pc2d != pc2dOther && pc2d.bounds.Intersects(pc2dOther.bounds))
            {
                pc2dTargets.Add(pc2dOther);
            }
        }
        cleanTargetList(pc2dTargets);

        //SpriteShape
        spriteShapeTargets.Clear();

        foreach (SpriteShapeController ssOther in GameObject.FindObjectsOfType<SpriteShapeController>())
        {
            if (pc2d.gameObject != ssOther.gameObject
                && pc2d.bounds.Intersects(ssOther.polygonCollider.bounds))
            {
                spriteShapeTargets.Add(ssOther);
            }
        }
        cleanTargetList(spriteShapeTargets);
    }

    private void autoSelectTargetList(System.Type type, List<Component> list)
    {

    }
}
