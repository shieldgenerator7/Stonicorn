using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportPortal : SavableMonoBehaviour
{
    private int otherEndId;

    private TeleportPortal otherEnd;
    private Collider2D coll2d;

    public override SavableObject CurrentState
    {
        get => new SavableObject(this,
            "otherEndId", otherEndId
            );
        set
        {
            otherEndId = value.Int("otherEndId");
            if (!otherEnd)
            {
                connectTo(Managers.Object.getObject(otherEndId));
            }
        }
    }

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
        Managers.Player.findTeleportablePositionOverride += checkPortal;
    }

    private void OnDisable()
    {
        Managers.Player.findTeleportablePositionOverride -= checkPortal;
    }

    public void connectTo(GameObject other)
    {
        TeleportPortal tp = other.GetComponent<TeleportPortal>();
        connectTo(tp);
    }

    public void connectTo(TeleportPortal other)
    {
        otherEnd = other;
        otherEndId = other.GetComponent<SavableObjectInfo>().Id;
        other.otherEnd = this;
        other.otherEndId = this.GetComponent<SavableObjectInfo>().Id;
    }

    public bool containsPoint(Vector2 point)
        => coll2d.OverlapPoint(point);

    Vector2 checkPortal(Vector2 oldPos, Vector2 tapPos)
    {
        if (containsPoint(tapPos)
            && oldPos.inRange(tapPos, Managers.Player.Range)
            )
        {
            return otherEnd.transform.position;
        }
        return Vector2.zero;
    }

    public override void init()
    {
        throw new System.NotImplementedException();
    }
}
