using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuButton : MonoBehaviour
{

    public MenuFrame frame;
    public MenuActionButton mab;
    
    private BoxCollider2D bc2d;

    // Use this for initialization
    protected virtual void Start()
    {
        bc2d = GetComponent<BoxCollider2D>();
    }

    public bool tapInArea(Vector2 pos)
    {
        return bc2d.OverlapPoint(pos);
    }

    public virtual void processTap(Vector2 tapPos)
    {
        activate();
    }

    public virtual bool acceptsDragGesture()
    {
        return false;
    }

    public virtual void activate()
    {
        Debug.Log("MenuButton " + name + " pressed");
        if (frame != null)
        {
            frame.frameCamera();
        }
        if (mab != null)
        {
            mab.activate();
        }
    }
}
