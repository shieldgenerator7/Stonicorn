using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

public class MenuButton : MonoBehaviour, ISetupable
{
    public Color hoverColor = Color.white;

    public MenuFrame frame;
    [AutoInitialize(AllowUnfound =true)]
    public MenuActionButton mab;

    [AutoInitialize, SerializeField, HideInInspector]
    private BoxCollider2D bc2d;

    [SerializeField,HideInInspector]
    private List<Component> srs = new List<Component>();
    [SerializeField,HideInInspector]
    private List<Color> srOrigColors = new List<Color>();

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
        for (int i = 0; i < srs.Count; i++)
        {
            Component comp = srs[i];
            Color color;
            if (srs.Count != srOrigColors.Count)
            {
                Debug.LogError($"MenuButton {gameObject.name} has wrong number of original colors! srs count: {srs.Count}, colors count: {srOrigColors.Count}", this);
                color = (v) ? hoverColor : Color.white;
            }
            else
            {
                color = (v) ? hoverColor : srOrigColors[i];
            }
            if (comp is SpriteRenderer)
            {
                SpriteRenderer sr1 = (SpriteRenderer)comp;
                sr1.color = color;
            }
            else
            {
                Debug.LogError($"Unsupported type: {comp.GetType().Name}! Perhaps update this script to support it?", this);
            }
        }
    }

#if UNITY_EDITOR
    protected int checkForColors()
    {
        if (srs.Count != srOrigColors.Count)
        {
            Debug.LogError($"MenuButton {gameObject.name} has wrong number of original colors! srs count: {srs.Count}, colors count: {srOrigColors.Count}", this);
            return 1;
        }
        return 0;
    }
    public virtual int checkForErrorsPostSetup()
    {
        int errorCount = 0;

        if (!frame && !mab)
        {
            Debug.LogError($"MenuButton {gameObject.name} has nothing to do! {frame}, {mab}", this);
            errorCount++;
        }

        return errorCount;
    }

    public virtual int setup()
    {
        int changeCount = 0;

        //TODO: allow saying what search types to include
        int prevcount = srs?.Count ?? 0;
        srs = new List<Component>();
        srs.Add(GetComponent<SpriteRenderer>());
        srs.Add(GetComponent<SpriteShapeRenderer>());
        srs.Add(GetComponent<CanvasRenderer>());
        srs.AddRange(GetComponentsInChildren<SpriteRenderer>());
        srs.AddRange(GetComponentsInChildren<SpriteShapeRenderer>());
        srs.AddRange(GetComponentsInChildren<Image>());
        srs.AddRange(GetComponentsInChildren<TMP_Text>());
        srs.Add(GetComponent<SpriteMask>());
        srs.RemoveAll(sr => sr == null);
        if (srs.Count != prevcount)
        {
            changeCount++;
        }
        //update colors
        srOrigColors = new List<Color>();
        srs.ForEach(sr =>
        {
            if (sr is SpriteRenderer)
            {
                SpriteRenderer sr1 = (SpriteRenderer)sr;
                srOrigColors.Add(sr1.color);
            }
            else
            {
                Debug.LogError($"Unsupported type: {sr.GetType().Name}! Perhaps update this script to support it?", this);
            }
        });

        //
        return changeCount;
    }
#endif

}
