using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuButton : MonoBehaviour, ISetupable
{
    public Color hoverColor = Color.white;

    public MenuFrame frame;
    [AutoInitialize(AllowUnfound =true)]
    public MenuActionButton mab;

    [AutoInitialize, SerializeField, HideInInspector]
    private BoxCollider2D bc2d;

    [SerializeField]
    private List<Component> srs;

    public virtual void init()
    {

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

    public virtual void processDrag(Vector2 tapPos, GestureState state)
    {
        if (state.Finished())
        {
            activate();
        }
    }

    public virtual void activate()
    {
        frame?.frameCamera();
        mab?.activate();
    }

    internal void highlight(bool v)
    {
        Color color = (v) ? hoverColor : Color.white;
        srs.ForEach(sr =>
        {
            if (sr is SpriteRenderer)
            {
                SpriteRenderer sr1 = (SpriteRenderer)sr;
                sr1.color = color;
            }
        });
    }

    [Initializer]
    private List<Component> init_srs()
    {
        List<Component> list = new List<Component>();
        list.AddRange(GetComponentsInChildren<SpriteRenderer>());
        return list;
    }

    public int checkForErrors()
    {
        int errorCount = 0;

        if (!frame && !mab)
        {
            Debug.LogError($"MenuButton has nothing to do! {frame}, {mab}", this);
            errorCount++;
        }

        return errorCount;
    }

    public virtual int setup()
    {
        return 0;
    }

}
