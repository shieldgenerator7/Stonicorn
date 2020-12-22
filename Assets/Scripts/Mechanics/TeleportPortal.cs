using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportPortal : MonoBehaviour
{
    private TeleportPortal otherEnd;
    private Collider2D coll2d;

    // Start is called before the first frame update
    void OnEnable()
    {
        coll2d = GetComponent<Collider2D>();
        //Avoid being at Vector2.zero
        if ((Vector2)transform.position == Vector2.zero)
        {
            transform.position = Vector2.one * 0.001f;
        }
        //Register delegates
        Managers.Player.Teleport.findTeleportablePositionOverride += checkPortal;
    }

    private void OnDisable()
    {
        Managers.Player.Teleport.findTeleportablePositionOverride -= checkPortal;
    }

    public void connnectTo(GameObject other)
    {
        TeleportPortal tp = other.GetComponent<TeleportPortal>();
        connectTo(tp);
    }

    public void connectTo(TeleportPortal other)
    {
        otherEnd = other;
        other.otherEnd = this;
    }

    public bool containsPoint(Vector2 point)
        => coll2d.OverlapPoint(point);

    Vector2 checkPortal(Vector2 oldPos, Vector2 tapPos)
    {
        if (containsPoint(tapPos))
        {
            return otherEnd.transform.position;
        }
        return Vector2.zero;
    }
}
